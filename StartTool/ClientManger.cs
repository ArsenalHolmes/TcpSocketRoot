﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using TcpClientRoot;
using WindowsInput;
using WindowsInput.Native;

namespace StartTool
{
    public class ClientManger
    {
        public TcpClient bc;
        public static ClientManger instaces;
        public ClientManger(string ip,int port)
        {
            new LogManger(new LogClass(), DateTime.Now.ToShortDateString().ToString() + "_log.txt");
            instaces = this;
            ToolClass.GetDataPack = GetPack;
            ToolClass.msgArrLen = 102400;
            bc = new TcpClient(new socketEvent());
            bc.Connect(ip, port);
        }

        private BaseDataPack GetPack()
        {
            return new newDataPack();
        }

        public void HandleDesktopImg()
        {
            DataPack dp = new DataPack();
            dp += (short)MsgEnum.DesktopImg;
            dp += GetDesktopScreenShot();
            bc.sendMsg(dp);
        }

        public void HandleMouseClick(DataPack dp)
        {

            MouseEventFlag flag = (MouseEventFlag)dp.ReadShort();
            mouse_event(flag, 0, 0, 0, UIntPtr.Zero);

        }
        public void HnadleMouseMove(DataPack dp)
        {
            float t_x = dp.ReadFloat();
            float t_y = dp.ReadFloat();

            int x = (int)(1920 * t_x);
            int y = (int)(1080 * t_y);

            SetCursorPos(x, y);
        }

        #region 鼠标 
        [Flags]
        enum MouseEventFlag : uint //设置鼠标动作的键值
        {
            Move = 0x0001,               //发生移动
            LeftDown = 0x0002,           //鼠标按下左键
            LeftUp = 0x0004,             //鼠标松开左键
            RightDown = 0x0008,          //鼠标按下右键
            RightUp = 0x0010,            //鼠标松开右键
            MiddleDown = 0x0020,         //鼠标按下中键
            MiddleUp = 0x0040,           //鼠标松开中键
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,              //鼠标轮被移动
            VirtualDesk = 0x4000,        //虚拟桌面
            Absolute = 0x8000
        }

        //设置鼠标位置
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        //设置鼠标按键和动作
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy,
            uint data, UIntPtr extraInfo); //UIntPtr指针多句柄类型

        #endregion

        public byte[] GetDesktopScreenShot(int wid = 1920, int hig = 1080, long Quality = 20L)
        {
            Bitmap img = new Bitmap(wid, hig);
            Graphics gc = Graphics.FromImage(img);
            gc.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(wid, hig));

            //img = resizeImage(img, new Size(1920,1080));

            var eps = new EncoderParameters(1);
            var ep = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
            eps.Param[0] = ep;
            var jpsEncodeer = GetEncoder(ImageFormat.Jpeg);
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, jpsEncodeer, eps);
                arr = ms.ToArray();

                DataPack dp = new DataPack();
                dp.WriteShort((short)MsgEnum.DesktopImg);
                dp.WriteByteArr(arr);
            }
            ep.Dispose();
            eps.Dispose();
            //Console.WriteLine(arr.Length+"--桌面长度");
            return arr;
        }


        private Bitmap resizeImage(Bitmap imgToResize, Size size)
        {
            //获取图片宽度
            int sourceWidth = imgToResize.Width;
            //获取图片高度
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //计算宽度的缩放比例
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //计算高度的缩放比例
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //期望的宽度
            int destWidth = (int)(sourceWidth * nPercent);
            //期望的高度
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //绘制图像
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return b;
        }


        public ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        FileStream fs;
        public void HandleFile(DataPack dp)
        {
            lock (bc)
            {
                string full = dp.ReadString();
                int start = dp.ReadInt();
                int end = dp.ReadInt();
                int len = dp.ReadInt();
                byte[] msg = dp.ReadByteArr();

                if (start == 0)
                {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    fs = new FileStream(Path.Combine(dir, full), FileMode.OpenOrCreate);
                }
                fs.Write(msg, 0, msg.Length);
                if (end == len)
                {
                    fs.Dispose();
                    fs.Close();
                    fs = null;
                }
            }

        }

        public void HandleKeyBD(DataPack dp)
        {
            Key k = (Key)dp.ReadShort();
            Keyboard.Type(k);
        }

        internal void HandleSpeBD(DataPack dp)
        {
            KeyBoardMsg k = (KeyBoardMsg)dp.ReadShort();
            InputSimulator sim = new InputSimulator();
            Console.WriteLine(k);
            switch (k)
            {
                case KeyBoardMsg.AltF4:
                    sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.F4);
                    break;
                case KeyBoardMsg.AltTab:
                    sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
                    break;
                case KeyBoardMsg.WinD:
                    sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);
                    break;
            }
        }
    }

    public class socketEvent : ISocketEvent
    {
        public void ClientDisconnect(TcpClient tc)
        {
            Console.WriteLine("断开连接" + tc.IsConnect);
            //tc.Connect();
        }

        public void ConnectFail(TcpClient bc)
        {
            Console.WriteLine("连接失败");
        }

        public void ConnectSuccess(TcpClient bc)
        {
            Console.WriteLine("连接成功");
            DataPack dp = new DataPack();
            dp = dp + (short)MsgEnum.ComputerName + System.Net.Dns.GetHostEntry("localhost").HostName;
            bc.sendMsg(dp);
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

    public enum MsgEnum
    {
        DesktopImg = 0,
        ComputerName,

        MouseClick = 100,
        MouseMove = 101,
        SendFiles=102,
        KeyBoard=103,
        SpeKeyBoard=104,


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
            MsgEnum me = (MsgEnum)dp.ReadShort();
            switch (me)
            {
                case MsgEnum.DesktopImg:
                    ClientManger.instaces.HandleDesktopImg();
                    break;
                case MsgEnum.MouseClick:
                    ClientManger.instaces.HandleMouseClick(dp);
                    break;
                case MsgEnum.MouseMove:
                    ClientManger.instaces.HnadleMouseMove(dp);
                    break;
                case MsgEnum.SendFiles:
                    ClientManger.instaces.HandleFile(dp);
                    break;
                case MsgEnum.KeyBoard:
                    ClientManger.instaces.HandleKeyBD(dp);
                    break;
                case MsgEnum.SpeKeyBoard:
                    ClientManger.instaces.HandleSpeBD(dp);
                    break;

            }
        }
    }

    public class LogClass : ILog
    {
        public void Error(string msg)
        {
            Console.WriteLine("error:" + msg);
        }

        public void Info(string msg)
        {
            Console.WriteLine("info:" + msg);
        }

        public void Warning(string msg)
        {
            //throw new NotImplementedException();
            Console.WriteLine("warning:" + msg);
        }
    }
}