using System;
using System.Collections.Generic;
using System.Text;

namespace TcpServerRoot
{
    public class SocketException : ApplicationException
    {
        private string error;
        private Exception innerException;
        public SocketException()
        {

        }
        public SocketException(string message) : base(message)//调用基类的构造器
        {

        }

        public SocketException(string msg, Exception innerException) : base(msg)
        {
            this.innerException = innerException;
            this.error = msg;
        }
    }
}
