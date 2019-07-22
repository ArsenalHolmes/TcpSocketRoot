using System;
using System.Collections.Generic;
using System.IO;

namespace TcpServerRoot
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
            printInfo("没有对应的键值对");
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
            printInfo("没有value值");
        }

        public delegate BaseDataPack dataPackDelegate();
        public static dataPackDelegate GetDataPack;

        public static int msgArrLen = 1024;
        public static bool isUserDataPack = true;

        public static int heartIntervalTime=5;//心跳间隔时间
        public static int MaxNumberHeartFail=5;

        public static Action<object> outPutInfo;
        public static void printInfo(Exception e)
        {
            if (outPutInfo!=null)
            {
                outPutInfo(e.StackTrace+"--"+e.Message);
            }
        }
        public static void printInfo(string error)
        {
            if (outPutInfo != null)
            {
                outPutInfo(error);
            }
        }


    }
}
