using DesktopWndView;
using Lordeo.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace pic_capture
{
    /// <summary>
    /// Window_Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Setting : Window
    {
        public Window_Setting()
        {
            InitializeComponent();
            init();
            Windows.Add(this);
        }

        List<Window> Windows = null;

        public Window_Setting(Window w)
        {
            InitializeComponent();
            init();
            Windows = new List<Window>();
            Windows.Add(w);
            Windows.Add(this);
        }

        private void init()
        {
            textbox_Image_hWnd.Text = AppConfig.GetAppConfig("Image_hWnd");
            textbox_lpszClass_Button.Text = AppConfig.GetAppConfig("lpszClass_Button");
            textbox_lpszClass_Image.Text = AppConfig.GetAppConfig("lpszClass_Image");
            textbox_lpszClass_Thumbnail.Text = AppConfig.GetAppConfig("lpszClass_Thumbnail");
            textbox_lpszParentClass.Text = AppConfig.GetAppConfig("lpszParentClass");
            textbox_Parent_hWnd.Text = AppConfig.GetAppConfig("Parent_hWnd");
            textbox_WidgetOffset.Text = AppConfig.GetAppConfig("WidgetOffset");
            checkbox_SavePic.IsChecked = AppConfig.GetAppConfig("SavePic").Equals("True");
        }

        private void button_confirm_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.SetAppConfig("Image_hWnd", textbox_Image_hWnd.Text);
            AppConfig.SetAppConfig("lpszClass_Button",textbox_lpszClass_Button.Text);
            AppConfig.SetAppConfig("lpszClass_Image",textbox_lpszClass_Image.Text);
            AppConfig.SetAppConfig("lpszClass_Thumbnail",textbox_lpszClass_Thumbnail.Text);
            AppConfig.SetAppConfig("lpszParentClass",textbox_lpszParentClass.Text);
            AppConfig.SetAppConfig("Parent_hWnd",textbox_Parent_hWnd.Text);
            AppConfig.SetAppConfig("WidgetOffset", textbox_WidgetOffset.Text);
            AppConfig.SetAppConfig("SavePic", checkbox_SavePic.IsChecked.ToString());
            this.Close();  
        }

        private void button_find_lpszParentClass_Click(object sender, RoutedEventArgs e)
        {
            findWindow(textbox_lpszParentClass, textbox_Parent_hWnd);
        }

        private void findWindow(TextBox tb,String type)
        {
            FindWindowDlg dlg = new FindWindowDlg(Windows);
            //设置主窗口指针
            
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int hwnd = dlg.HWND;
                if (hwnd > 0)
                {
                    switch (type)
                    {
                        case "class": tb.Text = getFullClass(new IntPtr(hwnd)); break;
                        case "hwnd": tb.Text = hwnd.ToString(); break;
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("未找到窗口!");
                    return;
                }
            }
        }

        private void findWindow(TextBox tb_class, TextBox tb_hWnd)
        {
            FindWindowDlg dlg = new FindWindowDlg(Windows);
            //设置主窗口指针

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int hwnd = dlg.HWND;
                if (hwnd > 0)
                {
                    tb_class.Text = getFullClass(new IntPtr(hwnd));
                    tb_hWnd.Text = hwnd.ToString(); 
                    return;
                }
                else
                {
                    MessageBox.Show("未找到窗口!");
                    return;
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
 

        private String getFullClass(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetClassName(hWnd,sb, 256);
            string fullClass = sb.ToString();
            IntPtr parrentWnd = Win32API.GetParent(hWnd);
            while(parrentWnd!=IntPtr.Zero)
            {
                sb = new StringBuilder(256);
                GetClassName(parrentWnd, sb, 256);
                fullClass = sb.ToString() + " " + fullClass;
                parrentWnd = Win32API.GetParent(parrentWnd);
            }
            return fullClass;
        }

        private void button_find_lpszClass_Image_Click(object sender, RoutedEventArgs e)
        {
            findWindow(textbox_lpszClass_Image, textbox_Image_hWnd);
        }

        private void button_find_lpszClass_Button_Click(object sender, RoutedEventArgs e)
        {
            findWindow(textbox_lpszClass_Button, "class");
        }

        private void button_find_lpszClass_Thumbnail_Click(object sender, RoutedEventArgs e)
        {
            findWindow(textbox_lpszClass_Thumbnail, "class");
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
