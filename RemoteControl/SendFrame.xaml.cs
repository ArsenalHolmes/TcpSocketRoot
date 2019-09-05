using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteControl
{
    /// <summary>
    /// SendFrame.xaml 的交互逻辑
    /// </summary>
    public partial class SendFrame : Window
    {
        public SendFrame()
        {
            InitializeComponent();
        }
        ClientControl cc;

        Thread t;
        public SendFrame(ClientControl cc)
        {
            InitializeComponent();
            this.cc = cc;

        }

        protected override void OnContentRendered(EventArgs e)
        {
            t = new Thread(ThreadFunc);
            t.IsBackground = true;
            t.Start();
        }


        private void ThreadFunc()
        {
            while (cc.pos!=cc.len)
            {
                Console.WriteLine(cc.pos+"--"+cc.len);
                //ChangText(((cc.pos*1f / cc.len)*100).ToString("F2"));
                ChangText((cc.pos * 1f / cc.len));
            }
            Console.WriteLine("关闭");
            this.Dispatcher.Invoke(() => {
                this.Close();
            });
        }

        public void ChangText(string s)
        {
           
            //this.Dispatcher.Invoke(() => {
            //    m_text.Text = string.Format(s+"%");
            //});
        }
        public void ChangText(float f)
        {
            this.Dispatcher.Invoke(() => {
                SetValue(f);
            });
           
        }

        /// <summary>
        /// 设置百分百，输入小数，自动乘100
        /// </summary>
        /// <param name="percentValue"></param>
        private void SetValue(double percentValue)
        {
            /*****************************************
              方形矩阵边长为34，半长为17
              环形半径为14，所以距离边框3个像素
              环形描边3个像素
            ******************************************/
            double angel = percentValue * 360; //角度



            double radius = 110; //环形半径

            //起始点
            double leftStart = 150;
            double topStart = 40;

            //结束点
            double endLeft = 0;
            double endTop = 0;



            //数字显示
            lbValue.Content = (percentValue * 100).ToString("0") + "%";

            /***********************************************
            * 整个环形进度条使用Path来绘制，采用三角函数来计算
            * 环形根据角度来分别绘制，以90度划分，方便计算比例
            ***********************************************/

            bool isLagreCircle = false; //是否优势弧，即大于180度的弧形

            //小于90度
            if (angel <= 90)
            {
                /*****************
                          *
                          *   *
                          * * ra
                   * * * * * * * * *
                          *
                          *
                          *
                ******************/
                double ra = (90 - angel) * Math.PI / 180; //弧度
                endLeft = leftStart + Math.Cos(ra) * radius; //余弦横坐标
                endTop = topStart + radius - Math.Sin(ra) * radius; //正弦纵坐标

            }

            else if (angel <= 180)
            {
                /*****************
                          *
                          *  
                          * 
                   * * * * * * * * *
                          * * ra
                          *  *
                          *   *
                ******************/

                double ra = (angel - 90) * Math.PI / 180; //弧度
                endLeft = leftStart + Math.Cos(ra) * radius; //余弦横坐标
                endTop = topStart + radius + Math.Sin(ra) * radius;//正弦纵坐标
            }

            else if (angel <= 270)
            {
                /*****************
                          *
                          *  
                          * 
                   * * * * * * * * *
                        * *
                       *ra*
                      *   *
                ******************/
                isLagreCircle = true; //优势弧
                double ra = (angel - 180) * Math.PI / 180;
                endLeft = leftStart - Math.Sin(ra) * radius;
                endTop = topStart + radius + Math.Cos(ra) * radius;
            }

            else if (angel < 360)
            {
                /*****************
                      *   *
                       *  *  
                     ra * * 
                   * * * * * * * * *
                          *
                          *
                          *
                ******************/
                isLagreCircle = true; //优势弧
                double ra = (angel - 270) * Math.PI / 180;
                endLeft = leftStart - Math.Cos(ra) * radius;
                endTop = topStart + radius - Math.Sin(ra) * radius;
            }
            else
            {
                isLagreCircle = true; //优势弧
                endLeft = leftStart - 0.001; //不与起点在同一点，避免重叠绘制出非环形
                endTop = topStart;
            }

            Point arcEndPt = new Point(endLeft, endTop); //结束点
            Size arcSize = new Size(radius, radius);
            SweepDirection direction = SweepDirection.Clockwise; //顺时针弧形
            //弧形
            ArcSegment arcsegment = new ArcSegment(arcEndPt, arcSize, 0, isLagreCircle, direction, true);

            //形状集合
            PathSegmentCollection pathsegmentCollection = new PathSegmentCollection();
            pathsegmentCollection.Add(arcsegment);

            //路径描述
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(leftStart, topStart); //起始地址
            pathFigure.Segments = pathsegmentCollection;

            //路径描述集合
            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathFigureCollection.Add(pathFigure);

            //复杂形状
            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures = pathFigureCollection;

            //Data赋值
            myCycleProcessBar1.Data = pathGeometry;
            //达到100%则闭合整个
            if (angel == 360)
                myCycleProcessBar1.Data = Geometry.Parse(myCycleProcessBar1.Data.ToString() + " z");
        }

    }
}
