using System;
using System.Collections;
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
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using Microsoft.Win32;
using abaqus_helper.CADCtrl;

namespace abaqus_helper
{
    /// <summary>
    /// 钢筋强度枚举
    /// </summary>
    public enum RebarStrength
    {
        HPB235,
        HRB335,
        HRB400,
        RRB400,
    };
    
    /// <summary>
    /// 钢筋直径枚举
    /// </summary>
    //public enum RebarDiameter
    //{
    //    D6,
    //    D6.5,
    //    D8,
    //    D12,
    //    D14,
    //    D16,
    //    D18,
    //    D20,
    //    D22,
    //    D25,
    //    D28,
    //};
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
        public ObservableCollection<CADPoint> dataGrid_sel_rebar = null;
        public CADRect m_edit_cell = null;
        public CADRect m_add_rect = null;
        public CADRect m_sel_rect = null; 
        public CADPoint m_edit_rebar = null;
        public int m_cur_cell_val = 0;
        public string m_cur_col_name = "";
        private bool key_down_esc = false;
        Dictionary<int, CADRect>[] ctrl_all_rects = null;
        Dictionary<string, object[]> all_section_set = null;//键值为section名称，真值为一个序列，序列第一个是CADRect即截面矩形，而后是CADPoint即钢筋
        private int cur_floor;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            backThreader = new BackgroundWorker();
            InitializeBackgroundWorker();
            msgQueue = new Queue<int>();

            dataGrid_list = CADctrl_frame.m_sel_rect_list;
            dataGrid_sel.ItemsSource = dataGrid_list;
            dataGrid_sel_point = CADctrl_frame.m_sel_point_list;
            dataGrid_sel_rebar = CADctrl_rebar.m_sel_point_list;
            dataGrid_new.ItemsSource = dataGrid_sel_point;
            dataGrid_rebar.ItemsSource = dataGrid_sel_rebar;
            cur_floor = -1;
            #region
            comboBox_concrete.Items.Add("C20");
            comboBox_concrete.Items.Add("C25");
            comboBox_concrete.Items.Add("C30");
            comboBox_concrete.Items.Add("C35");
            comboBox_concrete.Items.Add("C40");
            comboBox_concrete.Items.Add("C50");
            comboBox_concrete.Items.Add("C60");
            comboBox_concrete.SelectedIndex = 0;

            comboBox_strength.Items.Add("HPB235");
            comboBox_strength.Items.Add("HRB335");
            comboBox_strength.Items.Add("HRB400");
            comboBox_strength.Items.Add("RRB400");
            comboBox_strength.SelectedIndex = 0;


            comboBox_diameter.Items.Add("6");
            comboBox_diameter.Items.Add("6.5");
            comboBox_diameter.Items.Add("8");
            comboBox_diameter.Items.Add("10");
            comboBox_diameter.Items.Add("12");
            comboBox_diameter.Items.Add("14");
            comboBox_diameter.Items.Add("16");
            comboBox_diameter.Items.Add("18");
            comboBox_diameter.Items.Add("20");
            comboBox_diameter.Items.Add("22");
            comboBox_diameter.Items.Add("25");
            comboBox_diameter.Items.Add("28");
            comboBox_diameter.SelectedIndex = 0;
            #endregion
            CADctrl_rebar.isRebar = 1;
            CADctrl_rebar.m_scale = 0.05;
            

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
      
            y_str = textBox_y.Text;
            Queue<int> y_input = new Queue<int>();
            y_input.Enqueue(0);
            y_str = y_str.Replace(",", " ");
            if (y_str == "")
            {
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
          
        }


