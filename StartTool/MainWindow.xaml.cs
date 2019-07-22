using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StartTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread mouseThread;
        string basePath;
        public static MainWindow instances;

        ClientManger cm;
        public MainWindow()
        {
            instances = this;
            InitializeComponent();
            InitBindEvent();
            InitLoadSetting();
            //File.ReadAllLines()
            cm = new ClientManger(IP, int.Parse(port));
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            SettingPath = basePath + SettingPath;

            if (fileName != null)
            {
                StartFunction();
            }
            mouseThread = new Thread(startGetMouse);
            mouseThread.Start();

            this.KeyDown += KeyDownEvent;

            ipBox.LostFocus += IpBox_LostFocus;
            portBox.LostFocus += PortBox_LostFocus;
        }

        private void PortBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int temp;
            if (int.TryParse(portBox.Text, out temp))
            {
                port = temp.ToString();
            }
            else
            {
                portBox.Text = port.ToString();
            }
        }

        private void IpBox_LostFocus(object sender, RoutedEventArgs e)
        {
            System.Net.IPAddress ip;
            if (System.Net.IPAddress.TryParse(ipBox.Text, out ip))
            {
                IP = ip.ToString();
            }
            else
            {
                ipBox.Text = IP.ToString();
            }
        }

        string IP = "192.168.5.102";
        string port = "54321";

        private new void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == System.Windows.Input.Keyboard.GetKeyStates(Key.F4) && System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
            }

        }

        /// <summary>
        /// 初始化绑定事件
        /// </summary>
        public void InitBindEvent()
        {
            selectFile.Click += SelectFile_Click;
            IsFrame.Checked += IsFrame_Checked;
            posBox.LostKeyboardFocus += PosBox_LostKeyboardFocus;
            argBox.LostKeyboardFocus += ArgBox_LostKeyboardFocus;
        }

        private void ArgBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Arg = argBox.Text;
        }

        /// <summary>
        /// 位置大小输入框失去焦点后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PosBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //Console.WriteLine('e');
            string[] Arr = posBox.Text.Split(',');


            if (Arr.Count() == 4)
            {
                posList.Clear();
                for (int i = 0; i < Arr.Length; i++)
                {
                    int temp = 0;
                    if (!int.TryParse(Arr[i], out temp))
                    {
                        MessageBox.Show("格式不对 输出4位");
                        posBox.Text = "";
                        posBox.Focus();
                        return;
                    }
                    else
                    {
                        posList.Add(temp);
                    }
                }
            }
            else
            {
                MessageBox.Show("格式不对");
                posBox.Focus();
            }
        }


        /// <summary>
        /// 配置文件名字
        /// </summary>
        string SettingPath = "Setting";

        /// <summary>
        /// 初始化加载配置文件
        /// </summary>
        public void InitLoadSetting()
        {
            if (File.Exists(SettingPath))
            {
                string s = File.ReadAllText(SettingPath);
                Dictionary<string, object> settingDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(s);
                FilePathStr = (string)settingDic["filePath"];
                string[] posArr = ((string)settingDic["posSize"]).Split(',');
                foreach (var item in posArr)
                {
                    posList.Add(int.Parse(item.Trim()));
                }
                isShowFrame = (bool)settingDic["isShowFrame"];
                fileName = (string)settingDic["fileName"];
                Arg = (string)settingDic["Arg"];

                IP = (string)settingDic["IP"];
                port = (string)settingDic["port"];

                ipBox.Text = IP;
                portBox.Text = port;

                argBox.Text = Arg;
                posBox.Text = string.Format("{0},{1},{2},{3}", posList[0], posList[1], posList[2], posList[3]);
                FilePath.Content = fileName;
                IsFrame.IsChecked = isShowFrame;
            }
            else
            {
                FilePath.Content = "未选择文件路径";
                posList.Add(0);
                posList.Add(0);
                posList.Add(1920);
                posList.Add(1080);
                posBox.Text = string.Format("{0},{1},{2},{3}", posList[0], posList[1], posList[2], posList[3]);
            }

        }

        /// <summary>
        /// 存储配置文件
        /// </summary>
        public void SavaSetting()
        {
            Dictionary<string, object> settingDic = new Dictionary<string, object>();
            settingDic.Add("filePath", FilePathStr);

            settingDic.Add("posSize", posBox.Text);
            settingDic.Add("isShowFrame", isShowFrame);
            settingDic.Add("fileName", fileName);
            settingDic.Add("Arg", Arg);
            settingDic.Add("IP", IP);
            settingDic.Add("port", port);

            string temp = JsonConvert.SerializeObject(settingDic);

            File.WriteAllText(SettingPath, temp);
        }

        string fileName;
        string FilePathStr = string.Empty;
        List<int> posList = new List<int>();
        bool isShowFrame;
        string Arg;


        /// <summary>
        /// 选择文件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "启动程序|*.exe";
            if (dialog.ShowDialog() == true)
            {
                fileName = dialog.SafeFileName;
                FilePathStr = dialog.FileName;
                FilePath.Content = dialog.SafeFileName;
            }
        }

        /// <summary>
        /// 是否显示边框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsFrame_Checked(object sender, RoutedEventArgs e)
        {
            isShowFrame = (bool)IsFrame.IsChecked;
        }

        /// <summary>
        /// 设置窗口的位置和大小
        /// </summary>
        /// <param name="hWnd"></param>
        private void SetWindowPosBig(IntPtr hWnd)
        {
            SetWindowPos(hWnd, -1, posList[0], posList[1], posList[2], posList[3], 1 | 4);
            SetWindowPos(hWnd, -1, posList[0], posList[1], posList[2], posList[3], 1 | 2);

            //1|4 设置位置和大小
            //1|2 强制置顶
        }

        /// <summary>
        /// 设置窗口无边框
        /// </summary>
        /// <param name="hWnd"></param>
        private void SetWindowFrame(IntPtr hWnd)
        {
            SetWindowLong(hWnd, -16, 369164288);
        }

        #region windowApi

        /// 设置程序位置
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hWndInsertAfter"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

        /// <summary>
        /// 去掉程序边框
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="nIndex"></param>
        /// <param name="dwNewLong"></param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 设置前置置顶
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="fAltTab"></param>
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 得到当前激活窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        #endregion

        IntPtr wnd;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartFunction();
        }
        Process p;
        /// <summary>
        /// 启动方法
        /// </summary>
        void StartFunction()
        {
            if (!File.Exists(FilePathStr))
            {
                //文件不存在
                MessageBox.Show("未找到指定文件");
                return;
            }
            p = new Process();
            p.StartInfo.FileName = FilePathStr;
            p.StartInfo.Arguments = Arg;
            if (p.Start())
            {
                //启动成功
                FilePath.Content = "程序启动成功";
            }
            else
            {
                //启动失败
                FilePath.Content = "程序失败";
            }
            int index = 0;
            while (p.MainWindowHandle == IntPtr.Zero)
            {
                index++;
                System.Threading.Thread.Sleep(100);
                if (index > 100)
                {
                    break;
                }
            }

            if (p.MainWindowHandle != IntPtr.Zero)
            {
                wnd = p.MainWindowHandle;
                SetForegroundWindow(wnd);
                SendMessage(wnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0); // 最大化
                if (!isShowFrame)
                {
                    SetWindowFrame(wnd);
                }
                SetWindowPosBig(wnd);
                FilePath.Content = FilePath.Content + "找到了intPtr";
            }
            else
            {
                FilePath.Content = FilePath.Content + "没找到了intPtr";
            }
            new Thread(TopThreadFunction).Start();
        }

        /// <summary>
        /// 置顶程序线程方法
        /// </summary>
        private void TopThreadFunction()
        {
            while (!isExit)
            {
                SwitchToThisWindow(wnd, true);

                if (!isShowFrame)
                {
                    SetWindowFrame(wnd);
                }

                SendMessage(wnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
                SetWindowPosBig(wnd);

                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 重启程序线程方法
        /// </summary>
        void RestThreadFunction()
        {
            p.WaitForExit();
            if (isExit)
            {
                return;
            }
            this.Dispatcher.Invoke(StartFunction);
        }

        bool isExit;
        /// <summary>
        /// 程序结束方法
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            SavaSetting();
            isExit = true;
            cm.bc.CloseSocket();
        }

        #region 拖入程序。得到路径

        private void FilePath_PreviewDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            FilePathStr = path;
            fileName = new FileInfo(path).Name;
            FilePath.Content = fileName;
        }

        private void FilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        #endregion

        #region  获得内存占用

        void GetMemory()
        {
            PerformanceCounter pf2 = new PerformanceCounter("Process", "Working Set", p.ProcessName);
            //PerformanceCounter cur = new PerformanceCounter("Process", "Working Set - Private", cur.ProcessName);
            while (!isExit)
            {
                Console.WriteLine(pf2.NextValue() / 1024 / 1024 + "mb");
                //pf2.
                Thread.Sleep(2000);
            }
        }

        #endregion


        #region 显示鼠标坐标

        public delegate void showMouse(mousePoint p);

        public void startGetMouse()
        {
            showMouse t = delegate (mousePoint p)
            {
                mousePos.Text = p.X + "--" + p.Y;
            };
            mousePoint MousePos;
            while (!isExit)
            {
                Thread.Sleep(100);
                GetCursorPos(out MousePos);
                this.Dispatcher.Invoke(t, MousePos);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out mousePoint pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct mousePoint
        {
            public int X;
            public int Y;

            public mousePoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        #endregion

        #region 屏幕点击事件

        [DllImport("User32")]
        public extern static void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);

        public enum MouseEventFlags
        {
            Move = 0x0001, //移动鼠标
            LeftDown = 0x0002,//模拟鼠标左键按下
            LeftUp = 0x0004,//模拟鼠标左键抬起
            RightDown = 0x0008,//鼠标右键按下
            RightUp = 0x0010,//鼠标右键抬起
            MiddleDown = 0x0020,//鼠标中键按下 
            MiddleUp = 0x0040,//中键抬起
            Wheel = 0x0800,
            Absolute = 0x8000//标示是否采用绝对坐标
        }

        public void MoveCursorClick(int x, int y)
        {
            SetCursorPos(x, y);//鼠标移动
            mouse_event((int)(MouseEventFlags.LeftUp | MouseEventFlags.Absolute | MouseEventFlags.LeftDown), 0, 0, 0, IntPtr.Zero);//鼠标点击
        }


        #endregion

        #region 程序最大化
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        private static extern void SetForegroundWindow(IntPtr hwnd);
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0xF060;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;

        #endregion

        //设置开机启动
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var wi = System.Security.Principal.WindowsIdentity.GetCurrent();
            var wp = new System.Security.Principal.WindowsPrincipal(wi);
            bool runAsAdmin = wp.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (!runAsAdmin)
            {
                MessageBox.Show("请以管理员模式启动");
                return;
            }

            string localPath = Process.GetCurrentProcess().MainModule.FileName;

            RegistryKey reg = Registry.LocalMachine;
            RegistryKey run = reg.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            if (run.GetValue("BootUp") == null)
            {
                try
                {
                    run.SetValue("BootUp", localPath);
                    MessageBox.Show("设置开机启动成功", "温馨提示");
                    reg.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "温馨提示");
                }
            }
            else
            {
                try
                {
                    run.DeleteValue("BootUp");
                    reg.Close();
                    MessageBox.Show("已删除开机启动", "温馨提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "温馨提示");
                }

            }
        }

        //禁用边缘
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var wi = System.Security.Principal.WindowsIdentity.GetCurrent();
            var wp = new System.Security.Principal.WindowsPrincipal(wi);
            bool runAsAdmin = wp.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            if (!runAsAdmin)
            {
                MessageBox.Show("请以管理员模式启动");
                return;
            }

            try
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\EdgeUI", true);
                rk2.SetValue("AllowEdgeSwipe", 0);
                MessageBox.Show("禁止边缘启动成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "温馨提示");
            }


        }
    }
}
