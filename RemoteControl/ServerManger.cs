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
        TcpServer bc;
        public static ServerManger instances;
        public ServerManger()
        {
            instances = this;
            ToolClass.GetDataPack = GetPack;
            ToolClass.outPutInfo = printInfo;
            ToolClass.isUserDataPack = true;
            ToolClass.msgArrLen = 10240000;
            bc = new TcpServer(new socketEvent());
            bc.InitServer("0.0.0.0",54321);
        }

        private void printInfo(object obj)
        {
            //Console.WriteLine();
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
                    Console.WriteLine(n);
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

        }

        public void AcceptSuccessEvent(TcpServer bs, TcpClient bc)
        {
            
        }

        public void ClientDisconnect(TcpServer ts, TcpClient tc)
        {
            //MainWindow.instances.client = null;
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
}
