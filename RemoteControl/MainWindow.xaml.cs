using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Windows.Threading;
using TcpServerRoot;
using Image = System.Drawing.Image;

namespace RemoteControl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instances;

        public ServerManger server;

        public Dictionary<TcpClient, ClientControl> ControlDic = new Dictionary<TcpClient, ClientControl>();
        public Dictionary<TcpClient, string> ClientNameDic = new Dictionary<TcpClient, string>();

        bool isExit=false;
        Thread thread;
        public MainWindow()
        {

            instances = this;
            this.Closed += CloseEvent;
            InitializeComponent();
            server = new ServerManger();

            rushNumber.LostFocus += RushNumber_LostFocus;
            RushNumber = 100;
            rushNumber.Text = RushNumber.ToString();

            thread = new Thread(ThreadFunction);
            thread.Start();

        }

        private void CloseEvent(object sender, EventArgs e)
        {
            isExit = true;
            LogManger.Instance.SaveLog();
            List<TcpClient> keys = new List<TcpClient>(ControlDic.Keys);
            foreach (var item in keys)
            {
                CloseControl(item);
            }
        }

        private void ThreadFunction()
        {
            while (isExit==false)
            {
                Thread.Sleep(100);
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate () {
                    ServerManger.instances.bc.MainThreadFunction();
                });
            }
        }

        private void RushNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            int temp;
            if (int.TryParse(rushNumber.Text,out temp)&&temp>50)
            {
                RushNumber = temp;
            }
            else
            {
                rushNumber.Text = RushNumber.ToString(); ;
            }
        }

        public int RushNumber;

        public void RushClientList()
        {

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                clientBox.ItemsSource = null;

                List<ModelItem> mi = new List<ModelItem>();
                foreach (var item in ClientNameDic)
                {

                    ModelItem temp = new ModelItem(item.Value, item.Key);
                    mi.Add(temp);
                }

                clientBox.ItemsSource = mi;//ClientNameDic.Values;
                
                
                clientBox.MouseDoubleClick += ClientBox_MouseDoubleClick;
            });

        }

        private void ClientBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ModelItem mi = (ModelItem)clientBox.SelectedValue;
            ShowControl(mi.Client);
        }

        public void ShowControl(TcpClient tc)
        {
            if (ControlDic.ContainsKey(tc))
            {
                return;
            }
            ClientControl con = new ClientControl(tc);
            con.Show();
            ControlDic.Add(tc, con);
        }

        public void CloseControl(TcpClient tc)
        {
            try
            {
                if (ControlDic.ContainsKey(tc))
                {
                    if (ControlDic[tc].isExit == false)
                    {
                        ControlDic[tc].Close();
                    }
                    ControlDic.Remove(tc);
                }
            }
            catch (Exception e)
            {
                LogManger.Instance.Error(e);
                //throw;
            }


        }
    }

    public class ModelItem
    {
        string name;
        TcpClient client;

        public ModelItem(string name, TcpClient client)
        {
            this.Name = name;
            this.Client = client;
        }

        public override string ToString()
        {
            return name+"-"+client.GetEndPoint.ToString();
        }

        public string Name { get => name; set => name = value; }
        public TcpClient Client { get => client; set => client = value; }
    }

    public class LockBitmap
    {
        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()//传入bmp，
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;//像素

                // Create rectangle to lock
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);//指定要锁定的部分

                // get source bitmap pixel format size检验是否是8.24.32位图像
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];//如果是24位，像素*3=所有RGB数组组。
                Iptr = bitmapData.Scan0;//指针放到第一个数据。也就是第一个像素的B

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);//复制到Pixel，传入bmp的数据复制到Pixel数组
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                if (source==null)
                {
                    Console.WriteLine(source);
                }
                else
                {
                    source.UnlockBits(bitmapData);
                }
   
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public System.Drawing.Color LockGetPixel(int x, int y)
        {
            System.Drawing.Color clr = System.Drawing.Color.Empty;

            // Get color components count
            int cCount = Depth / 8;//Depth表示多少位数据

            // Get start index of the specified pixel//获得指定像素的起始索引
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();//抛出新异常

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = System.Drawing.Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = System.Drawing.Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = System.Drawing.Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void LockSetPixel(int x, int y, System.Drawing.Color color)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
    }


}
