using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL;
using SharpGL.SceneGraph;

namespace abaqus_helper.CADCtrl
{

    /// <summary>
    /// CADCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class CADCtrl : UserControl
    {
        private Point m_center_point;
        private double m_distance;
        private double m_scale;
        private OpenGL m_openGLCtrl;
        private int LineNumber ;
        private int RectNumber ;
        Dictionary<int, CADLine> AllLines = new Dictionary<int, CADLine>();
        Dictionary<int, CADRect> AllRects = new Dictionary<int, CADRect>();
        Point MidMouseDownStart = new Point(0,0);
        Point MidMouseDownEnd = new Point(0, 0);
        public CADCtrl()
        {
            InitializeComponent();
            m_openGLCtrl = openGLCtrl.OpenGL;
            m_center_point = new Point(0,0);
            m_distance = -1;
            m_scale = 10;
            LineNumber = 0;
            RectNumber = 0;
        }

        /// <summary>
        /// 画直线
        /// </summary>
        /// <param name="sX">起点x坐标</param>
        /// <param name="sY">起点y坐标</param>
        /// <param name="eX">终点x坐标</param>
        /// <param name="eY">终点y坐标</param>
        /// <param name="isArrow">是否带箭头</param>
        /// <param name="withBorder">是否带边界线</param>
        //private void drawLine(int sX, int sY, int eX, int eY, bool isArrow = false, bool withBorder = true)
        //{
        //    OpenGL gl = openGLControl.OpenGL;
        //    gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        //    gl.LoadIdentity();
        //    gl.Color(1.0f, 1.0f, 1.0f);
        //    gl.Translate(-1, -1, 0);
        //    gl.Begin(SharpGL.Enumerations.BeginMode.Lines);
        //    gl.Vertex(sX * xRange + 1 - xTimes, sY * yRange + 1 - yTimes);
        //    gl.Vertex(eX * xRange + 1 - xTimes, eY * yRange + 1 - yTimes);
        //    gl.End();
        //    //gl.Color(0.0f, 1.0f, 0.0f);

        //}

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //OpenGL gl = openGLCtrl.OpenGL;
            m_openGLCtrl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            m_openGLCtrl.MatrixMode(OpenGL.GL_2D);
            m_openGLCtrl.LoadIdentity();
            //m_openGLCtrl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);
            //m_openGLCtrl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);
            //m_openGLCtrl.Viewport(0, 0, (int)(this.Width > this.Height ? this.Width : this.Height),
            //    (int)(this.Width > this.Height ? this.Width : this.Height));
            m_openGLCtrl.Translate(0.0f, 0.0f, m_distance);
            m_openGLCtrl.Ortho2D(0.0,this.Width,0.0,this.Height);
            
            //m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            //m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            //m_openGLCtrl.Vertex(0.0f, 0.0f, 0.0f);
            //m_openGLCtrl.Vertex(1.0f, 1.0f, 0.0f);
            //m_openGLCtrl.End();

            //  Flush OpenGL.
            //m_openGLCtrl.Flush();
            //CADLine xline = new CADLine(0, 0, 1, 0);
            //this.AddLine(xline);
            //this.AddLine(new CADLine(1, 0, 0.8, 0.1));
            //this.AddLine(new CADLine(1, 0, 0.8, -0.1));

            //CADLine yline = new CADLine(0, 0, 0, 1);
            //this.AddLine(yline);
            //this.AddLine(new CADLine(0, 1, 0.1, 0.8));
            //this.AddLine(new CADLine(0, 1, -0.1, 0.8));
            CADLine xline = new CADLine(0, 0, 1, 0);
            this.AddLine(xline);
            this.AddLine(new CADLine(1, 0, 0.8, 0.1));
            this.AddLine(new CADLine(1, 0, 0.8, -0.1));

            CADLine yline = new CADLine(0, 0, 0, 1);
            this.AddLine(yline);
            this.AddLine(new CADLine(0, 1, 0.1, 0.8));
            this.AddLine(new CADLine(0, 1, -0.1, 0.8));

            this.RedrawAll();
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //  Enable the OpenGL depth testing functionality.
            m_openGLCtrl.Enable(OpenGL.GL_DEPTH_TEST);

        }

