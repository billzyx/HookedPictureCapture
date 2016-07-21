using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Lordeo.Framework;


namespace pic_capture
{
    class CaptureWindow
    {

        

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("gdi32.lib", EntryPoint = "CreateCompatibleBitmap")]
        private static extern int CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        public BitmapSource shot(IntPtr ParenthWnd)
        {
            if (Win32API.IsWindow(ParenthWnd)==0)
                return null;
            System.Drawing.Bitmap m_Bitmap = PrtWindow(ParenthWnd);
            IntPtr ip = m_Bitmap.GetHbitmap();
            if (ip == null)
                return null;
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }

        public BitmapSource shot(string lpszParentClass)
        {
            IntPtr ParenthWnd = new IntPtr(0);
            //查到窗体，得到整个窗体
            ParenthWnd = FindWindow(lpszParentClass, null);
            if (ParenthWnd.ToInt32() == 0)
                return null;
            System.Drawing.Bitmap m_Bitmap = PrtWindow(ParenthWnd);
            IntPtr ip = m_Bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
        }

        public BitmapSource shot(string lpszParentClass, string lpszClass)
        {
            IntPtr ParenthWnd = new IntPtr(0);
            IntPtr EdithWnd = new IntPtr(0);
            //查到窗体，得到整个窗体
            ParenthWnd = FindWindow(lpszParentClass, null);
            EdithWnd = FindWidgetWnd(lpszClass);
            if (EdithWnd.ToInt32() == 0)
                return null;
            System.Drawing.Bitmap m_Bitmap = PrtWindow(EdithWnd);
            IntPtr ip = m_Bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ip, IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ip);
            return bitmapSource;
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

        public Bitmap GetBitmap(RECT rect)
        {
            int width = rect.X2 - rect.X1;
            int height = rect.Y2 - rect.Y1;
            Bitmap image = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(rect.X1, rect.Y1, 0, 0, image.Size);

            return image;
        }

        public Bitmap PrtWindow(IntPtr hWnd)
        {
            IntPtr hscrdc = Win32API.GetWindowDC(hWnd);
            Lordeo.Framework.RECT rect = new Lordeo.Framework.RECT();
            Win32API.GetWindowRect(hWnd, ref rect);
            //IntPtr hbitmap = Win32API.CreateCompatibleBitmap(hscrdc, rect.right - rect.left, rect.bottom - rect.top);
            //IntPtr hmemdc = Win32API.CreateCompatibleDC(hscrdc);
            //Win32API.SelectObject(hmemdc, hbitmap);

            //Bitmap bmp = Bitmap.FromHbitmap(hbitmap);
            //Win32API.DeleteDC(hscrdc);
            //Win32API.DeleteDC(hmemdc);

            Bitmap bit = new Bitmap(rect.right - rect.left, rect.bottom - rect.top);
            Graphics g = Graphics.FromImage(bit);
            g.CopyFromScreen(new System.Drawing.Point(rect.left, rect.top), new System.Drawing.Point(0, 0), bit.Size);

            return bit;
        }

        private Lordeo.Framework.RECT getWindowRECT(IntPtr hWnd)
        {
            IntPtr hscrdc = Win32API.GetWindowDC(hWnd);
            Lordeo.Framework.RECT rect = new Lordeo.Framework.RECT();
            Win32API.GetWindowRect(hWnd, ref rect);
            return rect;
        }

        private Lordeo.Framework.RECT getWindowRECT(string WindowClass)
        {
            IntPtr hWnd = new IntPtr(0);
            //查到窗体，得到整个窗体
            hWnd = FindWindow(WindowClass, null);
            IntPtr hscrdc = Win32API.GetWindowDC(hWnd);
            Lordeo.Framework.RECT rect = new Lordeo.Framework.RECT();
            Win32API.GetWindowRect(hWnd, ref rect);
            return rect;
        }

        

