#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.IO;

namespace MessageEncoding
{
    public class CreateCache
    {
        static Queue<CreatePack> pool = new Queue<CreatePack>();
        static CreateCache()
        {
            for (int i = 0; i < 100; i++)
            {
                pool.Enqueue(new CreatePack());
            }
        }


        public static void Enqueue(CreatePack pack)
        {

            if (pool.Count < 200)
                pool.Enqueue(pack);
            pack = null;
        }
        public static CreatePack Dequeue()
        {
            if (pool.Count > 0)
                pool.Dequeue();

            return new CreatePack();
        }
    }


    /// <summary>
    /// 创建包
    /// </summary>
    public class CreatePack
    {

        MemoryStream ms;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CreatePack Create() { return CreateCache.Dequeue(); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CreatePack Create(byte[] data) { return CreateCache.Dequeue().addBytes(data); }

        /// <summary>
        /// 校验值
        /// </summary>
        private int code;
        /// <summary>
        /// 
        /// </summary>
        public CreatePack()
        {
            ms = new MemoryStream();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public CreatePack(byte[] data)
        {
            addBytes(data);
        }
        /// <summary>
        /// 添加byte
        /// </summary>
        /// <param name="val"></param>
        public CreatePack addByte(byte val)
        {
            ms.WriteByte(val);
            return this;
        }

        /// <summary>
        /// 添加 byte[]
        /// </summary>
        /// <param name="val"></param>
        /// <param name="len"></param>
        public CreatePack addBytes(byte[] val, uint len = 0)
        {

            if (len > 0 && val.Length != len)
            {
                byte[] b = new byte[len];
                if (val.Length > len)
                {
                    for (int i = 0; i < len; i++)
                        b[i] = val[i];
                }
                else
                {
                    for (int i = 0; i < val.Length; i++)
                        b[i] = val[i];
                }
                addInt(b.Length);
                ms.Write(b, 0, b.Length);
                return this;
            }
            addInt(val.Length);
            ms.Write(val, 0, val.Length);
            return this;
        }

        /// <summary>
        /// 添加 short
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addBool(bool val)
        {
            addByte((byte)(val ? 0x01 : 0x00));
            return this;
        }
        /// <summary>
        /// 添加 short
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addShort(short val)
        {
            byte[] tmp = new byte[sizeof(short)];
            fixed (byte* p = &tmp[0])
                *(short*)p = val;

            addArray(tmp);
            return this;
        }

        public unsafe CreatePack addUShort(ushort val)
        {
            byte[] tmp = new byte[sizeof(ushort)];
            fixed (byte* p = &tmp[0])
                *(ushort*)p = val;

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// 添加 ushort
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addUshort(ushort val)
        {
            byte[] tmp = new byte[sizeof(ushort)];
            fixed (byte* p = &tmp[0])
                *(ushort*)p = val;

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// 添加 int
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addInt(int val)
        {
            byte[] tmp = new byte[sizeof(int)];

            fixed (byte* p = &tmp[0])
                *(int*)p = val;

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// 添加 uint
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addUint(uint val)
        {
            byte[] tmp = new byte[sizeof(uint)];
            fixed (byte* p = &tmp[0])
                *(uint*)p = val;

            addArray(tmp);
            return this;
        }
        /// <summary>
        /// 添加 float
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addFloat(float val)
        {
            byte[] tmp = new byte[sizeof(float)];
            fixed (byte* p = &tmp[0])
                *(float*)p = val;

            addArray(tmp);
            return this;
        }
        /// <summary>
        /// 添加 Int32
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addInt32(Int32 val)
        {
            byte[] tmp = new byte[sizeof(Int32)];
            fixed (byte* p = &tmp[0])
                *(Int32*)p = val;

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// 添加 Uint32
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addUInt32(UInt32 val)
        {
            byte[] tmp = new byte[sizeof(UInt32)];
            fixed (byte* p = &tmp[0])
                *(UInt32*)p = val;

            addArray(tmp);
            return this;
        }


        public unsafe CreatePack addDouble(double val)
        {
            byte[] tmp = new byte[sizeof(double)];
            fixed (byte* p = &tmp[0])
                *(double*)p = val;

            addArray(tmp);
            return this;
        }
        /// <summary>
        /// 添加 Uint64
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addInt64(Int64 val)
        {
            byte[] tmp = new byte[sizeof(Int64)];
            fixed (byte* p = &tmp[0])
                *(Int64*)p = val;

            addArray(tmp);
            return this;
        }


        /// <summary>
        /// 添加 Uint64
        /// </summary>
        /// <param name="val"></param>
        public unsafe CreatePack addUInt64(UInt64 val)
        {
            byte[] tmp = new byte[sizeof(UInt64)];
            fixed (byte* p = &tmp[0])
                *(UInt64*)p = val;

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// 添加 byteArray
        /// </summary>
        /// <param name="array"></param>
        public CreatePack addArray(byte[] array)
        {
           ms.Write(array, 0, array.Length);
           return this;
        }


        public CreatePack addString(string val)
        {

            if (string.IsNullOrEmpty(val))
            {
                addInt(0);
                return this;
            }

            byte[] array = ConvertTobyte(val);
            //addInt(array.Length);
            addBytes(array);

            return this;
        }

        /// <summary>
        /// 添加 string 
        /// 指定添加长度
        /// 长度=0 则不计算
        /// 长度不足其余补0
        /// 长度超出 超出部分在放弃
        /// </summary>
        /// <param name="val"></param>
        /// <param name="len"></param>
        public CreatePack addString(string val, ushort len)
        {
            byte[] array;
            if (val == null) array = new byte[len];
            else array = ConvertTobyte(val);

            if (len > 0)
            {
                if (array.Length > len)
                {
                    byte[] cut = new byte[len];
                    Array.Copy(array, 0, cut, 0, len);
                    addArray(array);
                    return this;
                }
                else
                {
                    byte[] b = new byte[len];

                    for (int i = 0; i < array.Length; i++)
                        b[i] = array[i];

                    addArray(b);
                    return this;
                }
            }

            addArray(array);
            return this;
        }

        /// <summary>
        /// 创建包头
        /// int 
        /// </summary>
        public CreatePack createHeader()
        {
            addInt(0);
            Random rando = new Random();
            code = rando.Next();
            addInt(code);
            return this;
        }

        /// <summary>
        /// 创建结束
        /// </summary>
        public unsafe CreatePack createOver(int pos = 0)
        {
            addInt(code);

            ms.Position = 0;
            byte[] tmp = new byte[sizeof(int)];
            fixed (byte* p = &tmp[0])
                *(int*)p = (int)(ms.Length - pos);

            addArray(tmp);
            return this;
        }

        /// <summary>
        /// string 转 byte
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static byte[] ConvertTobyte(string content)
        {
            byte[] byteStream = Pack.encoding.GetBytes(content);
            return byteStream;
        }

        public byte[] ToArray()
        {
            //byte[] data = ms.ToArray();
            int len = (int)ms.Length;
            Insert(len);
            //SplicingPack(data);
            byte[] data = ms.ToArray();

            ms.Close();
            ms.Dispose();
            return data;
        }

        /// <summary>
        /// byte 转时间
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DateTime ByteToDate(byte[] b)
        {
            int date10 = (int)BitConverter.ToUInt16(b, 0);
            int year = date10 / 512 + 2000;
            int month = date10 % 512 / 32;
            int day = date10 % 512 % 32;
            return new DateTime(year, month, day);
        }
        /// <summary>
        /// 时间转byte
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static byte[] DateToByte(DateTime date)
        {
            int year = date.Year - 2000;
            if (year < 0 || year > 127)
                return new byte[4];
            int month = date.Month;
            int day = date.Day;
            int date10 = year * 512 + month * 32 + day;
            return BitConverter.GetBytes((ushort)date10);
        }


        public unsafe CreatePack Insert(string s)
        {
            byte[] temp = ms.ToArray();
            ms.Seek(0, SeekOrigin.Begin);
            addString(s);
            SplicingPack(temp);
            return this;
        }

        public unsafe CreatePack Insert(int s)
        {
            byte[] temp = ms.ToArray();
            ms.Seek(0, SeekOrigin.Begin);
            addInt(s);
            SplicingPack(temp);
            return this;
        }

        public unsafe CreatePack SplicingPack(byte[] val)
        {
            ms.Write(val, 0, val.Length);
            return this;
        }

        public CreatePack Write<T>(T t)
        {
            if (t is int)
            {
                addInt((int)Convert.ChangeType(t, typeof(int)));
            }
            else if (t is long)
            {
                addInt64((long)Convert.ChangeType(t, typeof(long)));
            }
            else if (t is short)
            {
                addShort((short)Convert.ChangeType(t, typeof(short)));
            }
            else if (t is bool)
            {
                addBool((bool)Convert.ChangeType(t, typeof(bool)));
            }
            else if (t is string)
            {
                addString((string)Convert.ChangeType(t, typeof(string)));
            }
            else if (t is float)
            {
                addFloat((float)Convert.ChangeType(t, typeof(float)));

            }
            else if (t is byte[])
            {
                addBytes((byte[])Convert.ChangeType(t, typeof(byte[])));
            }

            return this;
        }

        public static CreatePack operator +(CreatePack dp, int i)
        {
            return dp.addInt(i);
        }
        public static CreatePack operator +(CreatePack dp, float i)
        {
            return dp.addFloat(i);
        }
        public static CreatePack operator +(CreatePack dp, bool i)
        {
            return dp.addBool(i);
        }
        public static CreatePack operator +(CreatePack dp, short i)
        {
            return dp.addShort(i);
        }
        public static CreatePack operator +(CreatePack dp, string i)
        {
            return dp.addString(i);
        }
        public static CreatePack operator +(CreatePack dp, byte[] arr)
        {
            return dp.addBytes(arr);
        }
        public static CreatePack operator +(CreatePack dp, long l)
        {
            return dp.addInt64(l);
        }




    }
}