        private void OpenGLControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                MidMouseDownStart = e.GetPosition(e.Source as FrameworkElement);
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void OpenGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
                this.Cursor = Cursors.Arrow;
        }

        private void OpenGLControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
                this.Cursor = Cursors.Arrow;
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                MidMouseDownEnd = e.GetPosition(e.Source as FrameworkElement);
                Vector vDistance = MidMouseDownEnd - MidMouseDownStart;
                m_center_point.X = m_center_point.X + vDistance.X;
                m_center_point.Y = m_center_point.Y - vDistance.Y;
                m_openGLCtrl.Translate(m_center_point.X, m_center_point.Y, m_distance);
                //m_openGLCtrl.LookAt();
                MidMouseDownStart = MidMouseDownEnd;
            }
        }

        private void OpenGLControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (e.Delta > 0)
            //{
            //    m_distance -= 1;
            //    //m_scale = 1;
            //}
            //else
            //{
            //    m_distance += 1;
            //    //m_scale = 0.5;
            //}
            //if (m_distance <= -100)
            //    m_distance = -100;
            //if (m_distance >= -1)
            //    m_distance = -1;

            if (e.Delta > 0)
            {
                m_scale -= 1;
            }
            else
            {
                m_scale += 1;
            }

            if (m_scale <= 1)
                m_scale = 1;
            //if (m_distance <= -1)
            //    m_distance = 1;

            m_openGLCtrl.Scale(m_scale, m_scale, 1);
            //RedrawAll();


        }

        private void openGLCtrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Middle)
            //{
            //    m_openGLCtrl.Translate(m_center_point.X, m_center_point.Y, 0);
            //    m_openGLCtrl.Scale(1, 1, 1);
            //    Console.WriteLine("11111");
            //}

        }

        private void RedrawAll()
        {
            m_openGLCtrl.Translate(m_center_point.X, m_center_point.Y, 0);
            //m_openGLCtrl.Translate(0, 0, 0);
            m_openGLCtrl.Scale(m_scale, m_scale, 1);
            foreach (CADLine value in this.AllLines.Values)
            {
                this.DrawLine(value);
            }
        }

        private void AddLine(CADLine line)
        {
            LineNumber++;
            line.m_id = LineNumber;
            if (this.AllLines.ContainsKey(LineNumber))
                AllLines[LineNumber] = line;
            else
                AllLines.Add(LineNumber, line);
        }

        private void AddRect(CADRect rect)
        {
            RectNumber++;
            rect.m_id = RectNumber;
            if (this.AllRects.ContainsKey(RectNumber))
                AllRects[RectNumber] = rect;
            else
                AllRects.Add(RectNumber, rect);
        }

        private void DrawLine(CADLine line)
        {
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);
            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
            //LineNumber++;
            //if (this.AllLines.ContainsKey(LineNumber))
            //    AllLines[LineNumber] = line;
            //else
            //    AllLines.Add(LineNumber, line);
        }

        public void UserDrawLine(Point p1,Point p2)
        {
            CADLine line = new CADLine(p1,p2);
            this.AddLine(line);
            DrawLine(line);
        }

        private void DrawRect(CADRect rect)
        {
            //RectNumber++;
            //if (this.AllRects.ContainsKey(RectNumber))
            //    AllRects[RectNumber] = rect;
            //else
            //    AllRects.Add(RectNumber, rect);
        }

        private void DelLine(int line_id)
        {
            if (this.AllLines.ContainsKey(line_id))
                AllLines.Remove(line_id);
 
        }

        private void DelRect(int rect_id)
        {
            if (this.AllRects.ContainsKey(rect_id))
                AllRects.Remove(rect_id);

        }

       


    }


    public class CADLine
    {
        public int m_id;
        public float m_xs = 0.0f;
        public float m_ys = 0.0f;
        public float m_xe = 0.0f;
        public float m_ye = 0.0f;


        public CADLine(Point p1, Point p2)
        {
            m_xs = (float)p1.X;
            m_ys = (float)p1.Y;
            m_xe = (float)p2.X;
            m_ye = (float)p2.Y;
        }

        public CADLine(double xs, double ys, double xe, double ye)
        {
            m_xs = (float)xs;
            m_ys = (float)ys;
            m_xe = (float)xe;
            m_ye = (float)ye;
        }
    }

    public class CADRect
    {
        public int m_id;
        public float m_xs = 0.0f;
        public float m_ys = 0.0f;
        public float m_xe = 0.0f;
        public float m_ye = 0.0f;

        public CADRect(Point p1, Point p2)
        {
            m_xs = (float)p1.X;
            m_ys = (float)p1.Y;
            m_xe = (float)p2.X;
            m_ye = (float)p2.Y;
        }

        public CADRect(double xs, double ys, double xe, double ye)
        {
            m_xs = (float)xs;
            m_ys = (float)ys;
            m_xe = (float)xe;
            m_ye = (float)ye;
        }
    }
}
