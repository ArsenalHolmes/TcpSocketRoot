using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpServerRoot
{
    public class TcpServer
    {
        public TcpServer(ISocketEvent socketEvent= null,int maxConnectCount=5)
        {
            if (ToolClass.outPutInfo == null)
            {
                throw new SocketException("outPutInfo为空");
            }
            if (ToolClass.GetDataPack == null)
            {
                throw new SocketException("GetDataPack为空");
            }
            this.socketEvent = socketEvent;
            this.maxConnectCount = maxConnectCount;
        }
        Socket server;
        int maxConnectCount;
        ISocketEvent socketEvent;

        #region ClientList

        List<TcpClient> ClientList = new List<TcpClient>();

        public void AddClient(TcpClient client)
        {
            lock (ClientList)
            {
                ClientList.Add(client);
            }
        }

        public void RemoveClient(TcpClient client)
        {
            if (ClientList.Contains(client))
            {
                ClientList.Remove(client);
            }
        }

        public void MainThreadFunction()
        {
            //foreach (var item in ClientList)
            //{
            //    item.
            //}
        }

        #endregion

        public void InitServer(string ip, int port)
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
                server.Bind(point);
                server.Listen(maxConnectCount);

                server.BeginAccept(AcceptCallBack, null);
                if (socketEvent != null) socketEvent.InitSuccessEvent(this);
            }
            catch (Exception e)
            {
                //ToolClass.printInfo(e);
                LogManger.Instance.Error(e);
                if (socketEvent != null) socketEvent.InitFailEvent(this);
            }
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            TcpClient tc = null;
            try
            {
                Socket client = server.EndAccept(ar);

                tc = new TcpClient(client,this,ToolClass.GetDataPack(),socketEvent);
                AddClient(tc);
                tc.BeginReceive();
                if (socketEvent != null) socketEvent.AcceptSuccessEvent(this, tc);

            }
            catch (Exception e)
            {
                //ToolClass.printInfo(e);
                LogManger.Instance.Error(e);
                if (socketEvent != null) socketEvent.AcceptFailEvent(this, tc);
            }
            finally
            {
                server.BeginAccept(AcceptCallBack, null);
            }
        }

        void HeadThread()
        {
            Thread.Sleep(ToolClass.heartIntervalTime * 1000);
            lock (ClientList)
            {
                //foreach (var item in ClientList)
                //{
                    
                //}
            }
        }

    }

    public enum MessageType
    {
        System = 0,
        Normal = 1,
    }

    public enum SystemMessageType
    {
        HeartBeat,
    }
}
