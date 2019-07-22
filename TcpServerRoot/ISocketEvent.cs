using System;
using System.Collections.Generic;

namespace TcpServerRoot
{
    public interface ISocketEvent
    {
        void InitFailEvent(TcpServer bs);
        void InitSuccessEvent(TcpServer bs);

        void AcceptFailEvent(TcpServer bs, TcpClient bc);
        void AcceptSuccessEvent(TcpServer bs, TcpClient bc);

        void ReceiveFailEvent(TcpClient bc);
        void ReceiveSuccessEvent(TcpClient bc);

        void SendSuccessEvent(TcpClient bc, byte[] msg);
        void SendFailEvent(TcpClient bc, byte[] msg);

        void ClientDisconnect(TcpServer ts, TcpClient tc);

    }
}