        private void get_z_info()
        {
            string z_str = "";
         
            z_str = textBox_z.Text;
            Queue<int> z_input = new Queue<int>();
            z_str = z_str.Replace(",", " ");
            if (z_str == "")
            {
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
            if (ctrl_all_rects != null)
            {
                old_ctrl_all_rects = new Dictionary<int, CADRect>[ctrl_all_rects.Length];
                ctrl_all_rects.CopyTo(old_ctrl_all_rects, 0);

                ctrl_all_rects = new Dictionary<int, CADRect>[z_labels.Length];
                for (int i = 0; i < old_ctrl_all_rects.Length && i < z_labels.Length; i++)
                {
                    ctrl_all_rects[i] = old_ctrl_all_rects[i];
                }
            }
            else
            {
                ctrl_all_rects = new Dictionary<int, CADRect>[z_labels.Length];
                for (int i = 0; i < z_labels.Length; i++)
                {
                    Dictionary<int, CADRect> floor_concrete = new Dictionary<int, CADRect>();
                    ctrl_all_rects[i] = floor_concrete;
                }
            }

            comboBox_floor.Items.Clear();
            for (int i = 0; i < z_labels.Length; i++)
            {
                comboBox_floor.Items.Add(i + 1);
            }
            comboBox_floor.SelectedIndex = 0;
        }



        private void dataGrid_sel_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void dataGrid_sel_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!key_down_esc && m_edit_cell != null)
            {
                int cell_val = m_cur_cell_val;

                
                    switch (m_cur_col_name)
                    {
                        case "ID":
                            { } break;
                        case "起点X":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                    m_edit_cell.m_xs = cell_val;
                            } break;
                        case "起点Y":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                    m_edit_cell.m_ys = cell_val;
                            } break;
                        case "终点X":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                    m_edit_cell.m_xe = cell_val;
                            } break;
                        case "终点Y":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                    m_edit_cell.m_ye = cell_val;
                            } break;
                        case "是柱":
                            {
                                if ((bool)(e.EditingElement as CheckBox).IsChecked)
                                    m_edit_cell.m_flag = 1;
                                else
                                    m_edit_cell.m_flag = 0;
                            } break;
                        case "宽度":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                {
                                    m_edit_cell.m_width = cell_val;
                                    Point center = new Point((m_edit_cell.m_xs + m_edit_cell.m_xe) / 2, (m_edit_cell.m_ys + m_edit_cell.m_ye) / 2);
                                    m_edit_cell.m_xs = (float)center.X - cell_val / 2;
                                    m_edit_cell.m_xe = (float)center.X + cell_val / 2;
                                }
                            } break;
                        case "高度":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                {
                                    m_edit_cell.m_height = cell_val;
                                    Point center = new Point((m_edit_cell.m_xs + m_edit_cell.m_xe) / 2, (m_edit_cell.m_ys + m_edit_cell.m_ye) / 2);
                                    m_edit_cell.m_ys = (float)center.Y - cell_val / 2;
                                    m_edit_cell.m_ye = (float)center.Y + cell_val / 2;
                                }
                            } break;
                        case "深度":
                            {
                                if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                    m_edit_cell.m_len = cell_val;
                            } break;
                        default:
                            break;
                    }
                    if ((int)((m_edit_cell.m_xs - m_edit_cell.m_xe) * (m_edit_cell.m_xs - m_edit_cell.m_xe)) != 0)
                    {
                        m_edit_cell.UpdataWH();
                        this.CADctrl_frame.UserDrawRect(m_edit_cell);
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
            m_cur_col_name = e.Column.Header.ToString();
            //CADctrl_frame.key_down_esc = false;
            if (m_cur_col_name == "是柱")
            {
                if ((bool)(e.Column.GetCellContent(e.Row) as CheckBox).IsChecked)
                    m_cur_cell_val = 1;
                else
                    m_cur_cell_val = 0;
            }
            else
            {
                m_cur_cell_val = int.Parse((e.Column.GetCellContent(e.Row) as TextBlock).Text);
            }
            if (m_cur_cell_val == 0)
                m_cur_cell_val = 68;
            int rect_id = int.Parse((dataGrid_sel.Columns[0].GetCellContent(dataGrid_sel.Items[e.Row.GetIndex()]) as TextBlock).Text);
            for (int i = 0; i < dataGrid_list.Count; i++)
            {
                if (dataGrid_list[i].m_id == rect_id)
                {
                    m_edit_cell = dataGrid_list[i].Copy();
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
                    this.CADctrl_frame.UserDrawRect(m_add_rect);
                    //m_add_rect = null;
                }
                //else
            }
        }

        private void dataGrid_new_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            key_down_esc = false;
            int m_id = int.Parse((dataGrid_new.Columns[0].GetCellContent(e.Row) as TextBlock).Text);

            if (m_add_rect == null || m_add_rect.m_id != m_id)
            {
                m_add_rect = new CADRect();

                float m_x = float.Parse((dataGrid_new.Columns[1].GetCellContent(e.Row) as TextBlock).Text);
                float m_y = float.Parse((dataGrid_new.Columns[2].GetCellContent(e.Row) as TextBlock).Text);

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
                CADctrl_frame.ReactToESC();
                CADctrl_rebar.ReactToESC();
            }
            if (e.Key == Key.C)
            {

                //key_down_copy = true;
                CADctrl_frame.ReactToC();
                return;
            }
            if (e.Key == Key.M)
            {

                CADctrl_frame.ReactToM();
                return;
            }
            if (e.Key == Key.L)
            {
                CADctrl_frame.ReactToL();
                return;
            }

            if (e.Key == Key.Delete)
            {
                CADctrl_frame.ReactToDel();

                CADctrl_rebar.ReactToDel();
                return;
            }

        }



        private void button1_Click(object sender, RoutedEventArgs e)
        {
            textBox_x.Text = "4@4000";
            textBox_y.Text = "4@4000";
            textBox_z.Text = "4@4000";
        }

      

        private void btn_add_concrete_Click(object sender, RoutedEventArgs e)
        {
            string concrete = textBox_concrete.Text;
            Queue<int> concrete_input = new Queue<int>();
            concrete = concrete.Replace(",", " ");
            if (concrete == "")
            {
                this.statusBar.Content = "梁柱构件尺寸输入值无效";
                return;
            }
            if (dataGrid_sel_point.Count == 0)
            {
                this.statusBar.Content = "没有选中点";
                return;
            }
            string[] str_splited = concrete.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                concrete_input.Enqueue(int.Parse(str_splited[i]));
            }
            int[] data_concrete = concrete_input.ToArray();
            if (data_concrete.Length >= 3 && (int)(data_concrete[0] * data_concrete[1] * data_concrete[2]) > 0)
            {
                foreach (CADPoint value in dataGrid_sel_point)
                {
                    CADRect new_rect = new CADRect(value.m_x, value.m_y, value.m_x + data_concrete[0], value.m_y + data_concrete[1]);
                    if (radio_col.IsChecked == true)
                        new_rect.m_flag = 1;
                    else
                        new_rect.m_flag = 0;
                    new_rect.m_concrete = comboBox_concrete.SelectedIndex;
                    if (comboBox_rebar_style.SelectedIndex==-1)
                        new_rect.m_rebar = "";
                    else
                        new_rect.m_rebar = comboBox_rebar_style.SelectedValue.ToString();
                    new_rect.m_len = data_concrete[2];
                    new_rect.UpdataWH();
                    if (!CADctrl_frame.UserDrawRect(new_rect))
                    {
                        this.statusBar.Content = string.Format("点{0}处添加构件失败，已有同位置同大小构件", value.m_id);
                    }
                    else
                    {
                        this.statusBar.Content = string.Format("点{0}处添加构件完成", value.m_id);
                    }
                }
            }

        }

        private void btn_xyz_ok_Click(object sender, RoutedEventArgs e)
        {
            CADctrl_frame.UserDelAllLines();
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
                CADctrl_frame.UserDrawLine(line, 2);
            }
            line.m_ys = 0;
            line.m_ye = 0;
            for (int i = 0; i < y_labels.Length; i++)
            {

                line.m_ys = line.m_ys + y_labels[i];
                line.m_ye = line.m_ys;
                line.m_xs = 0;
                line.m_xe = xLength;
                CADctrl_frame.UserDrawLine(line, 2);
            }
            CADctrl_frame.ZoomView();

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
                    foreach (int value in CADctrl_frame.UserGetRects().Keys)
                    {
                        floor_concrete.Add(value, CADctrl_frame.UserGetRects()[value].Copy());
                    }
                    ctrl_all_rects[cur_floor - 1] = floor_concrete;
                }
                cur_floor = index;
            }

            if (ctrl_all_rects.Length >= index)
            {
                CADctrl_frame.UserDelAllRects();
                if (ctrl_all_rects[index - 1] == null || ctrl_all_rects[index - 1].Count == 0)
                {
                    this.statusBar.Content = string.Format("第{0}层未定义构件", index);
                    return;
                }
                foreach (int value in ctrl_all_rects[index - 1].Keys)
                {
                    ctrl_all_rects[index - 1][value].m_id = 0;
                    CADctrl_frame.UserDrawRect(ctrl_all_rects[index - 1][value]);
                }
            }

        }

        private void btn_update_section_Click(object sender, RoutedEventArgs e)
        {
            
            string section = textBox_concrete_section.Text;
            Queue<int> concrete_section_input = new Queue<int>();
            section = section.Replace(",", " ");
            if (section == "")
            {
                this.statusBar.Content = "梁柱截面尺寸输入值无效";
                return;
            }

            string[] str_splited = section.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                concrete_section_input.Enqueue(int.Parse(str_splited[i]));
            }
            int[] data_concrete_section = concrete_section_input.ToArray();
            if (data_concrete_section.Length >= 2 && data_concrete_section[0] * data_concrete_section[1] > 0)
            {
                CADRect new_rect = null;
                if (CADctrl_rebar.UserGetRects().Count == 0)
                    new_rect = new CADRect();
                else
                    new_rect = CADctrl_rebar.UserGetRects().ElementAt(0).Value.Copy();
                new_rect.m_xe = data_concrete_section[0];
                new_rect.m_ye = data_concrete_section[1];
               
                CADctrl_rebar.UserDrawRect(new_rect);
                this.statusBar.Content = string.Format("梁柱尺寸更新成功");
            }
            else if(data_concrete_section.Length ==1 && data_concrete_section[0]> 0)
            {
                CADRect new_rect = null;
                if (CADctrl_rebar.UserGetRects().Count == 0)
                    new_rect = new CADRect();
                else
                    new_rect = CADctrl_rebar.UserGetRects().ElementAt(0).Value.Copy();
                new_rect.m_xe = data_concrete_section[0];
                new_rect.m_ye = data_concrete_section[0];

                CADctrl_rebar.UserDrawRect(new_rect);
                this.statusBar.Content = string.Format("梁柱尺寸更新成功");
            }

        }

        private void btn_add_rebar_Click(object sender, RoutedEventArgs e)
        {
            string rebar_xy = textBox_rebar_xy.Text;
            Queue<int> rebar_xy_input = new Queue<int>();
            rebar_xy = rebar_xy.Replace(",", " ");
            if (rebar_xy == "")
            {
                this.statusBar.Content = "钢筋坐标输入值无效";
                return;
            }

            string[] str_splited = rebar_xy.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                rebar_xy_input.Enqueue(int.Parse(str_splited[i]));
            }
            int[] data_rebar_xy = rebar_xy_input.ToArray();
            if (data_rebar_xy.Length >= 2)
            {
                CADPoint new_rebar = new CADPoint(data_rebar_xy[0], data_rebar_xy[1]);
                new_rebar.m_is_rebar = 1;
                new_rebar.m_diameter = comboBox_diameter.SelectedIndex;
                new_rebar.m_strength =  comboBox_strength.SelectedIndex;
                foreach (CADPoint value in CADctrl_rebar.UserGetPoints().Values)
                {
                    if (value.m_is_rebar == 1 &&
                        ((int)(value.m_x - new_rebar.m_x))==0&&
                        ((int)(value.m_y - new_rebar.m_y))==0)
                    {
                        new_rebar.m_id = value.m_id;
                        CADctrl_rebar.UserDrawPoint(new_rebar);
                        this.statusBar.Content = string.Format("钢筋更新成功");
                        return;
                    }
                }
                bool flag = true;
                foreach(CADPoint value in CADctrl_rebar.UserGetPoints().Values)
                {
                    if (value.m_is_rebar == 1 &&
                        Math.Sqrt((value.m_x - new_rebar.m_x) * (value.m_x - new_rebar.m_x) + (value.m_y - new_rebar.m_y) * (value.m_y - new_rebar.m_y)) <= (value.m_diameter + new_rebar.m_diameter) / 2.0)
                    {
                        flag = false;
                        this.statusBar.Content = "新钢筋与其他钢筋过近，添加失败";
                        break;
                    }
                }
                if (flag)
                {
                    CADctrl_rebar.UserDrawPoint(new_rebar);
                    this.statusBar.Content = string.Format("钢筋添加成功");
                }
            }
            else if (data_rebar_xy.Length == 1)
            {
                CADPoint new_rebar = new CADPoint(data_rebar_xy[0], data_rebar_xy[0]);
                new_rebar.m_is_rebar = 1;
                new_rebar.m_diameter = comboBox_diameter.SelectedIndex;
                new_rebar.m_strength = comboBox_strength.SelectedIndex;
                foreach (CADPoint value in CADctrl_rebar.UserGetPoints().Values)
                {
                    if (value.m_is_rebar == 1 &&
                        ((int)(value.m_x - new_rebar.m_x)) == 0 &&
                        ((int)(value.m_y - new_rebar.m_y)) == 0)
                    {
                        new_rebar.m_id = value.m_id;
                        CADctrl_rebar.UserDrawPoint(new_rebar);
                        this.statusBar.Content = string.Format("钢筋更新成功");
                        return;
                    }
                }
                bool flag = true;
                foreach (CADPoint value in CADctrl_rebar.UserGetPoints().Values)
                {
                    if (value.m_is_rebar == 1 &&
                        Math.Sqrt((value.m_x - new_rebar.m_x) * (value.m_x - new_rebar.m_x) + (value.m_y - new_rebar.m_y) * (value.m_y - new_rebar.m_y)) <= (value.m_diameter + new_rebar.m_diameter) / 2.0)
                    {
                        flag = false;
                        this.statusBar.Content = "新钢筋与其他钢筋过近，添加失败";
                        break;
                    }
                }
                if (flag)
                {
                    CADctrl_rebar.UserDrawPoint(new_rebar);
                    this.statusBar.Content = string.Format("钢筋添加成功");
                }
            }

        }

        private void dataGrid_rebar_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {//m_edit_rebar
            Dictionary<int, CADRect> beam = this.CADctrl_rebar.UserGetRects();
            if (!beam.ContainsKey(1))
            {
                this.statusBar.Content = "截面未定义";
                return;
            }
            key_down_esc = false;
            m_cur_col_name = e.Column.Header.ToString();
            switch (m_cur_col_name)
            {
                case "X":
                    {
                        m_cur_cell_val = int.Parse((e.Column.GetCellContent(e.Row) as TextBlock).Text);
                    }break;
                case "Y":
                    {
                        m_cur_cell_val = int.Parse((e.Column.GetCellContent(e.Row) as TextBlock).Text);
                    } break;
                case "直径":
                    {
                        m_cur_cell_val = (e.Column.GetCellContent(e.Row) as ComboBox).SelectedIndex;
                    } break;
                case "强度":
                    {
                        m_cur_cell_val = (e.Column.GetCellContent(e.Row) as ComboBox).SelectedIndex;
                    } break;
                default:break;
            }
            int rebar_id = int.Parse((dataGrid_rebar.Columns[0].GetCellContent(dataGrid_rebar.Items[e.Row.GetIndex()]) as TextBlock).Text);
            for (int i = 0; i < dataGrid_sel_rebar.Count; i++)
            {
                if (dataGrid_sel_rebar[i].m_is_rebar==1 && dataGrid_sel_rebar[i].m_id == rebar_id)
                {
                    m_edit_rebar = dataGrid_sel_rebar[i].Copy();
                    break;
                }
            }

        }

        private void dataGrid_rebar_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!key_down_esc && m_edit_rebar != null)
            {
                int cell_val = m_cur_cell_val;
                switch (m_cur_col_name)
                {
                    case "X":
                        {
                            if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                m_edit_rebar.m_x = cell_val;
                            else
                                m_edit_rebar.m_x = m_cur_cell_val;
                        } break;
                    case "Y":
                        {
                            if (int.TryParse((e.EditingElement as TextBox).Text, out cell_val))
                                m_edit_rebar.m_y = cell_val;
                            else
                                m_edit_rebar.m_y = m_cur_cell_val;
                        } break;
                    case "直径":
                        {
                            cell_val = (e.Column.GetCellContent(e.Row) as ComboBox).SelectedIndex;
                            if (cell_val >= 0)
                                m_edit_rebar.m_diameter = cell_val;
                            else
                                m_edit_rebar.m_diameter = m_cur_cell_val;
                        } break;
                    case "强度":
                        {
                            cell_val = (e.Column.GetCellContent(e.Row) as ComboBox).SelectedIndex;
                            if (cell_val >= 0)
                                m_edit_rebar.m_strength = cell_val;
                            else
                                m_edit_rebar.m_strength = m_cur_cell_val;
                        } break;
                    default: break;
                }
                bool flag = true;
                foreach (CADPoint value in CADctrl_rebar.UserGetPoints().Values)
                {
                    if (m_edit_rebar.m_is_rebar != 1)
                        break;
                    if (value.m_is_rebar == 1 && value.m_id == m_edit_rebar.m_id)
                        continue;

                    if (value.m_is_rebar == 1 &&
                        ((int)(value.m_x - m_edit_rebar.m_x)) == 0 &&
                        ((int)(value.m_y - m_edit_rebar.m_y)) == 0)
                    {
                        flag = false;
                        this.statusBar.Content = string.Format("钢筋无法重复布置");
                        break;
                    }
                    if (value.m_is_rebar == 1 &&
                        Math.Sqrt((value.m_x - m_edit_rebar.m_x) * (value.m_x - m_edit_rebar.m_x) + (value.m_y - m_edit_rebar.m_y) * (value.m_y - m_edit_rebar.m_y)) <= (value.m_diameter + m_edit_rebar.m_diameter) / 2.0)
                    {
                        flag = false;
                        this.statusBar.Content = "新钢筋与其他钢筋过近，修改失败";
                        break;
                    }
                    Dictionary<int, CADRect>  beam = this.CADctrl_rebar.UserGetRects();
                    if (beam.ContainsKey(1))
                    {
                        if (m_edit_rebar.m_x + m_edit_rebar.m_diameter / 2 >= beam[1].m_width ||
                            m_edit_rebar.m_x - m_edit_rebar.m_diameter / 2 <= 0 ||
                            m_edit_rebar.m_y + m_edit_rebar.m_diameter / 2 >= beam[1].m_height ||
                            m_edit_rebar.m_y - m_edit_rebar.m_diameter / 2 <= 0)
                        {
                            flag = false;
                            this.statusBar.Content = "钢筋必须在截面内，添加失败";
                            break;
                        }
                    }
                    else
                    {
                        flag = false;
                        this.statusBar.Content = "截面未定义";
                        break;
                    }
                }

                if (flag && m_edit_rebar.m_is_rebar == 1)
                {
                    this.CADctrl_rebar.UserDrawPoint(m_edit_rebar);
                    this.CADctrl_rebar.UserSelPoint(m_edit_rebar.m_id);
                    this.CADctrl_rebar.UserSelPoint(m_edit_rebar.m_id);
                }
            } 
            m_edit_rebar = null;
            m_cur_col_name = "";
            m_cur_cell_val = 0;
            key_down_esc = false;
        }

        private void comboBox_rebar_style_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_rebar_style.SelectedValue == null)
                return;
            string select_name = comboBox_rebar_style.SelectedValue.ToString();
            object[] select_section_info = null;
            if(all_section_set.ContainsKey(select_name))
                select_section_info = all_section_set[select_name];
            if (select_section_info.Length > 0)
            {
                this.CADctrl_rebar.UserDelAllRects();
                this.CADctrl_rebar.UserDelAllPoints();
            }
            else
                return;
            for (int i = 0; i < select_section_info.Length; i++)
            {
                if (i == 0)
                {
                    this.CADctrl_rebar.UserDrawRect((CADRect)select_section_info[i]);
                    this.textBox_section_name.Text = comboBox_rebar_style.SelectedValue.ToString();
                    this.textBox_concrete_section.Text = string.Format("{0} {1}", ((CADRect)select_section_info[i]).m_width, ((CADRect)select_section_info[i]).m_height);
                }
                else
                    this.CADctrl_rebar.UserDrawPoint((CADPoint)select_section_info[i]);
            }

        }

        private void btn_add_section_Click(object sender, RoutedEventArgs e)
        {
            //此处集合CAD中的矩形大小，钢筋位置及钢筋强度直径，需规划好结构体
            //一旦确定将在comboBox_rebar_style(钢筋布置下拉框中添加或者更新style集合，最终梁柱只索引集合的键值即可)
            string section_name = "";
            if (textBox_section_name.Text.Replace(" ", "") == "")
            {
                this.statusBar.Content = "截面名称不能为空";
                return;
            }
            else
                section_name = textBox_section_name.Text;
            
            if (this.CADctrl_rebar.UserGetRects().Count == 0)
            {
                this.statusBar.Content = "请确定截面";
                return;
            }
            CADRect section_rect = this.CADctrl_rebar.UserGetRects().ElementAt(0).Value;
            int rebar_count = 0;
            for (int i = 0; i < this.CADctrl_rebar.UserGetPoints().Count;i++ )
            {
                CADPoint value = this.CADctrl_rebar.UserGetPoints().ElementAt(i).Value;
                if (value.m_is_rebar != 1)
                    continue;
                if (value.m_x + value.m_diameter / 2 >= section_rect.m_width ||
                            value.m_x - value.m_diameter / 2 <= 0 ||
                            value.m_y + value.m_diameter / 2 >= section_rect.m_height ||
                            value.m_y - value.m_diameter / 2 <= 0)
                {
                    this.CADctrl_rebar.UserDelPoint(value.m_id);
                    rebar_count--;
                    this.statusBar.Content = "有钢筋不在截面内，被删除";
                }
                rebar_count++;
            }
            object[] section_info = new object[rebar_count+1];
            section_info[0] = section_rect.Copy();
            rebar_count=0;
            foreach(CADPoint value in this.CADctrl_rebar.UserGetPoints().Values)
            {
                if(value.m_is_rebar==1)
                {
                    rebar_count++;
                    section_info[rebar_count]=value.Copy();
                }
            }
            if (all_section_set == null)
            {
                all_section_set = new Dictionary<string, object[]>();
            }
            if (all_section_set.ContainsKey(section_name))
                all_section_set[section_name] = section_info;
            else
            {
                all_section_set.Add(section_name, section_info);
                comboBox_rebar_style.Items.Add(section_name);
                comboBox_rebar_style.SelectedIndex = comboBox_rebar_style.Items.Count - 1;
            }
        }

        private void dataGrid_sel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_sel_rect = (CADRect)dataGrid_sel.SelectedItem;
            if (m_sel_rect != null)
            {
                m_sel_rect = ((CADRect)dataGrid_sel.SelectedItem).Copy();
                btn_concrete_update.Content = string.Format("更新{0}", m_sel_rect.m_id);
                textBox_concrete.Text = string.Format("{0} {1} {2}", m_sel_rect.m_width, m_sel_rect.m_height, m_sel_rect.m_len);
                if (m_sel_rect.m_flag == 1)
                {
                    radio_col.IsChecked = true;
                    radio_beam.IsChecked = false;
                }
                else
                {
                    radio_col.IsChecked = false;
                    radio_beam.IsChecked = true;
                }
                comboBox_concrete.SelectedIndex = m_sel_rect.m_concrete;
                if (m_sel_rect.m_rebar == "")
                    comboBox_rebar_style.SelectedIndex = -1;
                else
                    comboBox_rebar_style.SelectedValue = m_sel_rect.m_rebar;
            }
            else
                btn_concrete_update.Content = "更新";
        }

        private void btn_concrete_update_Click(object sender, RoutedEventArgs e)
        {
            if (m_sel_rect == null)
            {
                this.statusBar.Content = "没有可更新构件";
                return;
            }
            string concrete = textBox_concrete.Text;
            Queue<int> concrete_input = new Queue<int>();
            concrete = concrete.Replace(",", " ");
            if (concrete == "")
            {
                this.statusBar.Content = "梁柱构件尺寸输入值无效";
                return;
            }
            
            string[] str_splited = concrete.Split(' ');
            for (int i = 0; i < str_splited.Length; i++)
            {
                concrete_input.Enqueue(int.Parse(str_splited[i]));
            }
            int[] data_concrete = concrete_input.ToArray();
            if (data_concrete.Length >= 3 && (int)(data_concrete[0] * data_concrete[1] * data_concrete[2]) > 0)
            {
                m_sel_rect.m_xe = m_sel_rect.m_xs + data_concrete[0];
                m_sel_rect.m_ye = m_sel_rect.m_ys + data_concrete[1];
                m_sel_rect.m_len = data_concrete[2];
                if (radio_col.IsChecked == true)
                    m_sel_rect.m_flag = 1;
                else
                    m_sel_rect.m_flag = 0;
                m_sel_rect.m_concrete = comboBox_concrete.SelectedIndex;
                if (comboBox_rebar_style.SelectedIndex == -1)
                    m_sel_rect.m_rebar = "";
                else
                    m_sel_rect.m_rebar = comboBox_rebar_style.SelectedValue.ToString();
                m_sel_rect.UpdataWH();
                this.statusBar.Content = string.Format("构件{0}更新完成", m_sel_rect.m_id);
                this.CADctrl_frame.UserDrawRect(m_sel_rect);
            }
        }

        private void btn_output_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_floor.Items.Count <= 0)
            {
                this.statusBar.Content = "楼层布局信息未确认";
                return;
            }
            if (all_section_set == null)
            {
                this.statusBar.Content = "至少要有一个截面类型";
                return;
            }
            if (ctrl_all_rects.Length >= cur_floor)//保存当前楼层信息
            {
                Dictionary<int, CADRect> floor_concrete = new Dictionary<int, CADRect>();
                foreach (int value in CADctrl_frame.UserGetRects().Keys)
                {
                    floor_concrete.Add(value, CADctrl_frame.UserGetRects()[value].Copy());
                }
                ctrl_all_rects[cur_floor - 1] = floor_concrete;
            }
            //创建一个保存文件式的对话框  
            SaveFileDialog sfd = new SaveFileDialog();
            //设置这个对话框的起始保存路径  
            sfd.InitialDirectory = @"C:\";
            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "py文件|*.py";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == false)
                return;
            FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fs);
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            string str_in = 
@"# -*- coding:utf-8 -*-

