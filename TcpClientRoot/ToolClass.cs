using System;
using System.Collections.Generic;
using System.IO;

namespace TcpClientRoot
{
    public static class ToolClass
    {
        public static Tkey dicGetKeyByValue<Tkey,Tvalue>(Dictionary<Tkey,Tvalue> dic,Tvalue value)
        {
            foreach (var item in dic)
            {
                if (item.Value.Equals(value))
                {
                    return item.Key;
                }
            }
            return default(Tkey);
        }

        public static void dicRemoveByValue<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic, Tvalue value)
        {
            Tkey temp=default(Tkey);
            foreach (var item in dic)
            {
                if (item.Value.Equals(value))
                {
                    temp = item.Key;
                }
            }
            if (temp!=null)
            {
                dic.Remove(temp);
                return;
            }
        }

        public delegate BaseDataPack dataPackDelegate();
        public static dataPackDelegate GetDataPack;

        public static int msgArrLen = 1024;

        public static int heartIntervalTime=5;//心跳间隔时间
        public static int MaxNumberHeartFail=5;
        public static bool SendHeaderPack=true;//发送心跳包




    }
}
