using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpServerRoot
{
   public class TcpClient 
    {
        Socket client;
        TcpServer server;

        BaseDataPack DataPack;
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
        }



        public bool SendMsg(DataPack dp, MessageType mt=MessageType.Normal)
        {
            try
            {
                if (client!=null)
                {
                    dp.InsertHead((short)mt);
                    byte[] msg = dp.HaveLengthMsgArr;
                    client.SendBufferSize = msg.Length + 5;
                    client.Send(msg);
                    if (socketEvent != null) socketEvent.SendSuccessEvent(this, dp.Msg);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                if (socketEvent != null) socketEvent.SendFailEvent(this,dp.Msg);
                Disconnect();
                return false;
            }
        }

        byte[] msgArr;

        public void BeginReceive()
        {

            if (client == null)
            {
                return;
            }


            client.BeginReceive(msgArr, 0, msgArr.Length, SocketFlags.None, ReceiveCallBack, null);
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            bool isError = false;
            try
            {
                int len = client.EndReceive(ar);
                if (len == 0) {
                    Disconnect();
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
                ToolClass.printInfo(e);
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
            if (client!=null)
            {
                client.Disconnect(true);
                client = null;
            }
            if (socketEvent!=null)
            {
                socketEvent.ClientDisconnect(server, this);
            }
        
        }

        int NumberOfHeartFail;//心跳失效次数
        public void SendHeart()
        {

            if (client==null||NumberOfHeartFail>=ToolClass.MaxNumberHeartFail)
            {
                Disconnect();
                return;
            }

            DataPack dp = new DataPack();
            dp += (short)SystemMessageType.HeartBeat;
            SendMsg(dp, MessageType.System);
            NumberOfHeartFail++;
        }

        public void ReceiveHeart()
        {
            //NumberOfHeadFail = 0;
        }
    }
}
