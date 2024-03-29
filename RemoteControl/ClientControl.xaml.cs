﻿using MessageEncoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TcpServerRoot;
using Path = System.IO.Path;

namespace RemoteControl
{
    /// <summary>
    /// ClientControl.xaml 的交互逻辑
    /// </summary>
    public partial class ClientControl : Window
    {
        Thread t;
        TcpClient client;
        
        public ClientControl(TcpClient tc)
        {
            client = tc;
            InitializeComponent();

            t = new Thread(DesktopThread);
            t.IsBackground = true;
            t.Start();

            this.Closed += Window_Closed;
            this.PreviewKeyDown += ClientControl_PreviewKeyDown;

            //this.DesktopImg.MouseDown += ImgMouseDownEvent;
            //this.DesktopImg.MouseUp += ImgMouseUpEvent;
            //this.DesktopImg.MouseMove += ImgMouseMoveEvent;

            bar.Visibility = Visibility.Hidden;
        }

        private void ClientControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyStates == Keyboard.GetKeyStates(Key.F4) && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                CreatePack spe = new CreatePack();
                spe = spe + (int)MsgEnum.SpeKeyBoard + (int)KeyBoardMsg.AltF4;
                client.SendMsg(spe);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt && e.KeyStates == Keyboard.GetKeyStates(Key.Tab))
            {
                e.Handled = true;
                CreatePack spe = new CreatePack();
                spe = spe + (int)MsgEnum.SpeKeyBoard + (int)KeyBoardMsg.AltTab;
                client.SendMsg(spe);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Windows && e.KeyStates == Keyboard.GetKeyStates(Key.D))
            {
                e.Handled = true;
                CreatePack spe = new CreatePack();
                spe = spe + (int)MsgEnum.SpeKeyBoard + (int)KeyBoardMsg.WinD;
                client.SendMsg(spe);
            }
            else
            {
                int s = (int)e.Key;
                CreatePack dp = new CreatePack();
                dp = dp + (int)MsgEnum.KeyBoard + s;
                client.SendMsg(dp);
            }

        }


        public void ThreadClose()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.Close();
            });
        }

        #region 文件拖入

        Thread sendThread;
        public int pos;
        public int len;
        void SendThreadFunc(object obj)
        {
            string path = (string)obj;
            FileStream fs = new FileStream(path, FileMode.Open);
            string fullName = Path.GetFileName(path);

            int readLen = 102400;
            int startIndex = 0;
            int endIndex = 0;
            len = (int)fs.Length;
            while (fs.Length != fs.Position)
            {
                startIndex = (int)fs.Position;
                int tempLen = (int)(fs.Length - fs.Position < readLen ? fs.Length - fs.Position : readLen);
                byte[] arr = new byte[tempLen];
                fs.Read(arr, 0, tempLen);
                pos = (int)fs.Position;
                endIndex = (int)fs.Position;

                CreatePack dp = new CreatePack();
                // 消息头 文件名  开始位置 结束位置 总长度 内容
                dp = dp + (int)MsgEnum.SendFiles + fullName + startIndex + endIndex + len + arr;
                client.SendMsg(dp);
            }
            fs.Dispose();
            fs.Close();
        }

        private void FilePath_PreviewDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            sendThread = new Thread(new ParameterizedThreadStart(SendThreadFunc));
            sendThread.Start(path);

            SendFrame sf = new SendFrame(this);
            sf.ShowDialog();

            //string fullName = Path.GetFileName(path);
            //文件名 起始位 终止位 整体长度 内容
            //FileStream fs = new FileStream(path, FileMode.Open);

            //int readLen=102400;
            //int startIndex =0;
            //int endIndex = 0;
            //int len = (int)fs.Length;
            //while (fs.Length != fs.Position)
            //{
            //    startIndex = (int)fs.Position;
            //    int tempLen = (int)(fs.Length - fs.Position < readLen ? fs.Length - fs.Position : readLen);
            //    byte[] arr = new byte[tempLen];
            //    fs.Read(arr, 0, tempLen);
            //    endIndex = (int)fs.Position;

            //    CreatePack dp = new CreatePack();
            //    // 消息头 文件名  开始位置 结束位置 总长度 内容
            //    dp = dp + (int)MsgEnum.SendFiles + fullName + startIndex + endIndex + len+arr;
            //    client.SendMsg(dp);
            //}
            //fs.Dispose();
            //fs.Close();
        }

        private void FilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        #endregion

        #region 鼠标事件
        private void ImgMouseMoveEvent(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(DesktopImg);
            double wid = DesktopImg.ActualWidth;
            double hig = DesktopImg.ActualHeight;
            double x = p.X / wid;
            double y = p.Y / hig;


            //DataPack dp = new DataPack();
            CreatePack dp = new CreatePack();
            dp += (int)MsgEnum.MouseMove;
            dp += (float)x;
            dp += (float)y;
            client.SendMsg(dp);
        }

        private void ImgMouseUpEvent(object sender, MouseButtonEventArgs e)
        {
            
            if (e.LeftButton == MouseButtonState.Released)
            {
                //DataPack dp = new DataPack();
                CreatePack dp = new CreatePack();
                dp += (int)MsgEnum.MouseClick;
                dp += (int)MouseEventFlag.LeftUp;
                client.SendMsg(dp);
            }
            //else if (e.RightButton == MouseButtonState.Released)
            //{
            //    DataPack dp = new DataPack();
            //    dp += (short)MsgEnum.MouseClick;
            //    dp += (short)MouseEventFlag.RightUp;
            //    LogManger.Instance.Info("右键抬起");
            //    //client.SendMsg(dp);
            //}

        }

        private void ImgMouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton==MouseButtonState.Pressed)
            {
                //DataPack dp = new DataPack();
                CreatePack dp = new CreatePack();
                dp += (int)MsgEnum.MouseClick;
                dp += (int)MouseEventFlag.LeftDown;
                client.SendMsg(dp);
                //LogManger.Instance.Info("左键按下");
            }
            else if (e.RightButton==MouseButtonState.Pressed)
            {
                //DataPack dp = new DataPack();
                CreatePack dp = new CreatePack();
                dp += (int)MsgEnum.MouseClick;
                dp += (int)MouseEventFlag.RightDown;
                client.SendMsg(dp);
                //LogManger.Instance.Info("右键按下");

                Thread.Sleep(10);
                //DataPack dp2 = new DataPack();
                CreatePack dp2 = new CreatePack();
                dp2 += (int)MsgEnum.MouseClick;
                dp2 += (int)MouseEventFlag.RightUp;
                //LogManger.Instance.Info("右键抬起");
                client.SendMsg(dp2);
            }

        }

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

        #endregion
        public bool isExit;

        private void Window_Closed(object sender, EventArgs e)
        {
            isExit = true;
            MainWindow.instances.CloseControl(client);
        }

        public void DesktopThread()
        {
            while (isExit == false)
            {
                Thread.Sleep(100);
                if (client != null)
                {
                    CreatePack dp = new CreatePack();
                    dp += (short)MsgEnum.DesktopImg;
                    bool b = client.SendMsg(dp);
                }
            }
        }

        public void RushDesktopImg(byte[] msg)
        {
            //LogManger.Instance.Info("图片长度" + msg.Length);
            
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(msg);
                bmp.EndInit();
                DesktopImg.Source = bmp;
            });
        }

        public void RushBar(float value)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                if (value >= 90)
                {
                    bar.Visibility = Visibility.Hidden;

                }
                else
                {
                    bar.Value = value;
                    bar.Visibility = Visibility.Visible;
                }
            });
        }

        public delegate void ProgressBarSetter(double value);
        public void SetProgressBar(double value)
        {
            //显示进度条
            bar.Value = value;
            bar.Visibility = Visibility.Visible;
            if (value==100)
            {
                bar.Visibility = Visibility.Hidden;
            }
        }
    }
}
