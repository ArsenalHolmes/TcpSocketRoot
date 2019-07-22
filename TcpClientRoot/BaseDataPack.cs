using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace TcpClientRoot
{
    public abstract class BaseDataPack
    {
        List<byte> msgList = new List<byte>();

        protected TcpClient bc;
        public void AddMsg(byte[] msg)
        {
            msgList.AddRange(msg);
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
                //TODO 处理
                byte[] arr;
                using (MemoryStream ms = new MemoryStream(msgList.ToArray()))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int len = br.ReadInt32();
                        long oLen = ms.Length - ms.Position;
                        if (len > oLen) return;
                        arr = br.ReadBytes(len);

                        msgList.Clear();
                        msgList.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));
                    }
                }
                //msgRead(arr); //多线程处理消息
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    msgRead(arr);
                });
                if (msgList.Count > 0) HandleMsg();
            }
        }

        /// <summary>
        /// 区分系统消息和用户消息
        /// </summary>
        /// <param name="arr"></param>
        public void msgRead(byte[] msg)
        {
            DataPack dp = new DataPack(msg);
            MessageType mt = (MessageType)dp.ReadShort();
            switch (mt)
            {
                case MessageType.System:
                    SystemMsgRead(dp);
                    break;
                case MessageType.Normal:
                    if (ToolClass.isUserDataPack)
                    {
                        UserMsgRead(dp);
                    }
                    else
                    {
                        UserMsgRead(dp.Msg);
                    }
                    break;
            }
        }

        /// <summary>
        /// 系统消息处理
        /// </summary>
        /// <param name="red"></param>
        public void SystemMsgRead(DataPack dp)
        {
            SystemMessageType smt = (SystemMessageType)dp.ReadShort();
            switch (smt)
            {
                case SystemMessageType.HeartBeat:
                    //bc.ReceiveHeart();
                    break;
            }
        }

        /// <summary>
        /// 用户消息处理
        /// </summary>
        /// <param name="msg"></param>
        public abstract void UserMsgRead(byte[] msg);

        public abstract void UserMsgRead(DataPack dp);

        public int MsgDecompose(ref byte[] msg, int len = 4)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(msg))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int i = br.ReadInt32();
                        byte[] arr = br.ReadBytes((int)(ms.Length - ms.Position));
                        msg = arr;
                        return i;
                    }
                }
            }
            catch (Exception e)
            {
                ToolClass.printInfo("MsgDecomposeError" + e.StackTrace);
                throw;
            }

        }
    }

    public class DataPack
    {
        byte[] msg;

        public byte[] HaveLengthMsgArr
        {
            get
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(Count);
                        bw.Write(Msg);
                        return ms.ToArray();
                    }
                }
            }
        }


        public byte[] Msg
        {
            get
            {
                if (msg != null)
                {
                    return msg;
                }
                msg = new byte[0];
                return msg;
            }
            private set
            {
                msg = value;
            }
        }
        public int Count
        {
            get
            {
                return Msg.Length;
            }
        }

        public DataPack(byte[] msg=null)
        {
            this.Msg = msg;
        }

        //头部插入short
        public DataPack InsertHead(short s)
        {
            byte[] arr = BitConverter.GetBytes(s);

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    int len = arr.Length;
                    bw.Write(len);
                    bw.Write(arr);

                    bw.Write(Msg);
                    Msg = ms.ToArray();
                    return this;
                }
            }

        }

        #region 读写 5个类型 string float short int bool DataPack byteArr
        public string ReadString()
        {
            try
            {
                byte[] temp = ReadByteArr();
                string s = Encoding.UTF8.GetString(temp);
                return s;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return "";
            }

        }
        public DataPack WriteString(string str)
        {
            try
            {
                byte[] temp = Encoding.UTF8.GetBytes(str);
                return WriteByteArr(temp);
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return this;
            }
        }

        public float ReadFloat()
        {
            try
            {
                byte[] arr = ReadByteArr();
                float f = BitConverter.ToSingle(arr, 0);
                return f;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return -1;
            }
        }
        public DataPack WriteFloat(float f)
        {
            byte[] arr = BitConverter.GetBytes(f);
            return WriteByteArr(arr);
        }

        public short ReadShort()
        {
            try
            {
                byte[] arr = ReadByteArr();
                short f = BitConverter.ToInt16(arr, 0);
                return f;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return -1;
            }
        }
        public DataPack WriteShort(short f)
        {
            byte[] arr = BitConverter.GetBytes(f);
            return WriteByteArr(arr);
        }
        public short LookShort()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Msg))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int len = br.ReadInt32();
                        byte[] temp = br.ReadBytes(len);
                        short f = BitConverter.ToInt16(temp, 0);
                        return f;
                    }
                }
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return -1;
            }
        }

        public int ReadInt()
        {
            try
            {
                byte[] arr = ReadByteArr();
                int f = BitConverter.ToInt32(arr, 0);
                return f;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return -1;
            }
        }
        public DataPack WriteInt(int f)
        {
            byte[] arr = BitConverter.GetBytes(f);
            return WriteByteArr(arr);
        }

        public bool ReadBool()
        {
            try
            {
                byte[] arr = ReadByteArr();
                bool f = BitConverter.ToBoolean(arr, 0);
                return f;
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return false;
            }
        }
        public DataPack WriteBool(bool b)
        {
            byte[] arr = BitConverter.GetBytes(b);
            
            return WriteByteArr(arr);
        }


        public DataPack WriteDataPack(DataPack dp)
        {
            return WriteByteArr(dp.Msg);
        }

        public DataPack ReadDataPack()
        {
            byte[] msg = ReadByteArr();
            DataPack dp = new DataPack(msg);
            return dp;
        }


        public byte[] ReadByteArr()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(Msg))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        int len = br.ReadInt32();
                        byte[] temp = br.ReadBytes(len);
                        byte[] arr = br.ReadBytes((int)(ms.Length - ms.Position));
                        Msg = arr;
                        return temp;
                    }
                }
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return null;
            }

        }
        public DataPack WriteByteArr(byte[] Arr)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {

                        int len = Arr.Length;
                        bw.Write(Msg);
                        bw.Write(len);
                        bw.Write(Arr);
                        Msg = ms.ToArray();

                        
                        return this;
                    }
                }
            }
            catch (Exception e)
            {
                ToolClass.printInfo(e);
                return this;
            }
        }


        #endregion

        #region 加法重载

        public static DataPack operator +(DataPack dp, int i)
        {
            return dp.WriteInt(i);
        }
        public static DataPack operator +(DataPack dp, float i)
        {
            return dp.WriteFloat(i);
        }
        public static DataPack operator +(DataPack dp, bool i)
        {
            return dp.WriteBool(i);
        }
        public static DataPack operator +(DataPack dp, short i)
        {
            return dp.WriteShort(i);
        }
        public static DataPack operator +(DataPack dp, string i)
        {
            return dp.WriteString(i);
        }

        public static DataPack operator +(DataPack dp, DataPack i)
        {
            return dp.WriteDataPack(i);
        }

        public static DataPack operator+(DataPack dp,byte[] arr)
        {
            return dp.WriteByteArr(arr);
        }

        #endregion
    }
}