        private List<Lordeo.Framework.RECT> getWindowRECT(string WindowClass,string WidgetClass)
        {
            IntPtr hWnd = new IntPtr(0);
            //查到窗体，得到整个窗体
            hWnd = FindWindow(WindowClass, null);
            IntPtr EdithWnd = new IntPtr(0);
            EdithWnd = FindWidgetWnd(WidgetClass);
            IntPtr ParentWnd = getWidgetParentWnd(WidgetClass);
            List<Lordeo.Framework.RECT> l= new List<Lordeo.Framework.RECT>();
            string[] w = WidgetClass.Split(' ');
            while (true)
            {
                IntPtr hscrdc = Win32API.GetWindowDC(EdithWnd);
                Lordeo.Framework.RECT rect = new Lordeo.Framework.RECT();
                Win32API.GetWindowRect(EdithWnd, ref rect);
                l.Add(rect);
                EdithWnd = FindWindowEx(ParentWnd, EdithWnd, w[w.Length-1] , "");
                if (EdithWnd.ToInt32() == 0)
                    break;
            }
            
            return l;
        }

        public bool isInWindow(System.Drawing.Point p,string WindowClass)
        {
            Lordeo.Framework.RECT rect = getWindowRECT(WindowClass);
            if (rect.bottom > p.Y && rect.top <  p.Y && rect.left < p.X && rect.right > p.X)
                return true;
            else
                return false;
        }

        public bool isInWindow(System.Drawing.Point p, IntPtr hWnd)
        {
            if (Win32API.IsWindow(hWnd) == 0)
                return false;
            Lordeo.Framework.RECT rect = getWindowRECT(hWnd);
            if (rect.bottom > p.Y && rect.top < p.Y && rect.left < p.X && rect.right > p.X)
                return true;
            else
                return false;
        }

        public bool isInWindow(System.Drawing.Point p, string WindowClass, string WidgetClass)
        {
            try
            {
                int WidgetOffset = Int32.Parse(AppConfig.GetAppConfig("WidgetOffset"));
                List<Lordeo.Framework.RECT> rect = getWindowRECT(WindowClass, WidgetClass);
                foreach (Lordeo.Framework.RECT r in rect)
                {
                    if (r.bottom + WidgetOffset > p.Y && r.top - WidgetOffset < p.Y && r.left - WidgetOffset < p.X && r.right + WidgetOffset > p.X)
                        return true;
                }

                return false;
            }
            catch(Exception e)
            {
                int WidgetOffset = 0;
                List<Lordeo.Framework.RECT> rect = getWindowRECT(WindowClass, WidgetClass);
                foreach (Lordeo.Framework.RECT r in rect)
                {
                    if (r.bottom + WidgetOffset > p.Y && r.top - WidgetOffset < p.Y && r.left - WidgetOffset < p.X && r.right + WidgetOffset > p.X)
                        return true;
                }

                return false;
            }
           
        }

        private IntPtr FindWidgetWnd(string WidgetClass)
        {
            string[] sArray = WidgetClass.Split(' ');
            IntPtr EdithWnd = new IntPtr(0);
            IntPtr tempWnd = new IntPtr(0);
            EdithWnd = FindWindowEx(tempWnd, EdithWnd, sArray[sArray.Length-1], null);
            if (EdithWnd != IntPtr.Zero)
                return EdithWnd;
            foreach (string i in sArray)
            {
                EdithWnd = new IntPtr(0);
                EdithWnd = FindWindowEx(tempWnd, EdithWnd, i, null);
                tempWnd = EdithWnd;
            }
            return EdithWnd;
        }

        private IntPtr getWidgetParentWnd(string WidgetClass)
        {
            string[] sArray = WidgetClass.Split(' ');
            IntPtr EdithWnd = new IntPtr(0);
            IntPtr tempWnd = new IntPtr(0);
            EdithWnd = FindWindowEx(tempWnd, EdithWnd, sArray[sArray.Length - 1], null);
            if (EdithWnd != IntPtr.Zero)
                return IntPtr.Zero;
            for (int i = 0; i < sArray.Length - 1; i++)
            {
                EdithWnd = new IntPtr(0);
                EdithWnd = FindWindowEx(tempWnd, EdithWnd, sArray[i], null);
                tempWnd = EdithWnd;
            }
            return EdithWnd;
        }

        

    }
}
