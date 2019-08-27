using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MessageEncoding;

namespace TcpClientRoot
{
    public abstract class BaseDataPack
    {
        MemoryStream ms = new MemoryStream();

        protected TcpClient bc;
        public void AddMsg(byte[] msg)
        {

            lock (ms)
            {
                //msgList.AddRange(msg);
                ms.Seek(ms.Length, SeekOrigin.End);
                ms.Write(msg, 0, msg.Length);
            }
            HandleMsg();
        }

        public void setTcpClient(TcpClient bc)
        {
            this.bc = bc;
        }

        private void HandleMsg()
        {
            lock (ms)
            {
                if (ms.Length < 4)
                {
                    return;
                }
                byte[] arr;
                //using (BinaryReader br = new BinaryReader(ms))
                //{
                //    int len = br.ReadInt32();
                //    int oLen = (int)(ms.Length - ms.Position);
                //    if (len > oLen) return;
                //    arr = br.ReadBytes(len);

                //    ms = new MemoryStream(br.ReadBytes((int)(ms.Length - ms.Position)));
                //}
                ms.Seek(0, SeekOrigin.Begin);
                BinaryReader br = new BinaryReader(ms);
                int len = br.ReadInt32();
                int oLen = (int)(ms.Length - ms.Position);
                if (len > oLen) return;
                arr = br.ReadBytes(len);

                //ms = new MemoryStream(br.ReadBytes((int)(ms.Length - ms.Position)));
                byte[] newArr = br.ReadBytes((int)(ms.Length - ms.Position));
                Thread.Sleep(50);
                ms = new MemoryStream();
                if (newArr.Length != 0)
                {
                    ms.Write(newArr, 0, newArr.Length);
                }

                msgRead(arr);
                if (ms.Length > 0) { HandleMsg(); }
            }
        }

        /// <summary>
        /// 区分系统消息和用户消息
        /// </summary>
        /// <param name="arr"></param>
        public void msgRead(byte[] msg)
        {

            ParsePack dp = new ParsePack(msg);
            MessageType mt = (MessageType)dp.getInt();
            switch (mt)
            {
                case MessageType.System:
                    MainThreadFunctionQueue.Enqueue(() => {
                        SystemMsgRead(dp);
                    });
                    break;
                case MessageType.Normal:
                    MainThreadFunctionQueue.Enqueue(() => {
                        UserMsgRead(dp);
                    });
                    break;
            }
        }
        #region 主线程调用方法
        Queue<Action> MainThreadFunctionQueue = new Queue<Action>();

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

        /// <summary>
        /// 系统消息处理
        /// </summary>
        /// <param name="red"></param>
        public void SystemMsgRead(ParsePack dp)
        {
            //string time = dp.ReadString();
            string time = dp.getString();
            SystemMessageType smt = (SystemMessageType)dp.getInt();
            switch (smt)
            {
                case SystemMessageType.HeartBeat:
                    Console.WriteLine(bc+"--"+time);
                    bc.ReceiveHeart(time);
                    break;
            }
        }

        /// <summary>
        /// 用户消息处理
        /// </summary>
        /// <param name="msg"></param>
        public virtual void UserMsgRead(byte[] msg) { }

        public abstract void UserMsgRead(ParsePack dp);

    }
}
