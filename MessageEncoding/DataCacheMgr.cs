#pragma warning disable CS1591
using System.Collections.Generic;

namespace MessageEncoding
{
    /// <summary>
    /// 缓存数据管理
    /// </summary>
    public class DataCacheMgr
    {

        static object Look = new object();

        static Dictionary<string, List<byte>> cache = new Dictionary<string, List<byte>>();

        public static void AddCache(string socket)
        {
            lock (Look)
            {
                if (!cache.ContainsKey(socket))
                    cache.Add(socket, new List<byte>());

            }
        }

        public static void RemoveCache(string socket)
        {
            lock (Look)
            {


                if (cache.ContainsKey(socket))
                {
                    cache[socket].Clear();
                    cache[socket] = null;
                    cache.Remove(socket);
                }
            }
        }

        public static bool ReceiveData(string socket, byte[] data,int datalen, out byte[] packetData)
        {
            lock (Look)
            {
                if (!cache.ContainsKey(socket))
                    AddCache(socket);

                var ls = cache[socket];

                if (data != null)
                {
                    for (int i = 0; i < datalen; i++)
                        ls.Add(data[i]);
                }                

                int len = GetInt(ls);

                if (len <= 0)
                {
                    packetData = null;
                    return false;
                }


                if (len <= ls.Count)
                {
                    packetData = new byte[len];
                    ls.CopyTo(0, packetData, 0, len);
                    ls.RemoveRange(0, len);
                    return true;
                }

                packetData = null;
                return false;
            }
        }

        /// <summary>
        /// 获得 uint
        /// </summary>
        /// <returns></returns>
        public static int GetInt(List<byte> data)
        {
            if (data.Count < 4)
                return 0;
            return data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
        }
    }
}
