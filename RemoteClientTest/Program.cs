using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using TcpClientRoot;
using WindowsInput;
using WindowsInput.Native;

namespace RemoteClientTest
{
    class Program
    {

        public static void TestFunc()
        {
            HookDir.MouseHook mouseHook = new HookDir.MouseHook();
            HookDir.KeyboardHook keyboardHook = new HookDir.KeyboardHook();

            mouseHook.MouseMove += new MouseEventHandler(mouseHook_MouseMove);
            //mouseHook.MouseDown += new MouseEventHandler(mouseHook_MouseDown);
            //mouseHook.MouseUp += new MouseEventHandler(mouseHook_MouseUp);

            mouseHook.Start();
            while (isExit == false)
            {
                Thread.Sleep(50);
            }

            mouseHook.Stop();
        }
        static bool isExit;
        static void Main(string[] args)
        {


            InputSimulator sim = new InputSimulator();
            sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);

            while (true)
            {

            }
            //switch (k)
            //{
            //    case KeyBoardMsg.AltF4:
            //        sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.F4);
            //        break;
            //    case KeyBoardMsg.AltTab:
            //        sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
            //        break;
            //    case KeyBoardMsg.WinD:
            //        sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);

            //        ret
            //            ;
                    //TestFunc();
                    //while (true)
                    //{
                    //    Thread.Sleep(50);
                    //}
                    //mh.SetHook();
                    //mh.MouseMoveEvent += mh_MouseMoveEvent;
                    //mh.MouseClickEvent += mh_MouseClickEvent;
                    //new Thread(TestFunc).Start();
                    //while (true)
                    //{

                    //}
                    //isExit = true;


                    //return;
                    //InputSimulator sim = new InputSimulator();
                    //sim.SimulateTextEntry("");
                    //sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.a)

                    //KeyboardSimulator keyboard = new KeyboardSimulator(sim);
                    //keyboard.Mouse.LeftButtonDown();
                    //while (true)
                    //{
                    //    //sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_E);
                    //    //sim.Keyboard.KeyDown(VirtualKeyCode.VK_E);
                    //    keyboard.Mouse.LeftButtonDown();
                    //    Thread.Sleep(1000);
                    //}
                    //return;

                    //string path = @"C:\Users\admin\Desktop\TIM截图20190718104420.png";
                    //string p2 = @"C:\Users\admin\Desktop\新建文件夹 (2)\dis.png";

                    //string savePath = @"C:\Users\admin\Desktop\新建文件夹 (2)\";
                    //int count = 0;


                    clientManger cm = new clientManger();

            Bitmap oldMap = null;

            while (true)
            {
                Thread.Sleep(50);
                //Thread.Sleep(1000);
                //if (cm.bc.IsConnect == false)
                //{
                //    cm.bc.Connect();
                //}
            }

        }

