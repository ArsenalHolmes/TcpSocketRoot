using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MessageEncoding;

namespace TcpServerRoot
{
   public class TcpClient 
    {
        Socket client;
        TcpServer server;

        public BaseDataPack DataPack { private set; get; }
        ISocketEvent socketEvent;

        public EndPoint GetEndPoint
        {
            get
            {
                if (client==null)
                {
                    return null;
                }
                return client.RemoteEndPoint;
            }
        }


        public TcpClient(Socket client,TcpServer server,BaseDataPack DataPack,ISocketEvent socketEvent)
        {
            this.client = client;
            this.server = server;
            this.DataPack = DataPack;
            DataPack.setTcpClient(this);
            msgArr = new byte[ToolClass.msgArrLen];
            this.socketEvent = socketEvent;

            if (ToolClass.SendHeaderPack)
            {
                m_HeaderThread = new Thread(HeaderThread);
                m_HeaderThread.IsBackground = true;
                m_HeaderThread.Start();
            }
        }



        public bool SendMsg(CreatePack cp, MessageType mt=MessageType.Normal)
        {
            try
            {
                if (client!=null)
                {
                    cp.Insert(DateTime.Now.ToString("hh:mm:ss"));
                    cp.Insert((int)mt);
                    byte[] msg = cp.ToArray();
                    client.SendBufferSize = msg.Length + 5;
                    client.Send(msg);
                    if (socketEvent != null) socketEvent.SendSuccessEvent(this, msg);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                if (socketEvent != null) socketEvent.SendFailEvent(this,cp.ToArray());
                Disconnect();
                return false;
            }
        }

        byte[] msgArr;

        public void BeginReceive()
        {
            try
            {
                if (client == null)
                {
                    throw new SocketException("开始接受消息client为空");
                }
                client.BeginReceive(msgArr, 0, msgArr.Length, SocketFlags.None, ReceiveCallBack, null);
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                Disconnect();
            }

        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            bool isError = false;
            try
            {
                int len = client.EndReceive(ar);
                if (len == 0) {
                    isError = true;
                    if (socketEvent != null) socketEvent.ReceiveFailEvent(this);
                    return;
                }
                byte[] newMsgArr = new byte[len];
                Buffer.BlockCopy(msgArr, 0, newMsgArr, 0, len);

                DataPack.AddMsg(newMsgArr);
                if (socketEvent != null) socketEvent.ReceiveSuccessEvent(this);
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                isError = true;
                if (socketEvent != null) socketEvent.ReceiveFailEvent(this);
            }
            finally
            {
                if (isError)
                {
                    Disconnect();
                }
                else
                {
                    BeginReceive();
                }
            }
        }

        private void Disconnect()
        {
            try
            {
                if (client != null)
                {
                    server.RemoveClient(this);
                    client.Disconnect(true);
                    client = null;
                }
                if (socketEvent != null)
                {
                    socketEvent.ClientDisconnect(server, this);
                }
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
            }

        
        }

        private void HeaderThread()
        {
            while (true)
            {
                Thread.Sleep(1000 * ToolClass.heartIntervalTime);
                CreatePack cp = new CreatePack();
                cp = cp + (int)SystemMessageType.HeartBeat;
                SendMsg(cp, MessageType.System);
            }
        }
        Thread m_HeaderThread;

        public Action<string> HeartEvent;
        public void ReceiveHeart(string time)
        {
            if (HeartEvent != null) { HeartEvent(time); }
        }
    }
}
