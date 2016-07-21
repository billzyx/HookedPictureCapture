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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lordeo.Framework;
using System.Drawing;
using LowLevelHooks.Keyboard;
using LowLevelHooks.Mouse;
using System.Configuration;
using System.Threading;


namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
     public partial class MainWindow : Window
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName,string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx",SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent,IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("gdi32.lib", EntryPoint = "CreateCompatibleBitmap")]
        private static extern int CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        const int WM_GETTEXT = 0x000D;
        const int WM_SETTEXT = 0x000C;
        const int WM_CLICK = 0x00F5;
        public MainWindow()
        {
            InitializeComponent();
            kHook = new KeyboardHook();
            mHook = new MouseHook();
            kHook.KeyEvent += KHookKeyEvent;
            mHook.MouseEvent += MHookMouseEvent;
            Closing += Form1FormClosing;
        }

        void Form1FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kHook.Dispose();
            mHook.Dispose();
            //if (mouseWriter != null)
            //{
            //    mouseWriter.Dispose();
            //}
            //if (keyboardWriter != null)
            //{
            //    keyboardWriter.Dispose();
            //}
        }

        void MHookMouseEvent(object sender, MouseHookEventArgs e)
        {
            WriteMouse(e);
        }

        private void WriteMouse(MouseHookEventArgs e)
        {
            if (e.MouseEventName.ToString().Equals("LeftButtonDown"))
                Thread.Sleep(2);

                shot();
            
           
            
        }

        void KHookKeyEvent(object sender, KeyboardHookEventArgs e)
        {
            //WriteKey(e);
        }

        //private void WriteKey(KeyboardHookEventArgs e)
        //{
        //    if (e.Char == '\0')
        //    {
        //        KeyboardWriter.Write(e.KeyString);
        //    }
        //    else
        //    {
        //        if (e.KeyboardEventName == KeyboardEventNames.KeyUp)
        //            KeyboardWriter.Write(e.KeyString);
        //    }
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (capturingMouse)
            {

                mHook.Unhook();
                //MouseWriter.Flush();
            }
            else
            {
                mHook.Hook();
            }
            capturingMouse = !capturingMouse;
            //shot();
        }

        private readonly KeyboardHook kHook;
        private readonly MouseHook mHook;
        private bool capturingKeys=false;
        private bool capturingMouse=false;

        



         void shot()
         {
             int retval = 0; //增加一个返回值用来判断操作是否成功
             string lpszParentClass = "Photo_Lightweight_Viewer"; //整个窗口的类名
             //string lpszParentWindow = "Form1"; //窗口标题
             string lpszClass = "Photos_PhotoCanvas"; //需要查找的子窗口的类名，也就是输入框
             //string lpszClass = "Edit";
             string lpszClass_Submit = "WindowsForms10.BUTTON.app.0.b7ab7b"; //需要查找的Button的类名
             //string lpszClass_Submit = "Button";
             string lpszName_Submit = "确定"; //需要查找的Button的标题
             string text = "";

             IntPtr ParenthWnd = new IntPtr(0);
             IntPtr EdithWnd = new IntPtr(0);

             lpszParentClass = GetAppConfig("lpszParentClass");

             //查到窗体，得到整个窗体
             ParenthWnd = FindWindow(lpszParentClass, null);



             //IntPtr init = (IntPtr)ComboBox_Windows.SelectedValue;

             System.Drawing.Bitmap m_Bitmap = PrtWindow(ParenthWnd);
             IntPtr ip = m_Bitmap.GetHbitmap();
             BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                 ip, IntPtr.Zero, Int32Rect.Empty,
                 System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
             bp_show.Source = bitmapSource;
             
             //DeleteObject(ip);
             
         }

         void shot(IntPtr ParenthWnd)
         {
             

             

             

             //查到窗体，得到整个窗体
             



             //IntPtr init = (IntPtr)ComboBox_Windows.SelectedValue;

             System.Drawing.Bitmap m_Bitmap = PrtWindow(ParenthWnd);
             IntPtr ip = m_Bitmap.GetHbitmap();
             BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                 ip, IntPtr.Zero, Int32Rect.Empty,
                 System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
             bp_show.Source = bitmapSource;

             //DeleteObject(ip);

         }


         private static string GetAppConfig(string strKey)
         {
             foreach (string key in ConfigurationManager.AppSettings)
             {
                 if (key == strKey)
                 {
                     return ConfigurationManager.AppSettings[strKey];
                 }
             }
             return null;
         }  
        

        public struct RECT
        {

            public int X1;

            public int Y1;

            public int X2;

            public int Y2;

        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

        public static Bitmap GetBitmap(RECT rect)
        {
            int width = rect.X2 - rect.X1;
            int height = rect.Y2 - rect.Y1;
            Bitmap image = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(rect.X1, rect.Y1, 0, 0, image.Size);

            return image;
        }

        public static Bitmap PrtWindow(IntPtr hWnd)
        {
            IntPtr hscrdc = Win32API.GetWindowDC(hWnd);
             Lordeo.Framework.RECT rect=new Lordeo.Framework.RECT();
            Win32API.GetWindowRect(hWnd, ref rect);
            //IntPtr hbitmap = Win32API.CreateCompatibleBitmap(hscrdc, rect.right - rect.left, rect.bottom - rect.top);
            //IntPtr hmemdc = Win32API.CreateCompatibleDC(hscrdc);
            //Win32API.SelectObject(hmemdc, hbitmap);
            
            //Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            //Win32API.DeleteDC(hscrdc);
            //Win32API.DeleteDC(hmemdc);

            Bitmap bit = new Bitmap(rect.right - rect.left, rect.bottom - rect.top);
            Graphics g = Graphics.FromImage(bit);
           // g.CopyFromScreen(new System.Drawing.Point(rect.left, rect.top), new System.Drawing.Point(rect.left, rect.top), bit.Size);
            g.CopyFromScreen(new System.Drawing.Point(rect.left, rect.top), new System.Drawing.Point(0, 0), bit.Size);

            return bit;
        }

        private delegate bool WNDENUMPROC(IntPtr hWnd, int lParam);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(WNDENUMPROC lpEnumFunc, int lParam);
        //[DllImport("user32.dll")]
        //private static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool EnumChildProc(WNDENUMPROC lpEnumFunc, int lParam);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpString, int nMaxCount);

        public struct WindowInfo
        {
            public IntPtr hWnd { get; set; }
            public string szWindowName { get; set; }
            public string szClassName { get; set; }
            public string display
            {
                get
                {
                    string re = Marshal.PtrToStringAnsi(hWnd) + " " + szWindowName + " " + szClassName;
                    return re;
                }
                set { this.display = display; }
            }
        }

        public WindowInfo[] GetAllDesktopWindows()
        {
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows
            EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);
                //get hwnd
                wnd.hWnd = hWnd;
                //get window name
                GetWindowTextW(hWnd, sb, sb.Capacity);
                //if (sb.ToString().Equals(""))
                //{ return false; }
                wnd.szWindowName = sb.ToString();
                //get window class
                GetClassNameW(hWnd, sb, sb.Capacity);
                wnd.szClassName = sb.ToString();
                //add it into list
                wndList.Add(wnd);
                
                return true;
            }, 0);

            return wndList.ToArray();
        }

        public WindowInfo[] GetChildWindows()
        {
            List<WindowInfo> wndList = new List<WindowInfo>();

            //enum all desktop windows
            EnumChildProc(delegate(IntPtr hWnd, int lParam)
            {
                WindowInfo wnd = new WindowInfo();
                StringBuilder sb = new StringBuilder(256);
                //get hwnd
                wnd.hWnd = hWnd;
                //get window name
                GetWindowTextW(hWnd, sb, sb.Capacity);
                //if (sb.ToString().Equals(""))
                //{ return false; }
                wnd.szWindowName = sb.ToString();
                //get window class
                GetClassNameW(hWnd, sb, sb.Capacity);
                wnd.szClassName = sb.ToString();
                //add it into list
                wndList.Add(wnd);

                return true;
            }, 0);

            return wndList.ToArray();
        }

        private void Button_FindAll_Click(object sender, RoutedEventArgs e)
        {
            WindowInfo[] win=GetAllDesktopWindows();
            ComboBox_Windows.ItemsSource = win;
            ComboBox_Windows.DisplayMemberPath = "display";
            ComboBox_Windows.SelectedValuePath = "hWnd";
        }

        


        

    }
}
