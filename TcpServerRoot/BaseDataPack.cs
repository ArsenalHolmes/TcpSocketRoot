using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MessageEncoding;

namespace TcpServerRoot
{
    public abstract class BaseDataPack
    {

        MemoryStream ms;

        

        public long MsgListCount
        {
            get
            {
                return ms.Length;
            }
        }

        protected TcpClient bc;
        public void AddMsg(byte[] msg)
        {
            lock (ms)
            {
                //msgList.AddRange(msg);
                ms.Write(msg, 0, msg.Length);
            }
            HandleMsg();
        }

        public void setTcpClient(TcpClient bc)
        {
            this.bc = bc;
            ms = new MemoryStream();
        }

        private void HandleMsg()
        {
            lock (ms)
            {
                if (ms.Length<4)
                {
                    return;
                }

                byte[] arr;
                ms.Seek(0, SeekOrigin.Begin);

                BinaryReader br = new BinaryReader(ms);

                int len = br.ReadInt32();
                int oLen = (int)(ms.Length - ms.Position);
                if (len > oLen) return;
                arr = br.ReadBytes(len);

                byte[] newArr = br.ReadBytes((int)(ms.Length - ms.Position));
                ms = new MemoryStream();
                if (newArr.Length!=0)
                {
                    ms.Write(newArr, 0, newArr.Length);
                }
               

                msgRead(arr);

                //if (ms.Length > 0) { HandleMsg(); }
            }
        }

        //长度-类型-时间-消息类型-内容
        //4-4-*-4-长度
        /// <summary>
        /// 区分系统消息和用户消息
        /// </summary>
        /// <param name="arr"></param>
        public void msgRead(byte[] msg)
        {
            ParsePack pp = new ParsePack(msg);

            MessageType mt = (MessageType)pp.getInt();

            switch (mt)
            {
                case MessageType.System:
                    MainThreadFunctionQueue.Enqueue(() => {
                        SystemMsgRead(pp);
                    });
                    break;
                case MessageType.Normal:
                    MainThreadFunctionQueue.Enqueue(()=> {
                        UserMsgRead(pp);
                    });
                    break;
            }
        }

        /// <summary>
        /// 系统消息处理
        /// </summary>
        /// <param name="red"></param>
        public void SystemMsgRead(ParsePack dp)
        {
            string s = dp.getString();
            SystemMessageType smt = (SystemMessageType)dp.getInt();
            Console.WriteLine(s+"-"+smt);
            switch (smt)
            {
                case SystemMessageType.HeartBeat:
                    bc.ReceiveHeart(s);
                    break;
            }
        }

        /// <summary>
        /// 用户消息处理
        /// </summary>
        /// <param name="msg"></param>
        public virtual void UserMsgRead(byte[] msg) { }

        public abstract void UserMsgRead(ParsePack dp);

        #region 主线程调用方法

        Queue<Action> MainThreadFunctionQueue=new Queue<Action>();

        public void HandMainThreadFunctio()
        {
            try
            {
                while (MainThreadFunctionQueue.Count > 0)
                {
                    MainThreadFunctionQueue.Dequeue()();
                }
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
            }
        }

        #endregion

    }
}
