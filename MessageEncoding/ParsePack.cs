#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageEncoding
{


    public class ParsePackCache
    {

        static Queue<ParsePack> pool = new Queue<ParsePack>();

        static ParsePackCache()
        {
            for (int i = 0; i < 100; i++)
            {
                pool.Enqueue(new ParsePack());
            }
        }


        public static void Enqueue(ParsePack pack)
        {
            pack.Reset();
            if (pool.Count < 200)
                pool.Enqueue(pack);

            pack = null;
        }
        public static ParsePack Dequeue()
        {
            if (pool.Count > 0)
                pool.Dequeue();

            return new ParsePack();
        }

    }

    /// <summary>
    /// 解包 创建包 编码
    /// </summary>
    public class Pack
    {
        /// <summary>
        /// 解包编码
        /// </summary>
        public static Encoding encoding = Encoding.UTF8;
    }

    /// <summary>
    /// 解析包
    /// </summary>
    public class ParsePack
    {

        private int m_nIndex = 0;
        private byte[] data;

    
        public static ParsePack Create(byte[] data)
        {
            return new ParsePack(data);
        }

  
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="index"></param>
        public void MoveTo(int index)
        {
            m_nIndex = index;
        }

        /// <summary>
        /// 获得所有byte 数据
        /// </summary>
        /// <returns></returns>
        public byte[] getBytesAll()
        {
            return data;
        }

        /// <summary>
        /// 获得现在位置
        /// </summary>
        public int Position
        {
            get { return m_nIndex; }
        }

        public int Len
        {
            get { return data.Length; }
        }

        /// <summary>
        /// 解析包
        /// </summary>
        /// <param name="b"></param>
        public ParsePack(byte[] b)
        {
            data = b;
            m_nIndex = 0;
        }

        public ParsePack SetData(byte[] b)
        {
            data = b;

            return this;
        }
        public ParsePack()
        {
            data = null;
            m_nIndex = 0;
        }

        public void Reset()
        {
            m_nIndex = 0;
            data = null;
        }
        
        /// <summary>
        /// 获得 byte
        /// </summary>
        /// <returns></returns>
        public byte getByte()
        {
            if (m_nIndex >= data.Length)
                return 0x00;

            return data[m_nIndex++];
        }


        /// <summary>
        /// 获得剩下所有byte
        /// </summary>
        /// <param name="endPos"></param>
        /// <returns>剩余数量</returns>
        public byte[] getSurplusBytes(int endPos = 0)
        {
            byte[] b = new byte[data.Length - m_nIndex - endPos];
            for (int i = 0; i < b.Length; i++)            
                b[i] = data[m_nIndex++];
            
            return b;
        }
        /// <summary>
        /// 获得 byte[]
        /// </summary>
        /// <param name="leng"></param>
        /// <returns></returns>
        public byte[] getBytes(int leng)
        {

            if (m_nIndex + leng > data.Length)
            {
                return null;
            }
            byte[] b = new byte[leng];

            for (int i = 0; i < leng; i++)
                b[i] = data[m_nIndex++];

            return b;
        }
        /// <summary>
        /// 获得 ushort
        /// </summary>
        /// <returns></returns>
        public unsafe short getShort()
        {
            if (m_nIndex + 2 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                short val = *(short*)p;
                m_nIndex += 2;
                return val;
            }
        }
        /// <summary>
        /// 获得 ushort
        /// </summary>
        /// <returns></returns>
        public unsafe ushort getUShort()
        {
            if (m_nIndex + 2 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                ushort val = *(ushort*)p;
                m_nIndex += 2;
                return val;
            }
        }

        /// <summary>
        /// 获得 uint
        /// </summary>
        /// <returns></returns>
        public unsafe uint getUInt()
        {
            if (m_nIndex + 4 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                uint val = *(uint*)p;
                m_nIndex += 4;
                return val;
            }
        }
        /// <summary>
        /// 获得 int
        /// </summary>
        /// <returns></returns>
        public unsafe int getInt()
        {
            if (m_nIndex + 4 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                int val = *(int*)p;
                m_nIndex += 4;
                return val;
            }
        }

        /// <summary>
        /// Int32
        /// </summary>
        /// <returns></returns>
        public unsafe Int32 getInt32()
        {
            if (m_nIndex + 4 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                Int32 val = *(Int32*)p;
                m_nIndex += 4;
                return val;
            }
        }

        public unsafe Int64 getInt64()
        {
            if (m_nIndex + 8 > data.Length)
                return 0;
            fixed (byte* p = &data[m_nIndex])
            {
                Int64 val = *(Int64*)p;
                m_nIndex += 8;
                return val;
            }
        }
        /// <summary>
        /// 获得 Uint32
        /// </summary>
        /// <returns></returns>
        public unsafe UInt32 getUInt32()
        {
            if (m_nIndex + 4 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                UInt32 val = *(UInt32*)p;
                m_nIndex += 4;
                return val;
            }
        }

        public unsafe double getDouble()
        {
            int size = sizeof(double);
            if (m_nIndex + size > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                double val = *(double*)p;
                m_nIndex += size;
                return val;
            }
        }
        public unsafe long getLong()
        {
            int size = sizeof(long);
            if (m_nIndex + size > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                long val = *(long*)p;
                m_nIndex += size;
                return val;
            }
        }
        /// <summary>
        /// 获得 ushort
        /// </summary>
        /// <returns></returns>
        public unsafe ulong getUlong()
        {
            int size = sizeof(ulong);
            if (m_nIndex + size > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                ulong val = *(ulong*)p;
                m_nIndex += size;
                return val;
            }
        }

        /// <summary>
        /// 获得 UInt64
        /// </summary>
        /// <returns></returns>
        public unsafe UInt64 getUInt64()
        {
            if (m_nIndex + 8 > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                UInt64 val = *(UInt64*)p;
                m_nIndex += 8;
                return val;
            }
        }

        /// <summary>
        /// 获得 float
        /// </summary>
        /// <returns></returns>
        public unsafe float getFloat()
        {
            if (m_nIndex + sizeof(float) > data.Length)
                return 0;

            fixed (byte* p = &data[m_nIndex])
            {
                float val = *(float*)p;
                m_nIndex += sizeof(float);
                return val;
            }
        }

        /// <summary>
        /// 获得字符串 取前四个字节为长度
        /// </summary>
        /// <returns></returns>
        public string getString()
        {
            int len = getInt32();
            return getString(len);
        }


        /// <summary>
        /// 获得 string
        /// </summary>
        /// <param name="nCount"></param>
        /// <returns></returns>
        public string getString(int nCount)
        {
            if (m_nIndex + 1 > data.Length)
                return string.Empty;


            byte[] array = new byte[nCount];

            for (int i = 0; i < nCount; i++)
                array[i] = data[m_nIndex++];

            string val = ConvertToString(array);

            return val;
        }

        /// <summary>
        /// byte[] 转 string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToString(byte[] value)
        {
            char[] Chars = new char[Pack.encoding.GetCharCount(value, 0, value.Length)];
            Pack.encoding.GetChars(value, 0, value.Length, Chars, 0);
            return new string(Chars);
        }
     
        /// <summary>
        /// string 转 byte[]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static char[] ConvertToCahr(byte[] value)
        {
            char[] Chars = new char[Pack.encoding.GetCharCount(value, 0, value.Length)];
            Pack.encoding.GetChars(value, 0, value.Length, Chars, 0);
            return Chars;
        }



      
    }
}
