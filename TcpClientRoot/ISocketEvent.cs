using System;
using System.Collections.Generic;

namespace TcpClientRoot
{
    public interface ISocketEvent
    {

        void ConnectFail( TcpClient bc);
        void ConnectSuccess(TcpClient bc);

        void ReceiveFailEvent(TcpClient bc);
        void ReceiveSuccessEvent(TcpClient bc);

        void SendSuccessEvent(TcpClient bc, byte[] msg);
        void SendFailEvent(TcpClient bc, byte[] msg);

        void ClientDisconnect(TcpClient tc);

    }
}
