using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread main = Thread.CurrentThread;
            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine(DateTime.Now.ToString());
                i++;
                LogManger.Instance.Info(DateTime.Now.ToString()+"--"+i+"--info");
            }
        }
    }
}
