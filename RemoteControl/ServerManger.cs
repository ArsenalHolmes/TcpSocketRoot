using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpServerRoot;

namespace RemoteControl
{

    public class ServerManger
    {
        public TcpServer bc { private set; get; }
        public static ServerManger instances;
        public ServerManger()
        {
            instances = this;
            new LogManger(new LogClass(), AppDomain.CurrentDomain.BaseDirectory,"log.txt");

            ToolClass.GetDataPack = GetPack;
            ToolClass.msgArrLen = 102400;
            bc = new TcpServer(new socketEvent());
            bc.InitServer("0.0.0.0",54321);
        }



        private BaseDataPack GetPack()
        {
            return new newDataPack();
        }
    }

    public enum MsgEnum
    {
        DesktopImg = 0,
        ComputerName,

        MouseClick = 100,
        MouseMove = 101,
        SendFiles = 102,
        KeyBoard = 103,
        SpeKeyBoard = 104,


    }

    public enum KeyBoardMsg
    {
        AltF4,
        AltTab,
        WinD,
    }
    public class newDataPack : BaseDataPack
    {
        public override void UserMsgRead(byte[] msg)
        {

        }

        public override void UserMsgRead(DataPack dp)
        {
            short s = dp.ReadShort();
            MsgEnum me = (MsgEnum)s;
            //Console.WriteLine(dp.Msg.Length);
            switch (me)
            {
                case MsgEnum.DesktopImg:
                    if (MainWindow.instances.ControlDic.ContainsKey(bc))
                    {
                        MainWindow.instances.ControlDic[bc].RushDesktopImg(dp.ReadByteArr());
                    }
                    break;
                case MsgEnum.ComputerName:
                    string n = dp.ReadString();
                    if (MainWindow.instances.ClientNameDic.ContainsKey(bc)==false)
                    {
                      
                        MainWindow.instances.ClientNameDic.Add(bc, n);
                        MainWindow.instances.RushClientList();
                    }
                   
                    break;
                default:
                    break;
            }
        }
    }

    public class socketEvent : ISocketEvent
    {
        public void AcceptFailEvent(TcpServer bs, TcpClient bc)
        {
            LogManger.Instance.Info("连接失败"+bc.GetEndPoint);
        }

        public void AcceptSuccessEvent(TcpServer bs, TcpClient bc)
        {
            LogManger.Instance.Info("连接成功"+bc.GetEndPoint);

        }

        public void ClientDisconnect(TcpServer ts, TcpClient tc)
        {
            //MainWindow.instances.client = null;
            LogManger.Instance.Info("断开连接" + tc.GetEndPoint);

            if (MainWindow.instances.ClientNameDic.ContainsKey(tc))
            {
                MainWindow.instances.ClientNameDic.Remove(tc);
                MainWindow.instances.RushClientList();
            }

            if (MainWindow.instances.ControlDic.ContainsKey(tc))
            {
                MainWindow.instances.ControlDic[tc].ThreadClose();
            }


        }

        public void InitFailEvent(TcpServer bs)
        {
        }

        public void InitSuccessEvent(TcpServer bs)
        {
        }

        public void ReceiveFailEvent(TcpClient bc)
        {
        }

        public void ReceiveSuccessEvent(TcpClient bc)
        {
        }

        public void SendFailEvent(TcpClient bc, byte[] msg)
        {
        }

        public void SendSuccessEvent(TcpClient bc, byte[] msg)
        {
        }
    }

    public class LogClass : ILog
    {
        public void Error(string msg)
        {
            Console.WriteLine("error:"+msg);
        }

        public void Info(string msg)
        {
            Console.WriteLine("info:"+msg);
        }

        public void Warning(string msg)
        {
            //throw new NotImplementedException();
            Console.WriteLine("warning:"+msg);
        }
    }
}

