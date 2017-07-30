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
using System.ComponentModel;
using System.Windows.Threading;
using abaqus_helper.CADCtrl;
using System.Collections.ObjectModel;

namespace abaqus_helper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker backThreader = null;
        private Queue<int> msgQueue = null;
        public delegate object delegateGetTextCallBack(object text);
        public delegate void delegateSetTextCallBack(string str, object object_id);
        public delegate void delegateSetProcessCallBack(object value);
        public int[] x_labels = null;
        public int[] y_labels = null;
        public int[] z_labels = null;
        public ObservableCollection<CADRect> dataGrid_list = null;
        public ObservableCollection<CADPoint> dataGrid_sel_point = null;
        public CADRect m_edit_cell = null;
        public CADRect m_add_rect = null;
        public int m_cur_cell_val = 0;
        public string m_cur_col_name = "";
        private bool key_down_esc = false;
        Dictionary<int, CADRect>[] ctrl_all_rects = null;
        private int cur_floor;
        //private bool key_down_copy = false;
        //private bool key_down_move = false;
        //private bool key_down_del = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            backThreader = new BackgroundWorker();
            InitializeBackgroundWorker();
            msgQueue = new Queue<int>();
            //x_labels = new int[1];
            //y_labels = new int[1];
            dataGrid_list = CADctrl.m_sel_rect_list;
            dataGrid_sel.ItemsSource = dataGrid_list;
            dataGrid_sel_point = CADctrl.m_sel_point_list;
            dataGrid_new.ItemsSource = dataGrid_sel_point;
            cur_floor = -1;
            
        }

        private void InitializeBackgroundWorker()
        {
            backThreader.DoWork += new DoWorkEventHandler(backThreader_DoWork);
            backThreader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backThreader_RunWorkerCompleted);
            backThreader.ProgressChanged += new ProgressChangedEventHandler(backThreader_ProgressChanged);
        }

        private void backThreader_DoWork(object sender,
           DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (msgQueue.Count > 0)
                {
                    e.Result = RunMessage((int)msgQueue.Dequeue(), worker, e);
                }
            }
        }

        // This event handler deals with the results of the
        // background operation.
        private void backThreader_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void backThreader_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            //this.progressBar1.Value = e.ProgressPercentage;
        }

        private void PostMessage(int msg)
        {
            if (!backThreader.IsBusy)
                backThreader.RunWorkerAsync(0);
            if (msgQueue.Count > 200)
            {
                this.SetText("当前消息队列过于拥堵，暂停接收消息", this.statusBar);
                return;
            }
            msgQueue.Enqueue(msg);
        }

        private int RunMessage(int msg, BackgroundWorker worker, DoWorkEventArgs e)
        {
            switch (msg)//获取当前消息队列中消息，并一一比对执行相应的动作
            {
                case 1:
                    {
                        this.msgFunction_1();//例如消息码为1是，执行msgFunction_1()函数
                    } break;
                case 2:
                    {
                        this.msgFunction_2();//例如消息码为2是，执行msgFunction_2()函数
                    } break;
                //case 3:
                //    {
                //        msgFunction_3();//例如消息码为2是，执行msgFunction_2()函数
                //    } break;
                //case 4:
                //    {
                //        msgFunction_4();//例如消息码为2是，执行msgFunction_2()函数
                //    } break;
                default: break;
            }
            return 0;
        }

        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="object_id">text相关控件name</param>
        /// <returns>读取到的值</returns>
        public string GetText(object object_id)
        {
            string result = "";
            if (!Dispatcher.CheckAccess())
            {
                result = (string)Dispatcher.Invoke(DispatcherPriority.Normal, new delegateGetTextCallBack(GetTextCallBack), object_id);
            }
            return result;
        }
        /// <summary>
        /// delegate函数
        /// </summary>
        /// <param name="object_id">text相关控件name</param>
        /// <returns>读取到的值</returns>
        private object GetTextCallBack(object object_id)
        {
            string str = "";
            if (object_id is TextBox)
                str = ((TextBox)object_id).Text;
            if (object_id is TextBlock)
                str = ((TextBlock)object_id).Text;
            return str;
        }
        /// <summary>
        /// 执行函数
        /// </summary>
        /// <param name="str">要设置的值</param>
        /// <param name="object_id">text相关控件name</param>
        public void SetText(string str, object object_id)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new delegateSetTextCallBack(SetTextCallBack), str, object_id);
                return;
            }
        }
        /// <summary>
        /// delegate函数
        /// </summary>
        /// <param name="str">要设置的值</param>
        /// <param name="object_id">text相关控件name</param>
        private void SetTextCallBack(string str, object object_id)
        {
            if (object_id is TextBox)
                ((TextBox)object_id).Text = str;
            if (object_id is TextBlock)
                ((TextBlock)object_id).Text = str;
            if (object_id is System.Windows.Controls.Primitives.StatusBarItem)
                ((System.Windows.Controls.Primitives.StatusBarItem)object_id).Content = str;
        }

        private void SetProcessCallBack(object value)
        {
            this.progressBar.Value = (int)value;
        }

        private void SetProcess(int value)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new delegateSetProcessCallBack(SetProcessCallBack), value);
                return;
            }
        }

        private void msgFunction_1()
        {

        }

        private void msgFunction_2()
        {

        }

        private void get_x_info()
        {
            string x_str = "";
            //x_input_count = 0;
            //x_length = 0.0;
            //x_bridging_count = 0;
            //x_str = GetText(textBox_x);
            x_str = textBox_x.Text;
            Queue<int> x_input = new Queue<int>();
            x_input.Enqueue(0);
            x_str = x_str.Replace(",", " ");
            if (x_str == "")
            {
                //SetText("x输入值无效", this.statusBar);
                this.statusBar.Content = "x输入值无效";
                x_labels = x_input.ToArray();
                return;
            }
            string[] str_splited = x_str.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                if (str_splited[i].Contains('@'))
                {
                    int i_value = int.Parse(str_splited[i].Substring(str_splited[i].IndexOf('@') + 1));
                    int i_count = int.Parse(str_splited[i].Substring(0, str_splited[i].IndexOf('@')));
                    for (int j = 0; j < i_count; j++)
                    {
                        x_input.Enqueue(i_value);
                    }
                }
                else
                    x_input.Enqueue(int.Parse(str_splited[i]));
            }
            this.statusBar.Content = "就绪";
            x_labels = x_input.ToArray();

        }

        private void get_y_info()
        {
            string y_str = "";
            //y_input_count = 0;
            //y_length = 0.0;
            //y_bridging_count = 0;
            //y_str = GetText(textBoy_x);
            y_str = textBox_y.Text;
            Queue<int> y_input = new Queue<int>();
            y_input.Enqueue(0);
            y_str = y_str.Replace(",", " ");
            if (y_str == "")
            {
                //SetText("x输入值无效", this.statusBar);
                this.statusBar.Content = "y输入值无效";
                y_labels = y_input.ToArray();
                return;
            }
            string[] str_splited = y_str.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                if (str_splited[i].Contains('@'))
                {
                    int i_value = int.Parse(str_splited[i].Substring(str_splited[i].IndexOf('@') + 1));
                    int i_count = int.Parse(str_splited[i].Substring(0, str_splited[i].IndexOf('@')));
                    for (int j = 0; j < i_count; j++)
                    {
                        y_input.Enqueue(i_value);
                    }
                }
                else
                    y_input.Enqueue(int.Parse(str_splited[i]));
            }
            this.statusBar.Content = "就绪";
            y_labels = y_input.ToArray();
            //x_points = new double[x_input_count + 1];
            //x_points[0] = 0.0;
            //for (int i = 1; i < x_input_count + 1; i++)
            //{
            //    x_points[i] = x_points[i - 1] + x_input[i - 1];
            //}
            //x_length = x_points[x_points.Length - 1];

            //x_bridging_points = new double[100];
            //double temp_l1 = 0.0;
            //x_bridging_points[x_bridging_count++] = 0.0;
            //for (int i = 0; i < x_input_count; i++)
            //{
            //    temp_l1 = temp_l1 + x_input[i];
            //    if (temp_l1 > 4.5)
            //    {
            //        x_bridging_points[x_bridging_count++] = x_points[i];
            //        temp_l1 = x_input[i];
            //    }
            //}
            //if (x_bridging_points[x_bridging_count - 1] < x_length)
            //{
            //    x_bridging_points[x_bridging_count++] = x_length;
            //}
            //log.log("**********************************************");
            //for (int i = 0; i < x_input_count; i++)
            //{
            //    log.log(x_input[i].ToString());
            //}
            //Parent.FrameStatusBar
        }


        private void get_z_info()
        {
            string z_str = "";
            //z_input_count = 0;
            //z_length = 0.0;
            //z_bridging_count = 0;
            //z_str = GetText(textBoz_x);
            z_str = textBox_z.Text;
            Queue<int> z_input = new Queue<int>();
            //z_input.Enqueue(0);
            z_str = z_str.Replace(",", " ");
            if (z_str == "")
            {
                //SetText("x输入值无效", this.statusBar);
                this.statusBar.Content = "z输入值无效";
                z_labels = z_input.ToArray();
                return;
            }
            string[] str_splited = z_str.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                if (str_splited[i].Contains('@'))
                {
                    int i_value = int.Parse(str_splited[i].Substring(str_splited[i].IndexOf('@') + 1));
                    int i_count = int.Parse(str_splited[i].Substring(0, str_splited[i].IndexOf('@')));
                    for (int j = 0; j < i_count; j++)
                    {
                        z_input.Enqueue(i_value);
                    }
                }
                else
                    z_input.Enqueue(int.Parse(str_splited[i]));
            }
            this.statusBar.Content = "就绪";
            z_labels = z_input.ToArray();
            
            Dictionary<int, CADRect>[] old_ctrl_all_rects = null;
            ctrl_all_rects.CopyTo(old_ctrl_all_rects, 0);
            
            ctrl_all_rects = new Dictionary<int, CADRect>[z_labels.Length];
            for (int i = 0; i < old_ctrl_all_rects.Length; i++)
            {
                ct
            }

                cur_floor = -1;

            comboBox_floor.Items.Clear();
            for(int i=0;i<z_labels.Length;i++)
            {
                comboBox_floor.Items.Add(i+1); 
            }
            comboBox_floor.SelectedIndex = 0;

        }



        private void dataGrid_sel_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void dataGrid_sel_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

            //bool temp = e.Cancel;
            if (!key_down_esc && m_edit_cell != null)
            {
                int cell_val = m_cur_cell_val;

                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                {
                    switch (m_cur_col_name)
                    {
                        case "ID":
                            { } break;
                        case "起点X":
                            {
                                m_edit_cell.m_xs = cell_val;
                            } break;
                        case "起点Y":
                            {
                                m_edit_cell.m_ys = cell_val;
                            } break;
                        case "终点X":
                            {
                                m_edit_cell.m_xe = cell_val;
                            } break;
                        case "终点Y":
                            {
                                m_edit_cell.m_ye = cell_val;
                            } break;
                        case "是柱":
                            {
                                if (cell_val != 0)
                                    m_edit_cell.m_flag = 1;
                                else
                                    m_edit_cell.m_flag = 0;
                            } break;
                        case "宽度":
                            {
                                m_edit_cell.m_width = cell_val;
                                Point center = new Point((m_edit_cell.m_xs+m_edit_cell.m_xe)/2,(m_edit_cell.m_ys+m_edit_cell.m_ye)/2);
                                m_edit_cell.m_xs = (float)center.X - cell_val / 2;
                                m_edit_cell.m_xe = (float)center.X + cell_val / 2;
                            } break;
                        case "高度":
                            {
                                m_edit_cell.m_height = cell_val;
                                Point center = new Point((m_edit_cell.m_xs + m_edit_cell.m_xe) / 2, (m_edit_cell.m_ys + m_edit_cell.m_ye) / 2);
                                m_edit_cell.m_ys = (float)center.Y - cell_val / 2;
                                m_edit_cell.m_ye = (float)center.Y + cell_val / 2;
                            } break;
                        case "深度":
                            {
                                m_edit_cell.m_len = cell_val;
                            } break;
                        default:
                            break;
                    }
                    if ((int)((m_edit_cell.m_xs - m_edit_cell.m_xe) * (m_edit_cell.m_xs - m_edit_cell.m_xe)) != 0)
                    {
                        m_edit_cell.UpdataWH();
                        this.CADctrl.UserDrawRect(m_edit_cell);
                    }
                }
            }
            m_edit_cell = null;
            m_cur_col_name = "";
            m_cur_cell_val = 0;
            key_down_esc = false;
        }

        private void dataGrid_sel_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            key_down_esc = false;
            //CADctrl.key_down_esc = false;
            m_cur_cell_val = int.Parse((e.Column.GetCellContent(e.Row) as TextBlock).Text);
            if (m_cur_cell_val == 0)
                m_cur_cell_val = 68;
            m_cur_col_name = e.Column.Header.ToString();
            int rect_id = int.Parse((dataGrid_sel.Columns[0].GetCellContent(dataGrid_sel.Items[e.Row.GetIndex()]) as TextBlock).Text);
            for (int i = 0; i < dataGrid_list.Count; i++)
            {
                if (dataGrid_list[i].m_id == rect_id)
                {
                    m_edit_cell = dataGrid_list[i];
                    break;
                }
            }
        }


        private void dataGrid_new_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (m_add_rect == null)
                return;
            int m_id = int.Parse((dataGrid_new.Columns[0].GetCellContent(e.Row) as TextBlock).Text);
            if (m_id != m_add_rect.m_id)
            {
                m_add_rect = null;
                return;
            }
            string col_name = e.Column.Header.ToString();
            int cell_val = 0;
            if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
            {
                switch (col_name)
                {
                    case "ID":
                        { } break;
                    case "起点X":
                        {
                            m_add_rect.m_xs = cell_val;
                        } break;
                    case "起点Y":
                        {
                            m_add_rect.m_ys = cell_val;
                        } break;
                    case "终点X":
                        {
                            m_add_rect.m_xe = cell_val;
                        } break;
                    case "终点Y":
                        {
                            m_add_rect.m_ye = cell_val;
                        } break;
                    case "是柱":
                        {
                            if (cell_val != 0)
                                m_add_rect.m_flag = 1;
                            else
                                m_add_rect.m_flag = 0;
                        } break;
                    case "高度":
                        {
                            m_add_rect.m_len = cell_val;
                        } break;
                    default:
                        break;
                }
                if ((int)((m_add_rect.m_xs - m_add_rect.m_xe) * (m_add_rect.m_ys - m_add_rect.m_ye)) != 0)
                {//这里存在每次修改起始点数据就会触发addrect
                    m_add_rect.m_id = 0;
                    this.CADctrl.UserDrawRect(m_add_rect);
                    //m_add_rect = null;
                }
                //else
            }

            //int m_xs =0;
            //bool b_xs = int.TryParse((dataGrid_new.Columns[1].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_xs);
            //int m_ys =0;
            //bool b_ys = int.TryParse((dataGrid_new.Columns[2].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_ys);
            //int m_xe =0;
            //bool b_xe = int.TryParse((dataGrid_new.Columns[3].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_xe);
            //int m_ye =0;
            //bool b_ye = int.TryParse((dataGrid_new.Columns[4].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_ye);
            //int flag =0;
            //int len = 0;
            //if (int.TryParse((dataGrid_new.Columns[1].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_xs) &&
            //    int.TryParse((dataGrid_new.Columns[2].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_ys) &&
            //    int.TryParse((dataGrid_new.Columns[3].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_xe) &&
            //    int.TryParse((dataGrid_new.Columns[4].GetCellContent(dataGrid_new.Items[e.Row.GetIndex()]) as TextBlock).Text, out m_ye))
            //{
            //    if ((int)(m_xe - m_xs) * (m_ye - m_ys) != 0)
            //    {
            //        m_add_rect.m_id = 0;
            //        m_add_rect.m_xs = m_xs;
            //        m_add_rect.m_ys = m_ys;
            //        m_add_rect.m_xe = m_xe;
            //        m_add_rect.m_ye = m_ye;
            //        if (int.TryParse((dataGrid_new.Columns[5].GetCellContent(e.Row) as TextBlock).Text, out flag))
            //        {
            //            if (flag == 1 || flag == 0)
            //                m_add_rect.m_flag = flag;
            //        }
            //        if (int.TryParse((dataGrid_new.Columns[6].GetCellContent(e.Row) as TextBlock).Text, out len))
            //            m_add_rect.m_len = len;
            //        CADctrl.UserDrawRect(m_add_rect);
            //        m_add_rect = null;
            //    }
            //}
            //else
            //    m_add_rect = null;

        }

        private void dataGrid_new_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            key_down_esc = false;
            int m_id = int.Parse((dataGrid_new.Columns[0].GetCellContent(e.Row) as TextBlock).Text);
            if (m_add_rect == null || m_add_rect.m_id != m_id)
            {
                m_add_rect = new CADRect();

                int m_x = int.Parse((dataGrid_new.Columns[1].GetCellContent(e.Row) as TextBlock).Text);
                int m_y = int.Parse((dataGrid_new.Columns[2].GetCellContent(e.Row) as TextBlock).Text);

                m_add_rect.m_id = m_id;
                m_add_rect.m_xs = m_x;
                m_add_rect.m_ys = m_y;
            }

        }

 

        private void dataGrid_sel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                key_down_esc = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                key_down_esc = true;
                CADctrl.RectToESC();
            }
            if (e.Key == Key.C)
            {
                
                //key_down_copy = true;
                CADctrl.key_down_copy = true;
                CADctrl.key_down_move = false;
                CADctrl.key_down_del = false;
                return;
            }
            if (e.Key == Key.M)
            {

                CADctrl.key_down_copy = false;
                CADctrl.key_down_move = true;
                CADctrl.key_down_del = false;
                return;
            }
            if (e.Key == Key.Delete)
            {
                CADctrl.key_down_copy = false;
                CADctrl.key_down_move = false;
                CADctrl.key_down_del = true;
                int[] sel_keys = CADctrl.UserGetSelRects();
                foreach (int key in sel_keys)
                {
                    CADctrl.UserDelRect(key);
                }
                return;
            }

        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //CADctrl.UserDelAllRects();
            Dictionary<int, CADRect> ctrl_all_rects = CADctrl.UserGetRects();
            int[] sel_rect_id = CADctrl.UserGetSelRects();

            //dataGrid_list.Clear();
            //for(int i=0;i<sel_rect_id.Length;i++)
            //{
            //    dataGrid_list.Add(ctrl_all_rects[sel_rect_id[i]]);
            //    //sel_rect.Enqueue(ctrl_all_rects[sel_rect_id[i]]);
            //}
            //dataGrid_list = sel_rect.ToArray();
            //dataGrid_sel.ItemsSource = dataGrid_table;
            if (sel_rect_id.Length > 0)
            {
                ctrl_all_rects[sel_rect_id[0]].m_xe = ctrl_all_rects[sel_rect_id[0]].m_xe + 100;
                //CADRect new_rect = new CADRect(ctrl_all_rects[sel_rect_id[0]].m_xs, ctrl_all_rects[sel_rect_id[0]].m_ys, ctrl_all_rects[sel_rect_id[0]].m_xe + 100, ctrl_all_rects[sel_rect_id[0]].m_ye + 200);
                //new_rect.m_id = ctrl_all_rects[sel_rect_id[0]].m_id;
                this.CADctrl.UserDrawRect(ctrl_all_rects[sel_rect_id[0]]);
            }
            //CADctrl.UserDelLine(2);
            return;
            Dictionary<int, CADPoint> ctrl_all_points = CADctrl.UserGetPoints();
            int col_width = 300;
            int col_height = 500;
            CADRect col = new CADRect();
            foreach (int value in CADctrl.UserGetSelPoints())
            {
                col.m_xs = ctrl_all_points[value].m_x - col_width / 2;
                col.m_ys = ctrl_all_points[value].m_y + col_width / 2;
                col.m_xe = ctrl_all_points[value].m_x + col_width / 2;
                col.m_ye = ctrl_all_points[value].m_y - col_width / 2;
                CADctrl.UserDrawRect(col, 3);
            }
            //CADctrl.UserDrawRect(new Point(0,0),new Point(20000,10000));
            //CADctrl.UserDrawRect(new Point(0, 0), new Point(10000, 20000));
            //CADCtrl.UserDrawLine(new Point(10000, 0), new Point(10000, 20000));
            //CADCtrl.UserDrawLine(new Point(10000, 20000), new Point(0, 20000));
            //CADCtrl.UserDrawLine(new Point(0, 20000), new Point(0, 0));
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            CADctrl.UserDelAllLines();
            get_x_info();
            get_y_info();


            //CADctrl.UserDrawLine(line, 2);
            CADLine line = new CADLine(0, 0, 0, 0);
            int xLength = 0;
            for (int i = 0; i < x_labels.Length; i++)
            {
                xLength += x_labels[i];
            }
            int yLength = 0;
            for (int i = 0; i < y_labels.Length; i++)
            {
                yLength += y_labels[i];
            }
            for (int i = 0; i < x_labels.Length; i++)
            {

                line.m_xs = line.m_xs + x_labels[i];
                line.m_xe = line.m_xs;
                line.m_ys = 0;
                line.m_ye = yLength;
                CADctrl.UserDrawLine(line, 2);
            }
            line.m_ys = 0;
            line.m_ye = 0;
            for (int i = 0; i < y_labels.Length; i++)
            {

                line.m_ys = line.m_ys + y_labels[i];
                line.m_ye = line.m_ys;
                line.m_xs = 0;
                line.m_xe = xLength;
                CADctrl.UserDrawLine(line, 2);
            }
            CADctrl.UserDrawRect(new Point(0, 0), new Point(200, 100));
            CADctrl.UserDrawRect(new Point(0, 0), new Point(100, 200));

            CADctrl.ZoomView();
        }

        private void btn_add_concrete_Click(object sender, RoutedEventArgs e)
        {
            string concrete = textBox_concrete.Text;
            Queue<int> concrete_input = new Queue<int>();
            concrete = concrete.Replace(",", " ");
            if (concrete == "")
            {
                //SetText("x输入值无效", this.statusBar);
                this.statusBar.Content = "梁柱构件尺寸输入值无效";
                return;
            }
            if (dataGrid_sel_point.Count == 0)
            {
                //SetText("x输入值无效", this.statusBar);
                this.statusBar.Content = "没有选中点";
                return;
            }
            string[] str_splited = concrete.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                concrete_input.Enqueue(int.Parse(str_splited[i]));
            }
            int[] data_concrete = concrete_input.ToArray();
            if (data_concrete.Length >= 3 && (int)(data_concrete[0]*data_concrete[1]*data_concrete[2])>0)
            {
                foreach (CADPoint value in dataGrid_sel_point)
                {
                    CADRect new_rect = new CADRect(value.m_x, value.m_y, value.m_x + data_concrete[0], value.m_y + data_concrete[1]);
                    if (radio_col.IsChecked == true)
                        new_rect.m_flag = 1;
                    else
                        new_rect.m_flag = 0;
                    new_rect.m_len = data_concrete[2];
                    new_rect.UpdataWH();
                    CADctrl.UserDrawRect(new_rect);
                }
            }

        }

        private void btn_xyz_ok_Click(object sender, RoutedEventArgs e)
        {
            CADctrl.UserDelAllLines();
            get_x_info();
            get_y_info();
            get_z_info();
            
            CADLine line = new CADLine(0, 0, 0, 0);
            int xLength = 0;
            for (int i = 0; i < x_labels.Length; i++)
            {
                xLength += x_labels[i];
            }
            int yLength = 0;
            for (int i = 0; i < y_labels.Length; i++)
            {
                yLength += y_labels[i];
            }
            for (int i = 0; i < x_labels.Length; i++)
            {

                line.m_xs = line.m_xs + x_labels[i];
                line.m_xe = line.m_xs;
                line.m_ys = 0;
                line.m_ye = yLength;
                CADctrl.UserDrawLine(line, 2);
            }
            line.m_ys = 0;
            line.m_ye = 0;
            for (int i = 0; i < y_labels.Length; i++)
            {

                line.m_ys = line.m_ys + y_labels[i];
                line.m_ye = line.m_ys;
                line.m_xs = 0;
                line.m_xe = xLength;
                CADctrl.UserDrawLine(line, 2);
            }
            CADctrl.ZoomView();

        }

        private void comboBox_floor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (comboBox_floor.SelectedItem == null)
                return;
            int index = int.Parse(comboBox_floor.SelectedItem.ToString());
            if (cur_floor == -1)
            {
                cur_floor = index;
            }
            else
            {
                if (ctrl_all_rects.Length >= cur_floor)
                {
                    Dictionary<int, CADRect> floor_concrete = new Dictionary<int, CADRect>();
                    foreach (int value in CADctrl.UserGetRects().Keys)
                    {
                        floor_concrete.Add(value, CADctrl.UserGetRects()[value].Copy());
                    }
                    ctrl_all_rects[cur_floor - 1] = floor_concrete;
                }
                cur_floor = index;
 
            }
            //if (index == 2)
            //{
            //    int i = 0;
            //}
            if(ctrl_all_rects.Length>=index)
            {
                CADctrl.UserDelAllRects();
                if (ctrl_all_rects[index-1] == null || ctrl_all_rects[index-1].Count==0)
                {
                    this.statusBar.Content = string.Format("第{0}层未定义构件", index);
                    return;
                }
                foreach(int value in ctrl_all_rects[index-1].Keys)
                {
                    CADctrl.UserDrawRect(ctrl_all_rects[index-1][value]);
                }
                //Dictionary<int, CADRect> floor_concrete = new Dictionary<int,CADRect>();
                //foreach(int value in CADctrl.UserGetRects().Keys)
                //{
                //    floor_concrete.Add(value, CADctrl.UserGetRects()[value].Copy());
                //}
                //ctrl_all_rects[index] = floor_concrete;
            }
            
        }
    }
}
