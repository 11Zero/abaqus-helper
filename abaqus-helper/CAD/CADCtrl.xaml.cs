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
        private double m_pixaxis;//像素对应平移的倍数
        private double m_gridstep;//栅格间距，像素为单位
        private int LineNumber;
        private int RectNumber;
        private int PointNumber;
        private CADRect m_border;
        private double m_wheel_multi;//滚轮滚动一次放大的倍数
        Dictionary<int, CADLine> AllLines = new Dictionary<int, CADLine>();
        Dictionary<int, int[]> AllPointsInLines = new Dictionary<int, int[]>();
        Dictionary<int, CADRect> AllRects = new Dictionary<int, CADRect>();
        Dictionary<int, int[]> AllPointsInRects = new Dictionary<int, int[]>();
        Dictionary<int, CADPoint> AllPoints = new Dictionary<int, CADPoint>();
        Dictionary<int, CADPoint> SelPoints = new Dictionary<int, CADPoint>();
        Dictionary<int, CADLine> SelLines = new Dictionary<int, CADLine>();
        Dictionary<int, CADRect> SelRects = new Dictionary<int, CADRect>();
        Dictionary<int, CADRGB> AllColors = new Dictionary<int, CADRGB>();
        Dictionary<int, int> AllLinesColor = new Dictionary<int, int>();
        Dictionary<int, int> AllRectsColor = new Dictionary<int, int>();
        Point MidMouseDownStart = new Point(0, 0);
        Point MidMouseDownEnd = new Point(0, 0);
        Point m_currentpos = new Point(0, 0);
        public CADCtrl()
        {
            InitializeComponent();
            m_openGLCtrl = openGLCtrl.OpenGL;

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_openGLCtrl = openGLCtrl.OpenGL;
            m_center_offset = new Point(0, 0);
            m_distance = -10;
            m_pixaxis = 0.1208;//与移动速度成反比
            m_scale = 0.00002;
            m_border = new CADRect(0, 0, 0, 0);
            m_wheel_multi = 0.2;
            m_gridstep = 50;
            LineNumber = 0;
            RectNumber = 0;
            PointNumber = 1;
            AllPoints[PointNumber] = new CADPoint();
            m_pixaxis = m_pixaxis * this.Height;//(this.Width < this.Height ? this.Width : this.Height);
            AllColors.Add(1, new CADRGB(1, 1, 1));
            AllColors.Add(2, new CADRGB(1, 0, 0));
            AllColors.Add(3, new CADRGB(0, 1, 0));
            AllColors.Add(4, new CADRGB(0, 0, 1));
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
                    if ((m_border.m_ye - m_border.m_ys) > (m_border.m_xe - m_border.m_xs))
                        m_scale = this.Height / 50 / (m_border.m_ye - m_border.m_ys);
                    else
                        m_scale = this.Width / 50 / (m_border.m_xe - m_border.m_xs);

                    //m_scale = this.Height / this.Width * 6 / ((m_border.m_ye - m_border.m_ys) < (m_border.m_xe - m_border.m_xs) ? (m_border.m_ye - m_border.m_ys) : (m_border.m_xe - m_border.m_xs));
                    if (m_scale > 1000000)
                        m_scale = 8 / (m_border.m_xe - m_border.m_xs);
                    if (m_scale > 1000000)
                        m_scale = 8 / (m_border.m_ye - m_border.m_ys);
                    if (m_scale > 1000000)
                        m_scale = 0.00001;
                    m_center_offset.X = -(m_border.m_xe - m_border.m_xs) / 2 * m_pixaxis * m_scale;
                    m_center_offset.Y = -(m_border.m_ye - m_border.m_ys) / 2 * m_pixaxis * m_scale;
                }
                else if (e.ClickCount == 1)
                {
                    MidMouseDownStart = e.GetPosition(e.Source as FrameworkElement);
                    this.Cursor = Cursors.SizeAll;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                Point mousepos = new Point(m_currentpos.X / m_scale / m_pixaxis, m_currentpos.Y / m_scale / m_pixaxis);
                int id_sel_point = -1;
                double sel_dis_point = 1 / m_scale;
                foreach (int id in this.AllPoints.Keys)
                {
                    double dis = this.GetDistance(mousepos, AllPoints[id]);
                    if (dis < 0.06 / m_scale && dis < sel_dis_point)
                    {
                        id_sel_point = id;
                        sel_dis_point = dis;
                    }
                }
                if (id_sel_point > 0)
                {

                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        this.SelPoint(id_sel_point);
                        return;
                    }
                    else
                    {
                        if (!SelLines.ContainsKey(id_sel_point))
                            SelPoints.Clear();
                        this.SelPoint(id_sel_point);
                        return;
                    }
                }
                else
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                        this.SelPoints.Clear();
                }


                int id_sel_line = -1;
                double sel_dis_line = 1 / m_scale;
                foreach (int id in this.AllLines.Keys)
                {
                    double dis = this.GetDistance(mousepos, AllLines[id]);
                    if (dis < 0)
                        continue;
                    if (dis < 0.06 / m_scale && dis < sel_dis_line)
                    {
                        id_sel_line = id;
                        sel_dis_line = dis;
                    }
                }



                if (id_sel_line > 0)
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        this.SelLine(id_sel_line);
                    else
                    {
                        if (!SelLines.ContainsKey(id_sel_line))
                            SelLines.Clear();
                        this.SelLine(id_sel_line);
                    }
                }
                else
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                        this.SelLines.Clear();
                }

                int id_sel_rect = -1;
                double sel_dis_rect = 1 / m_scale;
                foreach (int id in this.AllRects.Keys)
                {
                    double dis = this.GetDistance(mousepos, AllRects[id]);
                    if (dis < 0)
                        continue;
                    if (dis < 0.06 / m_scale && dis < sel_dis_rect)
                    {
                        id_sel_rect = id;
                        sel_dis_rect = dis;
                    }
                }



                if (id_sel_rect > 0)
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        this.SelRect(id_sel_rect);
                    else
                    {
                        if (!SelRects.ContainsKey(id_sel_rect))
                            SelRects.Clear();
                        this.SelRect(id_sel_rect);
                    }
                }
                else
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                        this.SelRects.Clear();
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
                m_scale -= m_scale * m_wheel_multi;
            }
            else
            {
                vDistance.X = (m_center_offset.X - e.GetPosition(e.Source as FrameworkElement).X + this.Width / 2) * m_wheel_multi;
                vDistance.Y = (m_center_offset.Y + e.GetPosition(e.Source as FrameworkElement).Y - this.Height / 2) * m_wheel_multi;
                m_scale += m_scale * m_wheel_multi;
            }

            if (m_scale <= 0.0000001)
                m_scale = 0.0000001;
            else
            {
                m_center_offset.X = m_center_offset.X + vDistance.X;
                m_center_offset.Y = m_center_offset.Y + vDistance.Y;
            }


        }




        private void RedrawAll()
        {


            m_openGLCtrl.Translate(m_center_offset.X / m_pixaxis, m_center_offset.Y / m_pixaxis, m_distance);

            m_openGLCtrl.Scale(m_scale, m_scale, 0);

            this.DrawGrids();

            foreach (int value in this.SelLines.Keys)
            {
                this.DrawSelLine(value);
            }

            foreach (int value in this.SelRects.Keys)
            {
                this.DrawSelRect(value);
            }

            foreach (int value in this.SelPoints.Keys)
            {
                this.DrawSelPoint(value);
            }

            foreach (int key in this.AllLines.Keys)
            {
                this.DrawLine(AllLines[key], AllColors[AllLinesColor[key]]);
            }
            foreach (int key in this.AllRects.Keys)
            {
                this.DrawRect(AllRects[key], AllColors[AllRectsColor[key]]);
            }
            foreach (int key in this.AllPoints.Keys)
            {
                this.DrawPoint(AllPoints[key]);
            }

            this.DrawLine(new CADLine(0, 0, 0.5 / m_scale, 0));
            this.DrawLine(new CADLine(0.5 / m_scale, 0, 0.4 / m_scale, 0.05 / m_scale));
            this.DrawLine(new CADLine(0.5 / m_scale, 0, 0.4 / m_scale, -0.05 / m_scale));

            this.DrawLine(new CADLine(0, 0, 0, 0.5 / m_scale));
            this.DrawLine(new CADLine(0, 0.5 / m_scale, 0.05 / m_scale, 0.4 / m_scale));
            this.DrawLine(new CADLine(0, 0.5 / m_scale, -0.05 / m_scale, 0.4 / m_scale));


            string pos_str = string.Format("Position:[{0:0.00},{1:0.00}]", m_currentpos.X / m_scale / m_pixaxis, m_currentpos.Y / m_scale / m_pixaxis);
            this.DrawText(pos_str, new Point(0, 5));
        }

        private void AddLine(CADLine line, int color_id = 0)
        {
            LineNumber++;
            line.m_id = LineNumber;
            if (!AllColors.ContainsKey(color_id))
                color_id = 1;
            if (this.AllLines.ContainsKey(LineNumber))
            {
                AllLines[LineNumber] = line.Copy();
                AllLinesColor[LineNumber] = color_id;
                CADPoint point = new CADPoint(line.m_xs, line.m_ys);
                this.AddPoint(point);
                point.m_x = line.m_xe;
                point.m_y = line.m_ye;
                this.AddPoint(point);
                int[] pointsItems = { PointNumber - 1, PointNumber };
                AllPointsInLines[LineNumber] =  pointsItems;
            }
            else
            {
                AllLines.Add(LineNumber, line.Copy());
                AllLinesColor.Add(LineNumber, color_id);
                CADPoint point = new CADPoint(line.m_xs,line.m_ys);
                this.AddPoint(point);
                point.m_x = line.m_xe;
                point.m_y = line.m_ye;
                this.AddPoint(point);
                int[] pointsItems = {PointNumber-1,PointNumber};
                AllPointsInLines.Add(LineNumber, pointsItems);
                if (line.m_xs > line.m_xe)
                {
                    m_border.m_xs = m_border.m_xs < line.m_xe ? m_border.m_xs : line.m_xe;
                    m_border.m_xe = m_border.m_xe > line.m_xs ? m_border.m_xe : line.m_xs;
                }
                else
                {
                    m_border.m_xs = m_border.m_xs < line.m_xs ? m_border.m_xs : line.m_xs;
                    m_border.m_xe = m_border.m_xe > line.m_xe ? m_border.m_xe : line.m_xe;
                }
                if (line.m_ys > line.m_ye)
                {
                    m_border.m_ys = m_border.m_ys < line.m_ye ? m_border.m_ys : line.m_ye;
                    m_border.m_ye = m_border.m_ye > line.m_ys ? m_border.m_ye : line.m_ys;
                }
                else
                {
                    m_border.m_ys = m_border.m_ys < line.m_ys ? m_border.m_ys : line.m_ys;
                    m_border.m_ye = m_border.m_ye > line.m_ye ? m_border.m_ye : line.m_ye;
                }
            }
            //foreach (CADLine value in AllLines.Values)
            //{
            //    this.AddPoint(this.GetCrossPoint(line,value));
            //}
            //foreach (CADRect value in AllRects.Values)
            //{
            //    CADLine temp_line = new CADLine();
            //    temp_line.m_xs = value.m_xs;
            //    temp_line.m_ys = value.m_ys;

            //    temp_line.m_xe = value.m_xs;
            //    temp_line.m_ye = value.m_ye;
            //    this.AddPoint(this.GetCrossPoint(line, temp_line));
            //    temp_line.m_xs = value.m_xe;
            //    temp_line.m_ys = value.m_ye;
            //    this.AddPoint(this.GetCrossPoint(line, temp_line));
            //    temp_line.m_xe = value.m_xe;
            //    temp_line.m_ye = value.m_ys;
            //    this.AddPoint(this.GetCrossPoint(line, temp_line));
            //    temp_line.m_xs = value.m_xs;
            //    temp_line.m_ys = value.m_ys;
            //    this.AddPoint(this.GetCrossPoint(line, temp_line));
 
            //}
        }



        private void AddPoint(CADPoint point)
        {
            if (point == null)
                return;
            //if (!AllColors.ContainsKey(color_id))
            //    color_id = 1;
            PointNumber++;
            if (this.AllPoints.ContainsKey(PointNumber))
            {
                AllPoints[PointNumber] = point.Copy();
                //AllLinesColor[LineNumber] = color_id;
            }
            else
            {
                AllPoints.Add(PointNumber, point.Copy());
                //AllLinesColor.Add(LineNumber, color_id);
                //if (line.m_xs > line.m_xe)
                //{
                //    m_border.m_xs = m_border.m_xs < line.m_xe ? m_border.m_xs : line.m_xe;
                //    m_border.m_xe = m_border.m_xe > line.m_xs ? m_border.m_xe : line.m_xs;
                //}
                //else
                //{
                //    m_border.m_xs = m_border.m_xs < line.m_xs ? m_border.m_xs : line.m_xs;
                //    m_border.m_xe = m_border.m_xe > line.m_xe ? m_border.m_xe : line.m_xe;
                //}
                //if (line.m_ys > line.m_ye)
                //{
                //    m_border.m_ys = m_border.m_ys < line.m_ye ? m_border.m_ys : line.m_ye;
                //    m_border.m_ye = m_border.m_ye > line.m_ys ? m_border.m_ye : line.m_ys;
                //}
                //else
                //{
                //    m_border.m_ys = m_border.m_ys < line.m_ys ? m_border.m_ys : line.m_ys;
                //    m_border.m_ye = m_border.m_ye > line.m_ye ? m_border.m_ye : line.m_ye;
                //}
            }
        }


        private void AddRect(CADRect rect, int color_id = 0)
        {
            RectNumber++;
            rect.m_id = RectNumber;
            if (!AllColors.ContainsKey(color_id))
                color_id = 1;
            if (this.AllRects.ContainsKey(RectNumber))
            {
                AllRects[RectNumber] = rect.Copy();
                AllRectsColor[RectNumber] = color_id;
                CADPoint point = new CADPoint(rect.m_xs, rect.m_ys);
                this.AddPoint(point);
                point.m_x = rect.m_xs;
                point.m_y = rect.m_ye;
                this.AddPoint(point);
                point.m_x = rect.m_xe;
                point.m_y = rect.m_ys;
                this.AddPoint(point);
                point.m_x = rect.m_xe;
                point.m_y = rect.m_ye;
                this.AddPoint(point);
                int[] pointsItems = { PointNumber - 3, PointNumber - 2,PointNumber - 1, PointNumber };
                AllPointsInRects[RectNumber] = pointsItems;
            }
            else
            {
                AllRects.Add(RectNumber, rect.Copy());
                AllRectsColor.Add(RectNumber, color_id);
                CADPoint point = new CADPoint(rect.m_xs, rect.m_ys);
                this.AddPoint(point);
                point.m_x = rect.m_xs;
                point.m_y = rect.m_ye;
                this.AddPoint(point);
                point.m_x = rect.m_xe;
                point.m_y = rect.m_ys;
                this.AddPoint(point);
                point.m_x = rect.m_xe;
                point.m_y = rect.m_ye;
                this.AddPoint(point);
                int[] pointsItems = { PointNumber - 3, PointNumber - 2, PointNumber - 1, PointNumber };
                AllPointsInRects.Add(RectNumber,pointsItems);
                if (rect.m_xs > rect.m_xe)
                {
                    m_border.m_xs = m_border.m_xs < rect.m_xe ? m_border.m_xs : rect.m_xe;
                    m_border.m_xe = m_border.m_xe > rect.m_xs ? m_border.m_xe : rect.m_xs;
                }
                else
                {
                    m_border.m_xs = m_border.m_xs < rect.m_xs ? m_border.m_xs : rect.m_xs;
                    m_border.m_xe = m_border.m_xe > rect.m_xe ? m_border.m_xe : rect.m_xe;
                }
                if (rect.m_ys > rect.m_ye)
                {
                    m_border.m_ys = m_border.m_ys < rect.m_ye ? m_border.m_ys : rect.m_ye;
                    m_border.m_ye = m_border.m_ye > rect.m_ys ? m_border.m_ye : rect.m_ys;
                }
                else
                {
                    m_border.m_ys = m_border.m_ys < rect.m_ys ? m_border.m_ys : rect.m_ys;
                    m_border.m_ye = m_border.m_ye > rect.m_ye ? m_border.m_ye : rect.m_ye;
                }
            }
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


        private void SelPoint(int point_id)
        {
            if (!this.SelPoints.ContainsKey(point_id))
            {
                this.SelPoints.Add(point_id, this.AllPoints[point_id]);
            }
            else
                this.SelPoints.Remove(point_id);
        }


        private void SelRect(int rect_id)
        {
            if (!this.SelRects.ContainsKey(rect_id))
            {
                this.SelRects.Add(rect_id, this.AllRects[rect_id]);
            }
            else
                this.SelRects.Remove(rect_id);
        }


        private void DrawLine(CADLine line, CADRGB color = null)
        {
            m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            if (color == null)
                m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            else
                m_openGLCtrl.Color(color.m_r, color.m_g, color.m_b);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);
            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }

        private void DrawPoint(CADPoint point, CADRGB color = null)
        {
            //m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.PointSize(3.0f);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Points);
            
            if (color == null)
                m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            else
                m_openGLCtrl.Color(color.m_r, color.m_g, color.m_b);
            m_openGLCtrl.Vertex(point.m_x, point.m_y);
            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }


        private void DrawGridLine(CADLine line)
        {
            m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(0.2f, 0.2f, 0.2f);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);
            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }

        private void DrawRect(CADRect rect, CADRGB color = null)
        {
            m_openGLCtrl.LineWidth(1);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            if (color == null)
                m_openGLCtrl.Color(1.0f, 1.0f, 1.0f);
            else
                m_openGLCtrl.Color(color.m_r, color.m_g, color.m_b);
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

        private void DrawSelLine(int line_id)
        {
            if (!AllLines.ContainsKey(line_id))
                return;
            CADLine line = AllLines[line_id];
            m_openGLCtrl.LineWidth(5);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(0.7f, 0.2f, 0.2f);
            m_openGLCtrl.Vertex(line.m_xs, line.m_ys);
            m_openGLCtrl.Vertex(line.m_xe, line.m_ye);

            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }

        private void DrawSelPoint(int point_id)
        {
            if (!AllPoints.ContainsKey(point_id))
                return;
            CADPoint point = AllPoints[point_id];
            m_openGLCtrl.PointSize(8.0f);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Points);
            m_openGLCtrl.Color(0.7f, 0.2f, 0.7f);
            m_openGLCtrl.Vertex(point.m_x, point.m_y);

            m_openGLCtrl.End();
            m_openGLCtrl.Flush();
        }


        private void DrawSelRect(int rect_id)
        {
            if (!AllRects.ContainsKey(rect_id))
                return;
            CADRect rect = AllRects[rect_id];

            m_openGLCtrl.LineWidth(5);
            m_openGLCtrl.Begin(SharpGL.Enumerations.BeginMode.Lines);
            m_openGLCtrl.Color(0.2f, 0.7f, 0.2f);
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

        private void DrawGrids()
        {

            CADLine line = new CADLine(0, 0, 0, 0);

            for (int i = 0; i <= (int)(this.Width / m_gridstep / 2) + 1; i++)
            {
                line.m_xs = (float)((i - (int)(m_center_offset.X / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_xe = (float)((i - (int)(m_center_offset.X / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_ys = (float)((-this.Height / 2 - m_center_offset.Y) / m_scale / m_pixaxis);
                line.m_ye = (float)((this.Height / 2 - m_center_offset.Y) / m_scale / m_pixaxis);
                this.DrawGridLine(line);
                line.m_xs = (float)((-i - (int)(m_center_offset.X / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_xe = (float)((-i - (int)(m_center_offset.X / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_ys = (float)((-this.Height / 2 - m_center_offset.Y) / m_scale / m_pixaxis);
                line.m_ye = (float)((this.Height / 2 - m_center_offset.Y) / m_scale / m_pixaxis);
                this.DrawGridLine(line);
            }

            for (int i = 0; i <= (int)(this.Height / m_gridstep / 2) + 1; i++)
            {
                line.m_ys = (float)((i - (int)(m_center_offset.Y / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_ye = (float)((i - (int)(m_center_offset.Y / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_xs = (float)((-this.Width / 2 - m_center_offset.X) / m_scale / m_pixaxis);
                line.m_xe = (float)((this.Width / 2 - m_center_offset.X) / m_scale / m_pixaxis);
                this.DrawGridLine(line);
                line.m_ys = (float)((-i - (int)(m_center_offset.Y / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_ye = (float)((-i - (int)(m_center_offset.Y / m_gridstep)) * (m_gridstep / m_scale / m_pixaxis));
                line.m_xs = (float)((-this.Width / 2 - m_center_offset.X) / m_scale / m_pixaxis);
                line.m_xe = (float)((this.Width / 2 - m_center_offset.X) / m_scale / m_pixaxis);
                this.DrawGridLine(line);
            }

        }



        private void DelLine(int line_id)
        {
            if (this.AllLines.ContainsKey(line_id))
            {
                foreach (int value in AllPointsInLines[line_id])
                {
                    AllPoints.Remove(value);
                    if (SelPoints.ContainsKey(value))
                        SelPoints.Remove(value);
                }
                AllPointsInLines.Remove(line_id);
                AllLines.Remove(line_id);
                AllLinesColor.Remove(line_id);
                
            }
            this.UpdateBorder();
        }

        private void DelAllLines()
        {
            foreach (int line_id in AllLines.Keys)
            {
                foreach (int value in AllPointsInLines[line_id])
                {
                    AllPoints.Remove(value);
                    if (SelPoints.ContainsKey(value))
                        SelPoints.Remove(value);
                }
            }
            AllPointsInLines.Clear();
            AllLines.Clear();
            AllLinesColor.Clear();
            SelLines.Clear();
            
            LineNumber = 0;
            this.UpdateBorder();
        }

        private void DelRect(int rect_id)
        {
            if (this.AllRects.ContainsKey(rect_id))
            {
                foreach (int value in AllPointsInRects[rect_id])
                {
                    AllPoints.Remove(value);
                    if (SelPoints.ContainsKey(value))
                        SelPoints.Remove(value);
                }
                AllPointsInRects.Remove(rect_id);
                AllRects.Remove(rect_id);
                AllRectsColor.Remove(rect_id);

            }
            this.UpdateBorder();
        }

        private void DelAllRects()
        {
            foreach (int rect_id in AllRects.Keys)
            {
                foreach (int value in AllPointsInRects[rect_id])
                {
                    AllPoints.Remove(value);
                    if (SelPoints.ContainsKey(value))
                        SelPoints.Remove(value);
                }
            }
            AllPointsInRects.Clear();
            AllRects.Clear();
            AllRectsColor.Clear();

            SelRects.Clear();
            RectNumber = 0;
            this.UpdateBorder();
        }

        private void UpdateBorder()
        {
            m_border = new CADRect(0, 0, 0, 0);
            foreach (CADRect rect in this.AllRects.Values)
            {
                if (rect.m_xs > rect.m_xe)
                {
                    m_border.m_xs = m_border.m_xs < rect.m_xe ? m_border.m_xs : rect.m_xe;
                    m_border.m_xe = m_border.m_xe > rect.m_xs ? m_border.m_xe : rect.m_xs;
                }
                else
                {
                    m_border.m_xs = m_border.m_xs < rect.m_xs ? m_border.m_xs : rect.m_xs;
                    m_border.m_xe = m_border.m_xe > rect.m_xe ? m_border.m_xe : rect.m_xe;
                }
                if (rect.m_ys > rect.m_ye)
                {
                    m_border.m_ys = m_border.m_ys < rect.m_ye ? m_border.m_ys : rect.m_ye;
                    m_border.m_ye = m_border.m_ye > rect.m_ys ? m_border.m_ye : rect.m_ys;
                }
                else
                {
                    m_border.m_ys = m_border.m_ys < rect.m_ys ? m_border.m_ys : rect.m_ys;
                    m_border.m_ye = m_border.m_ye > rect.m_ye ? m_border.m_ye : rect.m_ye;
                }
            }

            foreach (CADLine line in this.AllLines.Values)
            {
                if (line.m_xs > line.m_xe)
                {
                    m_border.m_xs = m_border.m_xs < line.m_xe ? m_border.m_xs : line.m_xe;
                    m_border.m_xe = m_border.m_xe > line.m_xs ? m_border.m_xe : line.m_xs;
                }
                else
                {
                    m_border.m_xs = m_border.m_xs < line.m_xs ? m_border.m_xs : line.m_xs;
                    m_border.m_xe = m_border.m_xe > line.m_xe ? m_border.m_xe : line.m_xe;
                }
                if (line.m_ys > line.m_ye)
                {
                    m_border.m_ys = m_border.m_ys < line.m_ye ? m_border.m_ys : line.m_ye;
                    m_border.m_ye = m_border.m_ye > line.m_ys ? m_border.m_ye : line.m_ys;
                }
                else
                {
                    m_border.m_ys = m_border.m_ys < line.m_ys ? m_border.m_ys : line.m_ys;
                    m_border.m_ye = m_border.m_ye > line.m_ye ? m_border.m_ye : line.m_ye;
                }
            }
        }

        private double GetDistance(Point point, CADLine line)
        {
            //if ((point.X - line.m_xs) * (point.X - line.m_xe) > 0 || (point.Y - line.m_ys) * (point.Y - line.m_ye) > 0)
            //    return -1;
            double result = -1;
            double a = line.m_ys - line.m_ye;
            double b = line.m_xe - line.m_xs;
            double c = line.m_xs * line.m_ye - line.m_ys * line.m_xe;
            
            CADLine normal = new CADLine(point.X,point.Y,point.X-a,point.Y+b);
            // 如果分母为0 则平行或共线, 不相交  
            double denominator = (line.m_ye - line.m_ys) * (normal.m_xe - normal.m_xs) - (line.m_xs - line.m_xe) * (normal.m_ys - normal.m_ye);
            if (Math.Abs(denominator) < 0.00005)
            {
                return result;
            }

            // 线段所在直线的交点坐标 (x , y)      
            double x = ((line.m_xe - line.m_xs) * (normal.m_xe - normal.m_xs) * (normal.m_ys - line.m_ys) + (line.m_ye - line.m_ys) * (normal.m_xe - normal.m_xs) * line.m_xs - (normal.m_ye - normal.m_ys) * (line.m_xe - line.m_xs) * normal.m_xs) / denominator;
            double y = -((line.m_ye - line.m_ys) * (normal.m_ye - normal.m_ys) * (normal.m_xs - line.m_xs) + (line.m_xe - line.m_xs) * (normal.m_ye - normal.m_ys) * line.m_ys - (normal.m_xe - normal.m_xs) * (line.m_ye - line.m_ys) * normal.m_ys) / denominator;

            if ((x - line.m_xs) * (x - line.m_xe) <= 0 && (y - line.m_ys) * (y - line.m_ye) <= 0)
                result = Math.Abs((a * point.X + b * point.Y + c) / Math.Sqrt(a * a + b * b));
            //GetCrossPoint(line, normal);
            return result;

        }

        private double GetDistance(Point point, CADPoint cad_point)
        {
            double result = 0.0;
            result = (cad_point.m_x - point.X) * (cad_point.m_x - point.X) + (cad_point.m_y - point.Y) * (cad_point.m_y - point.Y);
            return Math.Sqrt(result);
        }
        private double GetDistance(Point point, CADRect rect)
        {
            //if ((point.X - rect.m_xs) * (point.X - rect.m_xe) > 0 || (point.Y - rect.m_ys) * (point.Y - rect.m_ye) > 0)
            //    return -1;
            double result = -1;
            double dis = 0.0;
            CADLine line = new CADLine(rect.m_xs, rect.m_ys, rect.m_xs, rect.m_ye);
            result = this.GetDistance(point, line);
            line.m_xs = rect.m_xe;
            line.m_ys = rect.m_ye;
            dis = this.GetDistance(point, line);
            if (dis >= 0)
            {
                if (result > 0)
                    result = result < dis ? result : dis;
                else
                    result = dis;
            }
            line.m_xe = rect.m_xe;
            line.m_ye = rect.m_ys;
            dis = this.GetDistance(point, line);
            if (dis >= 0)
            {
                if (result > 0)
                    result = result < dis ? result : dis;
                else
                    result = dis;
            } 
            line.m_xs = rect.m_xs;
            line.m_ys = rect.m_ys;
            dis = this.GetDistance(point, line);
            if (dis >= 0)
            {
                if (result > 0)
                    result = result < dis ? result : dis;
                else
                    result = dis;
            }
            return result;
        }

        private CADPoint GetCrossPoint(CADLine line1, CADLine line2)
        {
            CADPoint point = null;
            // 如果分母为0 则平行或共线, 不相交  
            double denominator = (line1.m_ye - line1.m_ys) * (line2.m_xe - line2.m_xs) - (line1.m_xs - line1.m_xe) * (line2.m_ys - line2.m_ye);
            if (Math.Abs(denominator) < 0.00005)
            {
                return point;
            }

            // 线段所在直线的交点坐标 (x , y)      
            double x = ((line1.m_xe - line1.m_xs) * (line2.m_xe - line2.m_xs) * (line2.m_ys - line1.m_ys) + (line1.m_ye - line1.m_ys) * (line2.m_xe - line2.m_xs) * line1.m_xs - (line2.m_ye - line2.m_ys) * (line1.m_xe - line1.m_xs) * line2.m_xs) / denominator;
            double y = -((line1.m_ye - line1.m_ys) * (line2.m_ye - line2.m_ys) * (line2.m_xs - line1.m_xs) + (line1.m_xe - line1.m_xs) * (line2.m_ye - line2.m_ys) * line1.m_ys - (line2.m_xe - line2.m_xs) * (line1.m_ye - line1.m_ys) * line2.m_ys) / denominator;

            /** 2 判断交点是否在两条线段上 **/
            if (
                // 交点在线段1上  
                (x - line1.m_xs) * (x - line1.m_xe) <= 0 && (y - line1.m_ys) * (y - line1.m_ye) <= 0
                // 且交点也在线段2上  
                 && (x - line2.m_xs) * (x - line2.m_xe) <= 0 && (y - line2.m_ys) * (y - line2.m_ye) <= 0)

                // 返回交点p  
                point = new CADPoint(x, y);
            return point;
        }

        public void UserDrawLine(Point p1, Point p2, int color_id = 0)
        {
            CADLine line = new CADLine(p1, p2);
            this.AddLine(line, color_id);

            //CADRect rect = new CADRect(p1, p2);
            //this.AddRect(rect);

        }

        public void UserDrawLine(CADLine line, int color_id = 0)
        {
            this.AddLine(line, color_id);
        }

        public void UserDelAllLines()
        {
            this.DelAllLines();
        }

        public void UserDelAllRects()
        {
            this.DelAllRects();
        }

        public void UserDrawRect(Point p1, Point p2, int color_id = 0)
        {
            //CADLine line = new CADLine(p1, p2);
            //this.AddLine(line);

            CADRect rect = new CADRect(p1, p2);
            this.AddRect(rect, color_id);

        }

        public void UserDrawRect(CADRect rect, int color_id = 0)
        {
            this.AddRect(rect, color_id);
        }

        public void UserSelLine(int id)
        {
            this.SelLine(id);
        }

        public int[] UserGetSelLines()
        {
            int[] result = this.SelLines.Keys.ToArray();
            return result;
        }

        public int[] UserGetSelRects()
        {
            int[] result = this.SelRects.Keys.ToArray();
            return result;
        }

        public int[] UserGetSelPoints()
        {
            int[] result = this.SelPoints.Keys.ToArray();
            return result;
        }
        //Dictionary<int, CADLine> AllLines =
        //Dictionary<int, CADRect> AllRects =
        //Dictionary<int, CADPoint> AllPoints
        public Dictionary<int, CADLine> UserGetLines()
        {
            return AllLines;
        }

        public Dictionary<int, CADRect> UserGetRects()
        {
            return AllRects;
        }

        public Dictionary<int, CADPoint> UserGetPoints()
        {
            return AllPoints;
        }

        public void ZoomView()
        {
            //m_scale = 8 / ((m_border.m_ye - m_border.m_ys) > (m_border.m_xe - m_border.m_xs) ? (m_border.m_ye - m_border.m_ys) : (m_border.m_xe - m_border.m_xs));
            if ((m_border.m_ye - m_border.m_ys) > (m_border.m_xe - m_border.m_xs))
                m_scale = this.Height / 50 / (m_border.m_ye - m_border.m_ys);
            else
                m_scale = this.Width / 50 / (m_border.m_xe - m_border.m_xs);
            if (m_scale > 1000000)
                m_scale = 8 / (m_border.m_xe - m_border.m_xs);
            if (m_scale > 1000000)
                m_scale = 8 / (m_border.m_ye - m_border.m_ys);
            if (m_scale > 1000000)
                m_scale = 0.00001;
            m_center_offset.X = -(m_border.m_xe - m_border.m_xs) / 2 * m_pixaxis * m_scale;
            m_center_offset.Y = -(m_border.m_ye - m_border.m_ys) / 2 * m_pixaxis * m_scale;
        }
        //private void openGLCtrl_KeyDown(object sender, KeyEventArgs e)
        //{

        //}

        //private void openGLCtrl_KeyUp(object sender, KeyEventArgs e)
        //{

        //}






    }


    public class CADLine
    {
        public int m_id;
        public float m_xs = 0.0f;
        public float m_ys = 0.0f;
        public float m_xe = 0.0f;
        public float m_ye = 0.0f;

        public CADLine()
        {
        }

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
        public CADLine Copy()
        {
            CADLine result = new CADLine(m_xs, m_ys, m_xe, m_ye);
            result.m_id = m_id;
            return result;
        }
    }

    public class CADRect
    {
        public int m_id;
        public float m_xs = 0.0f;
        public float m_ys = 0.0f;
        public float m_xe = 0.0f;
        public float m_ye = 0.0f;


        public CADRect()
        {
        }

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

        public CADRect Copy()
        {
            CADRect result = new CADRect(m_xs, m_ys, m_xe, m_ye);
            result.m_id = m_id;
            return result;
        }

    }


    public class CADPoint
    {
        public int m_id;
        public float m_x = 0.0f;
        public float m_y = 0.0f;

        public CADPoint()
        { }


        public CADPoint(double x, double y)
        {
            m_x = (float)x;
            m_y = (float)y;
        }

        public CADPoint Copy()
        {
            CADPoint result = new CADPoint(m_x, m_y);
            result.m_id = m_id;
            return result;
        }
    }
    public class CADRGB
    {
        public float m_r = 0.0f;
        public float m_g = 0.0f;
        public float m_b = 0.0f;
        public CADRGB()
        { }
        public CADRGB(double r, double g, double b)
        {
            m_r = (float)r;
            m_g = (float)g;
            m_b = (float)b;
        }

        public CADRGB Copy()
        {
            return new CADRGB(m_r, m_g, m_b);
        }

        //public operator =
        //{

        //}
    }
}