        private static void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("mouse move");
        }

        private static void mh_MouseClickEvent(object sender, MouseEventArgs e)
        {
            Console.WriteLine("click");
        }

        private static void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            Console.WriteLine("move");
        }



        #region 屏幕截图
        //得到屏幕截图 
        public static byte[] GetDesktopScreenShot(int wid=1920,int hig=1080,long Quality=20L)
        {
            Bitmap img = new Bitmap(wid, hig);
            //Bitmap img = new Bitmap(1280, 720);
            Graphics gc = Graphics.FromImage(img);
            gc.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(wid, hig));

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

            return arr;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        #endregion

        #region 废方法
        /// <summary>
        /// 比较两图的差异
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static Bitmap test(Bitmap m1, Bitmap m2)
        {
            LockBitmap lmp = new LockBitmap(m1);
            lmp.LockBits();//锁定bitmap到内存

            LockBitmap lmp2 = new LockBitmap(m2);
            lmp2.LockBits();//锁定bitmap到内存

            int w = 1920;
            int h = 1080;
            Bitmap bmp3 = new Bitmap(w, h);//建立空白图片，以便存储差异像素
            LockBitmap lmp3 = new LockBitmap(bmp3);
            lmp3.LockBits();//锁定bitmap到内存

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    //获取像素随便赋值给q  
                    Color c1 = lmp.LockGetPixel(i, j);
                    Color c2 = lmp2.LockGetPixel(i, j);
                    if (c1 != c2)
                    {
                        lmp3.LockSetPixel(i, j, c2);
                    }
                }
            }
            lmp.UnlockBits();//解除锁定
            lmp2.UnlockBits();//解除锁定
            lmp3.UnlockBits();//解除锁定

            //string savePath = @"C:\Users\admin\Desktop\新建文件夹 (2)\";
            //bmp3.Save(savePath+"dis.png");
            return bmp3;

        }

        /// <summary>
        /// 图片转换为字节数组
        /// </summary>
        /// <param name="image">图片</param>
        /// <returns>字节数组</returns>
        public static byte[] ImageToBytes(Bitmap map,ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                map.Save(ms, format);
                byte[] arr = ms.ToArray();
                return arr;
            }


        }
    
        /// <summary>
        /// 字节数组转换为图片
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <returns>图片</returns>
        public static Bitmap BytesToImage(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                Bitmap bit = new Bitmap(ms);
                return bit;
            }
        }

        public static Bitmap SmallPicWidth(Bitmap objPic,float size)
        {
            System.Drawing.Bitmap  objNewPic;
            try
            {
                double intWidth = objPic.Width * size;
                double intHeight = objPic.Height * size;
                //double intHeight = (intWidth / Convert.ToDouble(objPic.Width)) * objPic.Height;
                objNewPic = new System.Drawing.Bitmap(objPic, Convert.ToInt32(intWidth), Convert.ToInt32(intHeight));
                return objNewPic;
            }
            catch (Exception exp) { throw exp; }
            finally
            {
                objPic = null;
                objNewPic = null;
            }
        }



        public static Bitmap ystp(Bitmap bmp, string filePath_ystp)
        {
            //Bitmap
            //ImageCoderInfo  
            ImageCodecInfo ici = null;
            //Encoder
            Encoder ecd = null;
            //EncoderParameter
            EncoderParameter ept = null;
            //EncoderParameters
            EncoderParameters eptS = null;
            try
            {
                ici = getImageCoderInfo("image/jpeg");
                ecd = Encoder.Quality;
                eptS = new EncoderParameters(1);
                ept = new EncoderParameter(ecd, 50L);
                eptS.Param[0] = ept;
                //bmp.Save(filePath_ystp, ici, eptS);
                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                bmp.Dispose();
                ept.Dispose();
                eptS.Dispose();
            }
        }


        private static ImageCodecInfo getImageCoderInfo(string coderType)// 获取图片编码类型信息
        {
            ImageCodecInfo[] iciS = ImageCodecInfo.GetImageEncoders();

            ImageCodecInfo retIci = null;

            foreach (ImageCodecInfo ici in iciS)
            {
                if (ici.MimeType.Equals(coderType))
                    retIci = ici;
            }

            return retIci;
        }

        #endregion
    }


    public class clientManger
    {
        public TcpClient bc;
        public static clientManger instaces;
        public clientManger()
        {
            instaces = this;
            ToolClass.GetDataPack = GetPack;
            ToolClass.outPutInfo = printInfo;
            ToolClass.isUserDataPack = true;
            ToolClass.msgArrLen = 10240000;
            bc = new TcpClient(new socketEvent());
            bc.Connect("192.168.5.102", 54321);
        }



        private void printInfo(object obj)
        {
            //Console.WriteLine(obj);
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

            return arr;
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


    }

    public class socketEvent : ISocketEvent
    {
        public void ClientDisconnect(TcpClient tc)
        {
            Console.WriteLine("断开连接"+tc.IsConnect);
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
            dp = dp+(short)MsgEnum.ComputerName+ System.Net.Dns.GetHostEntry("localhost").HostName;
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
        DesktopImg=0,
        ComputerName,

        MouseClick =100,
        MouseMove=101,


    }

    public class newDataPack : BaseDataPack
    {
        public override void UserMsgRead(byte[] msg)
        {
        }

        public override void UserMsgRead(DataPack dp)
        {
            MsgEnum me = (MsgEnum)dp.ReadShort();
            Console.WriteLine(me);
            switch (me)
            {
                case MsgEnum.DesktopImg:
                    clientManger.instaces.HandleDesktopImg();
                    break;
                case MsgEnum.MouseClick:
                    clientManger.instaces.HandleMouseClick(dp);
                    break;
                case MsgEnum.MouseMove:
                    clientManger.instaces.HnadleMouseMove(dp);
                    break;

            }
        }
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
                Rectangle rect = new Rectangle(0, 0, Width, Height);//指定要锁定的部分

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
                source.UnlockBits(bitmapData);
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
        public Color LockGetPixel(int x, int y)
        {
            Color clr = Color.Empty;

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
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void LockSetPixel(int x, int y, Color color)
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