# Do not delete the following import lines
from abaqus import *
from abaqusConstants import *
# mbcs
import time
import os
import re

import section
import regionToolset
import displayGroupMdbToolset as dgm
import part
import material
import assembly
import step
import interaction
import load
import mesh
import optimization
import job
import sketch
import visualization ###通过run-script执行该脚本时需引用这句
# from odbAccess import * ###官方解释，通过shell或cmd执行该脚本时需引用这句
import xyPlot
import displayGroupOdbToolset as dgo
import connectorBehavior

### 采用SI(mm)国际单位制
### abaqus cae startup=Beam.py为cmd下启动执行的py方法
class AbaqusStructureClass:

    def __init__(self,model='structure',file = 'D:/pymodel.cae'):
        #初始化信息
        if not os.path.exists('c:/Abaqus_TempSave'):
            os.makedirs('c:/Abaqus_TempSave')
        mdb.saveAs(pathName='c:/Abaqus_TempSave/abaqus_model_'+time.strftime('%Y%m%d-%H%M%S', time.localtime()))
        Mdb()
        self.modelName = model
        self.modelVolume = [0,0,0]
        self.filePath = file
        self.partLength = {}
        self.partCount = {}
        self.rebarStyle = {}
        self.partSize = {}
        self.allCols = {}
        self.colsPos = {}
        self.colsPart = {}
        self.colsBeamState = {} #用于存储四个布尔值表示柱四周梁的有无[up,down,left,right]
        self.colsColState = {} #用于存储两个布尔值表示柱子上下是否有柱子[up,down]
        self.allZBeams = {}
        self.zBeamsPos = {}
        self.zBeamsPart = {}
        self.allXBeams = {}
        self.xBeamsPos = {}
        self.xBeamsPart = {}

    def __del__(self):
        #退出前保存
        mdb.save()

    def startBuilding(self):

        mdb.models.changeKey(fromName='Model-1', toName=self.modelName)  # 重命名模型
        session.viewports['Viewport: 1'].setValues(displayedObject=None)  # 设置视口
        mdb.saveAs(pathName=self.filePath)  # 存储
