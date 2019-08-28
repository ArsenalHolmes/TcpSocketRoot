﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MessageEncoding;

namespace TcpClientRoot
{
    public abstract class BaseDataPack
    {
        List<byte> msgList = new List<byte>();

        protected TcpClient bc;
        public void AddMsg(byte[] msg)
        {
            lock (msgList)
            {
                for (int i = 0; i < msg.Length; i++)
                {
                    msgList.Add(msg[i]);
                }
            }
            HandleMsg();
        }

        public void setTcpClient(TcpClient bc)
        {
            this.bc = bc;
        }

        private void HandleMsg()
        {
            lock (msgList)
            {
                if (msgList.Count < 4)
                {
                    return;
                }
                byte[] arr;
                using (MemoryStream ms = new MemoryStream(msgList.ToArray()))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int len = br.ReadInt32();
                        int oLen = (int)(ms.Length - ms.Position);
                        if (len > oLen || len == 0) { return; }

                        arr = br.ReadBytes(len);
                        msgList.Clear();
                        msgList.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));
                        msgRead(arr);
                        if (msgList.Count > 4) { HandleMsg(); }
                    }
                }
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
            string time = dp.getString();
            SystemMessageType smt = (SystemMessageType)dp.getInt();
            switch (smt)
            {
                case SystemMessageType.HeartBeat:
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
