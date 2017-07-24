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
        private Point m_center_offset;
        private double m_distance;
        private double m_scale;
        private OpenGL m_openGLCtrl;
        private int LineNumber ;
        private int RectNumber ;
        private double m_wheel_multi;//滚轮滚动一次放大的倍数
        Dictionary<int, CADLine> AllLines = new Dictionary<int, CADLine>();
        Dictionary<int, CADRect> AllRects = new Dictionary<int, CADRect>();
        Dictionary<int, CADLine> SelLines = new Dictionary<int, CADLine>();
        Dictionary<int, CADRect> SelRects = new Dictionary<int, CADRect>();
        Point MidMouseDownStart = new Point(0,0);
        Point MidMouseDownEnd = new Point(0, 0);
        Point m_currentpos = new Point(0,0);
        public CADCtrl()
        {
            InitializeComponent();
            m_openGLCtrl = openGLCtrl.OpenGL;
            m_center_offset = new Point(0,0);
            m_distance = -10;
            m_scale = 0.001;
            m_wheel_multi = 0.2;
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
            m_openGLCtrl.MatrixMode(OpenGL.GL_MODELVIEW);
            m_openGLCtrl.LoadIdentity();
            //m_openGLCtrl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);
            //m_openGLCtrl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);
            //m_openGLCtrl.Viewport(0, 0, (int)(this.Width > this.Height ? this.Width : this.Height),
            //    (int)(this.Width > this.Height ? this.Width : this.Height));

            m_openGLCtrl.Translate(m_center_offset.X/70, m_center_offset.Y/70, m_distance);
            //m_openGLCtrl.Translate(0, 0, m_distance);
            m_openGLCtrl.Scale(m_scale, m_scale, 0);
            //m_openGLCtrl.Ortho2D(0.0,this.Width,0.0,this.Height);
            //m_openGLCtrl.LookAt(0, 0, -2, 0, 0, -1, 0, 1, 0);
            m_openGLCtrl.LookAt(0, 0, 0, 0, 0, -1, 0, 1, 0);
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
                if (e.ClickCount == 2)
                {
                    this.Cursor = Cursors.SizeAll;
                    m_center_offset.X = 0;
                    m_center_offset.Y = 0;
                    m_scale = 0.0001;
                }
                else if (e.ClickCount == 1)
                {
                    MidMouseDownStart = e.GetPosition(e.Source as FrameworkElement);
                    this.Cursor = Cursors.SizeAll;
                }
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
                m_center_offset.X = m_center_offset.X + vDistance.X;
                m_center_offset.Y = m_center_offset.Y - vDistance.Y;
                MidMouseDownStart = MidMouseDownEnd;
            }


            m_currentpos.Y = this.Height / 2 - e.GetPosition(e.Source as FrameworkElement).Y - m_center_offset.Y;
            m_currentpos.X = -this.Width / 2 + e.GetPosition(e.Source as FrameworkElement).X - m_center_offset.X;
            //m_currentpos = (Point)(e.GetPosition(e.Source as FrameworkElement) - m_center_offset);
        }

        private void OpenGLControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Vector vDistance = new Vector();
            if (e.Delta > 0)
            {
                vDistance.X = (m_center_offset.X - e.GetPosition(e.Source as FrameworkElement).X + this.Width / 2) * -m_wheel_multi;
                vDistance.Y = (m_center_offset.Y + e.GetPosition(e.Source as FrameworkElement).Y - this.Height / 2) * -m_wheel_multi;
                m_scale -= m_scale*m_wheel_multi;
            }
            else
            {
                vDistance.X = (m_center_offset.X - e.GetPosition(e.Source as FrameworkElement).X + this.Width / 2) * m_wheel_multi;
                vDistance.Y = (m_center_offset.Y + e.GetPosition(e.Source as FrameworkElement).Y - this.Height / 2) * m_wheel_multi;
                m_scale += m_scale * m_wheel_multi;
            }

            if (m_scale <= 0.000001)
                m_scale = 0.000001;
            else
            {
                m_center_offset.X = m_center_offset.X + vDistance.X;
                m_center_offset.Y = m_center_offset.Y + vDistance.Y;
            }
            

        }

        private void openGLCtrl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            //if (e.ChangedButton == MouseButton.Middle)
            //{
            //    m_openGLCtrl.Translate(m_center_offset.X, m_center_offset.Y, 0);
            //    m_openGLCtrl.Scale(1, 1, 1);
            //    Console.WriteLine("11111");
            //}

        }

        private void RedrawAll()
        {
            this.DrawLine(new CADLine(0, 0, 0.5 / m_scale, 0));
            this.DrawLine(new CADLine(0.5 / m_scale, 0, 0.4 / m_scale, 0.05 / m_scale));
            this.DrawLine(new CADLine(0.5 / m_scale, 0, 0.4 / m_scale, -0.05 / m_scale));

            this.DrawLine(new CADLine(0, 0, 0, 0.5 / m_scale));
            this.DrawLine(new CADLine(0, 0.5 / m_scale, 0.05 / m_scale, 0.4 / m_scale));
            this.DrawLine(new CADLine(0, 0.5 / m_scale, -0.05 / m_scale, 0.4 / m_scale));

            foreach (int value in this.SelLines.Keys)
            {
                this.DrawSelLine(value);
            }

            foreach (CADLine value in this.AllLines.Values)
            {
                this.DrawLine(value);
            }
            foreach (CADRect value in this.AllRects.Values)
            {
                this.DrawRect(value);
            }
            
            

            string pos_str = string.Format("Position:[{0:0.00},{1:0.00}]", m_currentpos.X / m_scale / 70, m_currentpos.Y / m_scale / 70);
            this.DrawText(pos_str, new Point(0, 5));
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

        private void SelLine(int line_id)
        {
            if (!this.SelLines.ContainsKey(line_id))
            {
                this.SelLines.Add(line_id, this.AllLines[line_id]);
            }
            else
                this.SelLines.Remove(line_id);
        }


        private void DrawSelLine(int line_id)
        {
            if (!AllLines.ContainsKey(line_id))
                return;
            CADLine line = AllLines[line_id];
            m_openGLCtrl.LineWidth(4);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(0.7f, 0.2f, 0.2f);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);

            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
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
            m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);
            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }



        private void DrawRect(CADRect rect)
        {
            m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            m_openGLCtrl.Vertex(rect.m_xs, rect.m_ys);
            m_openGLCtrl.Vertex(rect.m_xe, rect.m_ys);

            m_openGLCtrl.Vertex(rect.m_xe, rect.m_ys);
            m_openGLCtrl.Vertex(rect.m_xe, rect.m_ye);
            
            m_openGLCtrl.Vertex(rect.m_xe, rect.m_ye);
            m_openGLCtrl.Vertex(rect.m_xs, rect.m_ye);
            
            m_openGLCtrl.Vertex(rect.m_xs, rect.m_ye);
            m_openGLCtrl.Vertex(rect.m_xs, rect.m_ys);

            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }

        private void DrawText(string text, Point pos)
        {
            m_openGLCtrl.DrawText((int)(pos.X), (int)(pos.Y), 0.5f, 1.0f, 0.5f, "Lucida Console", 12.0f, text);
        }


        public void UserDrawLine(Point p1, Point p2)
        {
            CADLine line = new CADLine(p1, p2);
            this.AddLine(line);

            CADRect rect = new CADRect(p1, p2);
            this.AddRect(rect);

        }

        public void UserSelLine(int id)
        {
            this.SelLine(id);
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