";
            streamWriter.Write(str_in);
            int x_min = 0, x_max = 0, z_min = 0, z_max = 0,y_min=0,y_max=0;
            Queue<string> tmpqueue = new Queue<string>();
            for (int i = 0; i < ctrl_all_rects.Length; i++)
            {
                if (ctrl_all_rects[i] == null)
                    continue;
                int floor_height = 0;
                
                for (int j = 0; j < ctrl_all_rects[i].Count; j++)
                {
                    
                    CADRect item = ctrl_all_rects[i].ElementAt(j).Value;
                    if(item.m_flag==1)
                        floor_height = (int)item.m_len;
                    x_min = x_min < (int)(item.m_xs < item.m_xe ? item.m_xs : item.m_xe) ? x_min : (int)(item.m_xs < item.m_xe ? item.m_xs : item.m_xe);
                    x_max = x_max > (int)(item.m_xs > item.m_xe ? item.m_xs : item.m_xe) ? x_max : (int)(item.m_xs > item.m_xe ? item.m_xs : item.m_xe);
                    z_min = z_min < (int)(item.m_xs < item.m_xe ? item.m_xs : item.m_xe) ? z_min : (int)(item.m_xs < item.m_xe ? item.m_xs : item.m_xe);
                    z_max = z_max > (int)(item.m_xs > item.m_xe ? item.m_xs : item.m_xe) ? z_max : (int)(item.m_xs > item.m_xe ? item.m_xs : item.m_xe);
                    str_in = comboBox_concrete.Items[item.m_concrete].ToString();

                    if (!tmpqueue.Contains(str_in) && str_in != "")
                        tmpqueue.Enqueue(str_in);
                }
                y_max = y_max + floor_height;
            }
            str_in = string.Format("\t\tself.modelVolume = [{0}, {1}, {2}]\r\n",x_max,y_max,z_max);
            streamWriter.Write(str_in);
            for (int i = 0; i < tmpqueue.Count; i++)
            {
                str_in = string.Format("\t\tself.createMaterialConcrete('{0}')\r\n", tmpqueue.ElementAt(i));
                streamWriter.Write(str_in);
            }
            for (int i = 0; i < tmpqueue.Count; i++)
            {
                str_in = string.Format("\t\tself.createSectionConcrete('concrete-{0}','{0}')\r\n", tmpqueue.ElementAt(i));
                streamWriter.Write(str_in);
            }
            tmpqueue.Clear();

            Queue<string> queue_rebar = new Queue<string>();
            Queue<string> queue_strength = new Queue<string>();
            
            for (int i = 0; i < all_section_set.Count; i++)
            {
                for (int j = 1; j < all_section_set.ElementAt(i).Value.Length; j++)
                {
                    CADPoint point = (CADPoint)all_section_set.ElementAt(i).Value[j];
                    str_in = comboBox_strength.Items[point.m_strength].ToString();
                    if (!queue_strength.Contains(str_in))
                        queue_strength.Enqueue(str_in);
                    str_in = "rebar-r" + comboBox_diameter.Items[point.m_diameter].ToString()+"-"+str_in;
                    str_in = str_in.Replace(".", "_");
                    if (!queue_rebar.Contains(str_in))
                        queue_rebar.Enqueue(str_in);
                }
            }
            for (int i = 0; i < queue_strength.Count; i++)
            {
                str_in = string.Format("\t\tself.createMaterialSteel('{0}')\r\n", queue_strength.ElementAt(i));
                streamWriter.Write(str_in);
            }
            queue_strength.Clear();

            for (int i = 0; i < queue_rebar.Count; i++)
            {
                string info = queue_rebar.ElementAt(i);
                str_in = string.Format("\t\tself.createSectionRebar('{0}',{1},'{2}')\r\n", info, info.Substring(info.IndexOf("-") + 2, info.LastIndexOf("-") - info.IndexOf("-")-2).Replace("_", "."), info.Substring(info.LastIndexOf('-') + 1));
                streamWriter.Write(str_in);
            }
            queue_rebar.Clear();

            for (int i = 0; i < ctrl_all_rects.Length; i++)
            {
                for (int j = 0; j < ctrl_all_rects[i].Count; j++)
                {
                    CADRect item = ctrl_all_rects[i].ElementAt(j).Value;
                    if (item.m_flag == 1)
                    {
                        str_in = string.Format("{0}-{1}-{2}-{3}", item.m_width, item.m_height, item.m_len, comboBox_concrete.Items[item.m_concrete].ToString());
                    }
                    if (!tmpqueue.Contains(str_in) && str_in != "")
                        tmpqueue.Enqueue(str_in);
                }
            }
            for (int i = 0; i < tmpqueue.Count; i++)
            {
                str_in = string.Format("\t\tself.createCol('col-{0}',[{1}], 'concrete-{2}')\r\n", tmpqueue.ElementAt(i), tmpqueue.ElementAt(i).Substring(0, tmpqueue.ElementAt(i).LastIndexOf("-")).Replace('-', ','), tmpqueue.ElementAt(i).Substring(tmpqueue.ElementAt(i).LastIndexOf("-") + 1));
                streamWriter.Write(str_in);
            }
            tmpqueue.Clear();

            for (int i = 0; i < ctrl_all_rects.Length; i++)
            {
                if (ctrl_all_rects[i] == null)
                    continue;
                for (int j = 0; j < ctrl_all_rects[i].Count; j++)
                {
                    CADRect item = ctrl_all_rects[i].ElementAt(j).Value;
                    if (item.m_flag == 0)
                    {
                        str_in = string.Format("{0}-{1}-{2}-{3}", item.m_width < item.m_height ? item.m_width : item.m_height, item.m_len, item.m_width > item.m_height ? item.m_width : item.m_height, comboBox_concrete.Items[item.m_concrete].ToString());
                    }
                    else
                        str_in = "";
                    if (!tmpqueue.Contains(str_in) && str_in != "")
                        tmpqueue.Enqueue(str_in);
                }
            }
            for (int i = 0; i < tmpqueue.Count; i++)
            {
                str_in = string.Format("\t\tself.createBeam('beam-{0}',[{1}], 'concrete-{2}')\r\n", tmpqueue.ElementAt(i), tmpqueue.ElementAt(i).Substring(0, tmpqueue.ElementAt(i).LastIndexOf("-")).Replace('-', ','), tmpqueue.ElementAt(i).Substring(tmpqueue.ElementAt(i).LastIndexOf("-") + 1));
                streamWriter.Write(str_in);
            }
            tmpqueue.Clear();

            for (int i = 0; i < ctrl_all_rects.Length; i++)
            {
                if (ctrl_all_rects[i] == null)
                    continue;
                for (int j = 0; j < ctrl_all_rects[i].Count; j++)
                {
                    CADRect item = ctrl_all_rects[i].ElementAt(j).Value;
                    int length = (int)((item.m_width>item.m_height?item.m_width:item.m_height)>item.m_len?(item.m_width>item.m_height?item.m_width:item.m_height):item.m_len);
                    for (int k = 1; k < all_section_set[item.m_rebar].Length; k++)
                    {
                        CADPoint point = (CADPoint)all_section_set[item.m_rebar][k];
                        if (tmpqueue.Contains(string.Format("{0}-{1}-{2}", length, point.m_diameter, point.m_strength)))
                            continue;
                        else
                            tmpqueue.Enqueue(string.Format("{0}-{1}-{2}", length, point.m_diameter, point.m_strength));
                        str_in = string.Format("\t\tself.createRebar('rebar-{0}-{1}-{2}',[{0}],'rebar-r{1}-{2}')\r\n", length, comboBox_diameter.Items[point.m_diameter].ToString().Replace('.', '_'), comboBox_strength.Items[point.m_strength].ToString());
                        streamWriter.Write(str_in);
                    }
                }
            }
            streamWriter.Flush();
            streamWriter.Close();
            this.statusBar.Content = "写入完成";
        }
     
    }
}
