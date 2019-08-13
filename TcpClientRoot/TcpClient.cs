using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpClientRoot
{

    public enum MessageType
    {
        System = 0,
        Normal = 1,
    }

    public enum SystemMessageType
    {
        HeartBeat,
    }
    public class TcpClient
    {
        protected Socket client;
        protected byte[] msgArr = new byte[ToolClass.msgArrLen];

        protected ISocketEvent socketEvent;

        public BaseDataPack dataPack { get; protected set; }

        bool isConnect;

        public bool IsConnect
        {
            get
            {
                return isConnect;
            }
        }

        public TcpClient(ISocketEvent socketEvent = null) 
        {
            this.socketEvent = socketEvent;
            dataPack = ToolClass.GetDataPack();
            ConnectThread = new Thread(ConnectThreadFunc);
            ConnectThread.IsBackground = true;
            ConnectThread.Start();
        }

        bool isEnd;
        ~TcpClient()
        {
            isEnd = true;
        }

        Thread ConnectThread;

        void ConnectThreadFunc()
        {
            while (isEnd == false)
            {
                Thread.Sleep(1000);
                if (IsConnect==false)
                {
                    Connect();
                }
            }
        }

        Thread m_HeaderThread;

        IPEndPoint point;
        public void Connect(string ip, int port)
        {
            if (isConnect)
            {
                return;
            }
            point = new IPEndPoint(IPAddress.Parse(ip), port);
            Connect();
        }

        public void Connect()
        {
            if (isConnect)
            {
                return;
            }
            if (point==null)
            {
                throw new SocketException("point为空");
            }
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(point);
                client.BeginReceive(msgArr, 0, ToolClass.msgArrLen, SocketFlags.None, ReceiveCallBack, null);
                isConnect = true;
                if (socketEvent != null) socketEvent.ConnectSuccess(this);
                if (ToolClass.SendHeaderPack)
                {
                    m_HeaderThread = new Thread(HeaderThread);
                    m_HeaderThread.IsBackground = true;
                    m_HeaderThread.Start();
                }

            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                if (socketEvent != null) socketEvent.ConnectFail(this);
                client = null;
                isConnect = false;
            }
        }


        public Action<string> HeartEvent;
        public void ReceiveHeart(string time)
        {
            if (HeartEvent != null) { HeartEvent(time); }
        }

        public bool SendMsg(DataPack dp, MessageType mt = MessageType.Normal)
        {

            if (isConnect==false)
            {
                Connect();
            }

            try
            {
                dp.InsertHead(DateTime.Now.ToString("hh:mm:ss"));
                dp.InsertHead((short)mt);
                byte[] msg = dp.HaveLengthMsgArr;
                client.SendBufferSize = msg.Length + 5;
                    
                client.Send(msg);
                if (socketEvent != null) socketEvent.SendSuccessEvent(this, dp.Msg);
                return true;
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                Disconnect();
                if (socketEvent != null) socketEvent.SendFailEvent(this, dp.Msg);
                return false;
            }

        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            bool isError = false;
            
            //if (isConnect == false)
            //{
            //    Connect();
            //}

            if (isConnect)
            {
                try
                {
                    int len = client.EndReceive(ar);
                    if (len == 0)
                    {
                        Disconnect();
                        if (socketEvent != null) socketEvent.ReceiveFailEvent(this);
                        isError = true;
                        return;
                    }
                    byte[] newMsgArr = new byte[len];
                    Buffer.BlockCopy(msgArr, 0, newMsgArr, 0, len);

                    
                    dataPack.AddMsg(newMsgArr);

                    if (socketEvent != null) socketEvent.ReceiveSuccessEvent(this);
                }
                catch (Exception e)
                {
                    isError = true;
                    //ToolClass.printInfo(e);
                    LogManger.Instance.Error(e);
                    if (socketEvent != null) socketEvent.ReceiveFailEvent(this);
                }
                finally
                {
                    if (!isError)
                    {
                        client.BeginReceive(msgArr, 0, ToolClass.msgArrLen, SocketFlags.None, ReceiveCallBack, null);
                    }
                    else
                    {
                        Disconnect();
                    }
                }
            }
            
        }

        private void Disconnect()
        {
            try
            {
                if (client != null)
                {
                    if (client.Connected)
                    {
                        client.Disconnect(true);
                    }
                    client = null;
                }
                socketEvent.ClientDisconnect(this);
                isConnect = false;
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
            }
            finally
            {
                isConnect = false;
                m_HeaderThread.Abort();
            }

        }

        public void CloseSocket()
        {
            Disconnect();
            isEnd = true;
        }

        private void HeaderThread()
        {
            while (isConnect==true)
            {
                Thread.Sleep(1000 * ToolClass.heartIntervalTime);
                DataPack dp = new DataPack();
                dp = dp + (short)SystemMessageType.HeartBeat;
                SendMsg(dp, MessageType.System);
            }
        }
    }
}
