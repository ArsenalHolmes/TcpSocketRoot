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
            t.Start();

            this.Closed += Window_Closed;
            this.KeyDown += KeyDownEvent;
            this.PreviewKeyDown += ClientControl_PreviewKeyDown;

            this.DesktopImg.MouseDown += ImgMouseDownEvent;
            this.DesktopImg.MouseUp += ImgMouseUpEvent;
            this.DesktopImg.MouseMove += ImgMouseMoveEvent;

            bar.Visibility = Visibility.Hidden;
        }

        private void ClientControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.F4) && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                DataPack spe = new DataPack();
                spe = spe + (short)MsgEnum.SpeKeyBoard + (short)KeyBoardMsg.AltF4;
                client.SendMsg(spe);

            }

            if (Keyboard.Modifiers == ModifierKeys.Alt && e.KeyStates == Keyboard.GetKeyStates(Key.Tab))
            {
                e.Handled = true;
                DataPack spe = new DataPack();
                spe = spe + (short)MsgEnum.SpeKeyBoard + (short)KeyBoardMsg.AltTab;
                client.SendMsg(spe);
            }

            if (Keyboard.Modifiers == ModifierKeys.Windows && e.KeyStates == Keyboard.GetKeyStates(Key.D))
            {
                e.Handled = true;
                DataPack spe = new DataPack();
                spe = spe + (short)MsgEnum.SpeKeyBoard + (short)KeyBoardMsg.WinD;
                client.SendMsg(spe);
            }


            short s = (short)e.Key;
            DataPack dp = new DataPack();
            dp = dp + (short)MsgEnum.KeyBoard + s;
            client.SendMsg(dp);
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {


        }

        public void ThreadClose()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.Close();
            });
        }

        #region 文件拖入
        private void FilePath_PreviewDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            //FileInfo fi = new FileInfo(path);

            string fullName = Path.GetFileName(path);
            //文件名 起始位 终止位 整体长度 内容
            FileStream fs = new FileStream(path, FileMode.Open);

            int readLen=102400;
            int startIndex =0;
            int endIndex = 0;
            int len = (int)fs.Length;
            while (fs.Length != fs.Position)
            {
                startIndex = (int)fs.Position;
                int tempLen = (int)(fs.Length - fs.Position < readLen ? fs.Length - fs.Position : readLen);
                byte[] arr = new byte[tempLen];
                fs.Read(arr, 0, tempLen);
                endIndex = (int)fs.Position;

                DataPack dp = new DataPack();
                dp = dp + (short)MsgEnum.SendFiles + fullName + startIndex + endIndex + len;
                dp.WriteByteArr(arr);


                //bar.Dispatcher.BeginInvoke(new ProgressBarSetter(SetProgressBar), ((float)endIndex / (float)len) * 100f);

                client.SendMsg(dp);
                Thread.Sleep(1);
            }
            fs.Dispose();
            fs.Close();
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


            DataPack dp = new DataPack();
            dp += (short)MsgEnum.MouseMove;
            dp += (float)x;
            dp += (float)y;
            client.SendMsg(dp);
        }

        private void ImgMouseUpEvent(object sender, MouseButtonEventArgs e)
        {
            DataPack dp = new DataPack();
            dp += (short)MsgEnum.MouseClick;
            dp += (short)MouseEventFlag.LeftUp;
            client.SendMsg(dp);
        }

        private void ImgMouseDownEvent(object sender, MouseButtonEventArgs e)
        {

            DataPack dp = new DataPack();
            dp += (short)MsgEnum.MouseClick;
            dp += (short)MouseEventFlag.LeftDown;
            client.SendMsg(dp);
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
        bool isExit;

        private void Window_Closed(object sender, EventArgs e)
        {
            isExit = true;
            MainWindow.instances.CloseControl(client);
        }

        public void DesktopThread()
        {
            while (isExit == false)
            {
                //Thread.Sleep(MainWindow.instances.RushNumber);
                Thread.Sleep(100);
                Console.WriteLine("qwq"+client);
                if (client != null)
                {
                    DataPack dp = new DataPack();
                    dp += (short)MsgEnum.DesktopImg;
                    client.SendMsg(dp);
                }
            }
        }

        public void RushDesktopImg(byte[] msg)
        {
            Console.WriteLine(msg.Length);
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
                Console.WriteLine(value);
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
            //显示百分比
            //label1.Content = (value / pbDown.Maximum) * 100 + "%";
        }
    }
}
