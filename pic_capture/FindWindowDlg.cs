using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows;

namespace DesktopWndView
{
	/// <summary>
	/// FindWindowDlg 的摘要说明。
	/// </summary>
	public class FindWindowDlg : System.Windows.Forms.Form
	{
		#region 自定义变量
		public const string DescriptionInTextBox=
			"窗口类：向系统注册窗口类时指定的窗口类名称，例如Edit,Button,SysListView32等。\r\n"
			+"\r\n窗口名称：对一个窗口来说是窗口的标题，对一个控件来说是控件的文本。\r\n"
			+"\r\n注意：\r\n(1)在窗口类和窗口名称中可以输入空字符串，空字符串会被作为NULL处理。\r\n"
			+"\r\n(2)查找结果是符合条件的所有匹配窗口中的第一个。";
		//匹配标志定义
		public const int MF_NONE=0;
		public const int MF_FULLWORD=1;
		public const int MF_UPPERLOWER=2;

		
		//自定义光标
		private Cursor m_Cursor;
		//光标形状在图片框中占据的客户区矩形
		private static Rectangle RC_CURSORPOS=new Rectangle(7,7,18,18);
		//用查找工具找到的窗口
		private int m_HWND=0;
		//找到的前一个窗口
		private int m_PreHWND=0;
		//鼠标是否正在查找窗口的过程中（即鼠标是否在按下状态）
		private bool b_FindingWnd=false;
		//记录查找窗口的点
		private POINT m_POINT=new POINT(0,0);
		//记录自身所在的屏幕坐标矩形
		private RECT m_Bounds = new RECT();

		//换算成屏幕坐标的点
		//private Point m_ScreenPoint;
		//找到的窗口的信息
		private WndInfo m_WndInfo;
        //用于API函数的缓冲区，因为频繁使用，所以设为类成员变量
        private StringBuilder m_StringBuilder = new StringBuilder(256);
        //API回调函数（在c＃中，一个委托实例代表一个函数的指针（入口地址））
        public delegate bool WNDENUMPROC(int hwnd, int lParam);
        //枚举子窗口回调函数指针
        private WNDENUMPROC lpEnumChildFunc;
        //枚举线程窗口回调函数指针
        private WNDENUMPROC lpEnumThreadFunc;
        //用于判断线程下是否有窗口的回调函数，如果有，则添加一个临时节点
        private WNDENUMPROC lpAddTempChild2ThreadNode;
        //枚举桌面顶级窗口的回调函数指针
        private WNDENUMPROC lpEnumDesktopFunc;
        //保存当前窗口的矩形
        private RECT m_Rect = new RECT(0, 0, 0, 0);
        //定时器寿命（也就是突出显示的绘制次数，注意为了复原，寿命应为偶数
        private int m_TimerLife;
        //是否需要在特定操作之前提醒用户
        private bool b_AskForConfirm;
		#endregion

        #region API define

        /*
		 * Message ID Constants
		 */
        public const int WM_PAINT = 0x000F;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_ENABLE = 0x000A;
        public const int WM_SETTEXT = 0x000C;
        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WM_CLOSE = 0x0010;
        public const int WM_QUIT = 0x0012;

        //获取和设置图标
        public const int WM_GETICON = 0x007F;
        public const int WM_SETICON = 0x0080;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;	//XP系统中使用
        /*
         * Button Control Messages
         */
        public const int BM_GETCHECK = 0x00F0;
        public const int BM_SETCHECK = 0x00F1;
        public const int BM_GETSTATE = 0x00F2;
        public const int BM_SETSTATE = 0x00F3;
        public const int BM_SETSTYLE = 0x00F4;
        public const int BM_CLICK = 0x00F5;	//单击按钮
        public const int BM_GETIMAGE = 0x00F6;
        public const int BM_SETIMAGE = 0x00F7;
        public const int BST_UNCHECKED = 0x0000;
        public const int BST_CHECKED = 0x0001;
        public const int BST_INDETERMINATE = 0x0002;
        public const int BST_PUSHED = 0x0004;
        public const int BST_FOCUS = 0x0008;

        /*
         * 键盘消息
         */
        public const int WM_KEYFIRST = 0x0100;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        public const int WM_DEADCHAR = 0x0103;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_SYSCHAR = 0x0106;
        public const int WM_SYSDEADCHAR = 0x0107;
        public const int WM_UNICHAR = 0x0109;			//XP系统
        public const int WM_KEYLAST = 0x0109;
        public const int UNICODE_NOCHAR = 0xFFFF;

        /*
         * 鼠标消息ID
         */
        public const int WM_MOUSEFIRST = 0x0200;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_MOUSEWHEEL = 0x020A;	//滚轮消息
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;
        public const int WM_XBUTTONDBLCLK = 0x020D;

        /*
         * Key State Masks for Mouse Messages
         */
        public const int MK_LBUTTON = 0x0001;
        public const int MK_RBUTTON = 0x0002;
        public const int MK_SHIFT = 0x0004;
        public const int MK_CONTROL = 0x0008;
        public const int MK_MBUTTON = 0x0010;
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;
        /*
         * ShowWindow() Constants
         */
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;		//显示但不激活窗口，一个关键参数
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;

        /*
         * GetWindow() Constants
        */
        public const uint GW_HWNDFIRST = 0;
        public const uint GW_HWNDLAST = 1;
        public const uint GW_HWNDNEXT = 2;
        public const uint GW_HWNDPREV = 3;
        public const uint GW_OWNER = 4;
        public const uint GW_CHILD = 5;
        public const uint GW_ENABLEDPOPUP = 6;
        public const uint GW_MAX = 6;	//(低版本系统中定义为5)

        /* Binary raster ops */
        public const int R2_BLACK = 1;   /*  0       */
        public const int R2_NOTMERGEPEN = 2;   /* DPon     */
        public const int R2_MASKNOTPEN = 3;   /* DPna     */
        public const int R2_NOTCOPYPEN = 4;   /* PN       */
        public const int R2_MASKPENNOT = 5;   /* PDna     */
        public const int R2_NOT = 6;   /* Dn       */
        public const int R2_XORPEN = 7;   /* DPx      */
        public const int R2_NOTMASKPEN = 8;   /* DPan     */
        public const int R2_MASKPEN = 9;   /* DPa      */
        public const int R2_NOTXORPEN = 10;  /* DPxn     */
        public const int R2_NOP = 11;  /* D        */
        public const int R2_MERGENOTPEN = 12;  /* DPno     */
        public const int R2_COPYPEN = 13;  /* P        */
        public const int R2_MERGEPENNOT = 14;  /* PDno     */
        public const int R2_MERGEPEN = 15;  /* DPo      */
        public const int R2_WHITE = 16;  /*  1       */
        public const int R2_LAST = 16;

        /* Ternary raster operations 光栅操作码，BitBlt函数的参数 */
        public const int SRCCOPY = 0x00CC0020; /* dest = source                   */
        public const int SRCPAINT = 0x00EE0086; /* dest = source OR dest           */
        public const int SRCAND = 0x008800C6; /* dest = source AND dest          */
        public const int SRCINVERT = 0x00660046; /* dest = source XOR dest          */
        public const int SRCERASE = 0x00440328; /* dest = source AND (NOT dest )   */
        public const int NOTSRCCOPY = 0x00330008; /* dest = (NOT source)             */
        public const int NOTSRCERASE = 0x001100A6; /* dest = (NOT src) AND (NOT dest) */
        public const int MERGECOPY = 0x00C000CA; /* dest = (source AND pattern)     */
        public const int MERGEPAINT = 0x00BB0226; /* dest = (NOT source) OR dest     */
        public const int PATCOPY = 0x00F00021; /* dest = pattern                  */
        public const int PATPAINT = 0x00FB0A09; /* dest = DPSnoo                   */
        public const int PATINVERT = 0x005A0049; /* dest = pattern XOR dest         */
        public const int DSTINVERT = 0x00550009; /* dest = (NOT dest)               */
        public const int BLACKNESS = 0x00000042; /* dest = BLACK                    */
        public const int WHITENESS = 0x00FF0062; /* dest = WHITE                    */

        /* Pen Styles */
        public const int PS_SOLID = 0;
        public const int PS_DASH = 1;       /* -------  */
        public const int PS_DOT = 2;       /* .......  */
        public const int PS_DASHDOT = 3;       /* _._._._  */
        public const int PS_DASHDOTDOT = 4;       /* _.._.._  */
        public const int PS_NULL = 5;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_USERSTYLE = 7;
        public const int PS_ALTERNATE = 8;
        public const int PS_STYLE_MASK = 0x0000000F;

        public const int PS_ENDCAP_ROUND = 0x00000000;
        public const int PS_ENDCAP_SQUARE = 0x00000100;
        public const int PS_ENDCAP_FLAT = 0x00000200;
        public const int PS_ENDCAP_MASK = 0x00000F00;

        public const int PS_JOIN_ROUND = 0x00000000;
        public const int PS_JOIN_BEVEL = 0x00001000;
        public const int PS_JOIN_MITER = 0x00002000;
        public const int PS_JOIN_MASK = 0x0000F000;

        public const int PS_COSMETIC = 0x00000000;
        public const int PS_GEOMETRIC = 0x00010000;
        public const int PS_TYPE_MASK = 0x000F0000;

        #endregion

		#region 窗体设计器变量

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbWndClass;
		private System.Windows.Forms.TextBox tbWindowName;
		private System.Windows.Forms.Button btOk;
		private System.Windows.Forms.Button btCancel;
		private System.Windows.Forms.Button btExpand;
		private System.Windows.Forms.TextBox tbDescription;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox cbMapFullword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox cbMapUpperLower;
		private System.Windows.Forms.CheckBox cbHideMainFrm;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.PictureBox pbFindWnd;

		#endregion
		private System.Windows.Forms.TextBox tbWndHandle;
		private System.Windows.Forms.Label LableMousePos;
		private Label label6;

		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FindWindowDlg()
		{
			InitializeComponent();
			//设置文本框的说明
			this.tbDescription.Text=DescriptionInTextBox;
			//加载光标
			this.LoadCursor();
			this.tbWindowName.Focus();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
		}

        List<Window> Windows = null;

        public FindWindowDlg(List<Window> Windows)
        {
            InitializeComponent();
            //设置文本框的说明
            this.tbDescription.Text = DescriptionInTextBox;
            //加载光标
            this.LoadCursor();
            this.tbWindowName.Focus();
            this.Windows = Windows;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
        }

        ~FindWindowDlg()
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Windows != null)
            {
                foreach (Window w in Windows)
                {
                    w.Visibility = Visibility.Visible;
                }
            }
        }

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
            
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

        #region P/Invoke USER32
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public RECT(int _left, int _top, int _right, int _bottom)
            {
                Left = _left;
                Top = _top;
                Right = _right;
                Bottom = _bottom;
            }
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //Declare wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public POINT(int _x, int _y)
            {
                X = _x;
                Y = _y;
            }
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public int hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            public byte[] rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LOGPEN
        {
            public uint lopnStyle;
            POINT lopnWidth;
            int lopnColor;
        }

        /// <summary>
        /// 判断一个点是否位于矩形内
        /// </summary>
        /// <param name="lprc"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern bool PtInRect(
            ref RECT lprc,
            POINT pt
            );

        [DllImport("user32", EntryPoint = "SetParent")]
        public static extern int SetParent(
            IntPtr hwndChild,
            int hwndNewParent
            );
        [DllImport("user32", EntryPoint = "FindWindowA")]
        public static extern int FindWindow(
            string lpClassName,
            string lpWindowName
            );
        [DllImport("user32", EntryPoint = "FindWindowExA")]
        public static extern int FindWindowEx(
            int hwndParent,
            int hwndChildAfter,
            string lpszClass,		//窗口类
            string lpszWindow		//窗口标题
            );

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
            int hWnd,
            int wMsg,
            int wParam,
            IntPtr lParam
            );
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
            int hWnd,
            int wMsg,
            int wParam,
            int lParam
            );

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(
            int hWnd,
            int wMsg,
            int wParam,
            string lParam
            );

        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(
            int hWnd,
            int wMsg,
            int wParam,
            StringBuilder lParam
            );

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(
            int hWnd,
            ref int lpdwProcessId);

        [DllImport("user32")]
        public static extern int Sleep(
            int dwMilliseconds
            );

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(
            int hWnd,
            ref RECT lpRect
            );

        [DllImport("user32")]
        public static extern int GetWindowText(
            int hWnd,
            StringBuilder lpString,
            int nMaxCount
            );


        [DllImport("user32.dll")]
        public static extern bool ShowWindow(
            int hWnd,
            int nCmdShow
            );

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        public static extern uint SetWindowLong(
            IntPtr hwnd,
            int nIndex,
            uint dwNewLong
            );
        [DllImport("user32", EntryPoint = "GetWindowLong")]
        public static extern uint GetWindowLong(
            IntPtr hwnd,
            int nIndex
            );
        [DllImport("user32", EntryPoint = "SetLayeredWindowAttributes")]
        public static extern int SetLayeredWindowAttributes(
            IntPtr hwnd,			//目标窗口句柄
            int ColorRefKey,		//透明色
            int bAlpha,				//不透明度
            int dwFlags
            );

        [DllImport("user32")]
        public static extern bool MoveWindow(
            int hWnd,
            int X,
            int Y,
            int nWidth,
            int nHeight,
            bool bRepaint
            );

        //获得窗口类名称，返回值为字符串的字符数量
        [DllImport("user32")]
        public static extern uint RealGetWindowClass(
            int hWnd,
            StringBuilder pszType,		//缓冲区
            uint cchType);				//缓冲区长度

        //枚举屏幕上所有顶级窗口（不会枚举子窗口，除了一些有WS_CHILD的顶级窗口）
        [DllImport("user32")]
        public static extern bool EnumWindows(
            WNDENUMPROC lpEnumFunc,
            int lParam
            );

        [DllImport("user32")]
        public static extern bool EnumChildWindows(
            int hWndParent,
            WNDENUMPROC lpEnumFunc,
            int lParam
            );

        [DllImport("user32")]
        public static extern bool EnumThreadWindows(		//枚举线程窗口
            int dwThreadId,
            WNDENUMPROC lpEnumFunc,
            int lParam
            );

        [DllImport("user32")]
        public static extern int GetParent(
            int hWnd
            );

        [DllImport("user32")]
        public static extern int GetWindow(
            int hWnd,	//基础窗口
            uint uCmd	//关系
            );

        /*
         * 获取鼠标的屏幕坐标，填充到Point
         * WINUSERAPI
            BOOL
            WINAPI
            GetCursorPos(
                    __out LPPOINT lpPoint);
         */
        [DllImport("user32")]
        public static extern bool GetCursorPos(
            out POINT lpPoint
            );

        [DllImport("user32")]
        public static extern int GetDC(
            int hWnd
            );

        [DllImport("user32")]
        public static extern int GetWindowDC(
            int hWnd
            );

        [DllImport("user32")]
        public static extern int ReleaseDC(
            int hWnd,
            int hDC
            );

        [DllImport("user32")]
        public static extern int FillRect(
            int hDC,
            RECT lprc,
            int hBrush
            );

        [DllImport("user32")]
        public static extern bool InvalidateRect(
            int hwnd,
            ref RECT lpRect,
            bool bErase
            );

        //判断一个窗口是否是可见的
        [DllImport("user32")]
        public static extern bool IsWindowVisible(
            int hwnd
            );

        //绘制焦点举行
        [DllImport("user32")]
        public static extern bool DrawFocusRect(
            int hDC,
            ref RECT lprc
            );

        [DllImport("user32")]
        public static extern bool UpdateWindow(
            int hwnd
            );

        [DllImport("user32")]
        public static extern bool EnableWindow(
            int hwnd,
            bool bEnable
            );

        //设置前景窗口，强制其线程成为前台，并激活窗口
        [DllImport("user32")]
        public static extern bool SetForegroundWindow(
            int hwnd
            );

        //设置前景窗口，强制其线程成为前台，并激活窗口
        [DllImport("user32")]
        public static extern bool GetForegroundWindow(
            );

        //获取拥有焦点窗口（唯一拥有键盘输入的窗口）
        [DllImport("user32")]
        public static extern int GetFocus(
            );

        //设置焦点窗口（返回值是前一个焦点窗口）
        [DllImport("user32")]
        public static extern int SetFocus(
            int hwnd
            );

        //根据点查找窗口
        [DllImport("user32")]
        public static extern int WindowFromPoint(
            POINT Point
            );

        [DllImport("user32")]
        public static extern int ChildWindowFromPoint(
            int hWndParent,
            POINT Point
            );

        [DllImport("user32")]
        public static extern bool DestroyIcon(
            int hIcon
            );


        #endregion

        #region P/Invoke GDI32
        [DllImport("Gdi32")]
        public static extern int SetROP2(
            int hDC,
            int fnDrawMode
            );

        [DllImport("Gdi32")]
        public static extern int GetROP2(
            int hDC
            );

        [DllImport("Gdi32")]
        public static extern bool ValidateRect(
            int hWnd,
            ref RECT lpRect
            );

        [DllImport("Gdi32")]
        public static extern int CreateSolidBrush(
            int crColor
            );

        [DllImport("Gdi32")]
        public static extern int CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            int lpInitData		//这个参数实际是一个 DEVMODE 结构的指针
            );

        [DllImport("Gdi32")]
        public static extern int SelectObject(
            int hDC,
            int hGdiObj
            );

        [DllImport("Gdi32")]
        public static extern int DeleteObject(
            int hObject
            );

        [DllImport("Gdi32")]
        public static extern int CreatePen(
            int fnPenStyle,
            int nWidth,
            int crColor
            );

        [DllImport("Gdi32")]
        public static extern int CreatePenIndirect(
            ref LOGPEN lplogpen
            );

        [DllImport("Gdi32")]
        public static extern bool MoveToEx(
            int hDC,
            int X,
            int Y,
            ref POINT lpPoint
            );

        [DllImport("Gdi32")]
        public static extern bool LineTo(
            int hDC,
            int nXEnd,
            int nYEnd
            );

        //无伸展位图传送
        [DllImport("Gdi32")]
        public static extern bool BitBlt(
            int hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            int hdcSrc,
            int nXsrc,
            int nYsrc,
            int dwRop		//光栅操作码
            );

        //获取特定设备的特定信息，例如屏幕象素高度，宽度
        [DllImport("Gdi32")]
        public static extern bool GetDeviceCaps(
            int hdc,
            int nIndex
            );

        //创建一个匹配的内存DC，返回其句柄
        [DllImport("Gdi32")]
        public static extern int CreateCompatibleDC(
            int hDC		//如果此参数null，则创建于屏幕匹配的内存dc
            );

        //创建一个与某dc匹配的内存位图，返回句柄
        [DllImport("Gdi32")]
        public static extern int CreateCompatibleBitmap(
            int hDC,
            int nWidth,
            int nHeight
            );


        #endregion

        
        

        public WndInfo GetWndInfo(int hwnd)
        {
            WndInfo info = new WndInfo();
            info.hWnd = hwnd;
            //获得窗口的标题
            GetWindowText(hwnd, this.m_StringBuilder, 256);
            info.Caption = this.m_StringBuilder.ToString();

            //获得窗口的类名
            RealGetWindowClass(hwnd, this.m_StringBuilder, 256);
            info.WndClassName = this.m_StringBuilder.ToString();

            //获取窗口矩形
            GetWindowRect(hwnd, ref this.m_Rect);
            info.X = this.m_Rect.Left;
            info.Y = this.m_Rect.Top;
            info.Width = this.m_Rect.Right - this.m_Rect.Left;
            info.Height = this.m_Rect.Bottom - this.m_Rect.Top;
            info.IsVisible = IsWindowVisible(hwnd);
            return info;
        }

        
       

        /// <summary>
        /// 根据Process返回一个进程信息结构
        /// </summary>
        /// <param name="pro"></param>
        /// <returns></returns>
        public static ProcessInfo GetProcessInfo(Process pro)
        {
            ProcessInfo info = new ProcessInfo();
            info.ID = pro.Id;
            info.BasePriority = pro.BasePriority;
            info.IsResponding = pro.Responding;
            info.ProcessName = pro.ProcessName;
            info.MainWindowHandle = (int)pro.MainWindowHandle;
            info.MainWindowTitle = pro.MainWindowTitle;
            //注意IDLE进程会有例外，不要试图访问特殊进程的信息！
            string pname = info.ProcessName.ToLower();
            //if (pname == "idle" || pname == "svchost" || pname == "msdtssrvr" || pname == "aspnet_state" || pname=="csrss" || pname=="scardsvr")
            //    info.StartTime = "不可获取";
            //else
            //    info.StartTime = pro.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                info.StartTime = pro.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                info.StartTime = "未知：" + ex.Message;
            }
            info.VirtualMemorySize = pro.VirtualMemorySize;
            return info;
        }

        /// <summary>
        /// 获取线程信息
        /// </summary>
        /// <param name="thread"></param>
        /// <returns></returns>
        public static ThreadInfo GetThreadInfo(ProcessThread thread)
        {
            ThreadInfo info = new ThreadInfo();
            info.ID = thread.Id;
            info.BasePriority = thread.BasePriority;
            info.StartTime = thread.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            return info;
        }

        //高亮显示某个窗口，在窗口外部发色绘制边框，绘制偶数次会擦除反色边框
        public void HighLightWindow(int hwnd)
        {
            GetWindowRect(hwnd, ref this.m_Rect);
            //获得窗口dc
            int width = this.m_Rect.Right - this.m_Rect.Left;
            int height = this.m_Rect.Bottom - this.m_Rect.Top;
            int hdc = GetWindowDC(hwnd);
            //创建黑色的，4象素宽度的画笔
            int hNewPen = CreatePen(PS_SOLID, 4, 0);
            int hOldPen = SelectObject(hdc, hNewPen);
            //设置为反色模式
            SetROP2(hdc, R2_NOT);
            POINT lpPoint = new POINT();

            //注意LineTo函数不会绘制最后一个点
            MoveToEx(hdc, 0, 2, ref lpPoint);
            LineTo(hdc, width - 2, 2);
            LineTo(hdc, width - 2, height - 2);
            LineTo(hdc, 2, height - 2);
            LineTo(hdc, 2, 2);

            //释放对象
            SelectObject(hdc, hOldPen);
            DeleteObject(hNewPen);
            //释放dc
            ReleaseDC(hwnd, hdc);
        }

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindWindowDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbWndClass = new System.Windows.Forms.ComboBox();
            this.tbWindowName = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btExpand = new System.Windows.Forms.Button();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbWndHandle = new System.Windows.Forms.TextBox();
            this.cbMapUpperLower = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbMapFullword = new System.Windows.Forms.CheckBox();
            this.pbFindWnd = new System.Windows.Forms.PictureBox();
            this.cbHideMainFrm = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.LableMousePos = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFindWnd)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "窗口类名：";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "窗口标题：";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbWndClass
            // 
            this.cbWndClass.Items.AddRange(new object[] {
            "#32770",
            "Notepad",
            "Progman",
            "Shell_TrayWnd",
            "MSPaintApp",
            "VBFloatingPalette"});
            this.cbWndClass.Location = new System.Drawing.Point(96, 24);
            this.cbWndClass.Name = "cbWndClass";
            this.cbWndClass.Size = new System.Drawing.Size(208, 20);
            this.cbWndClass.TabIndex = 2;
            // 
            // tbWindowName
            // 
            this.tbWindowName.Location = new System.Drawing.Point(96, 56);
            this.tbWindowName.Name = "tbWindowName";
            this.tbWindowName.Size = new System.Drawing.Size(208, 21);
            this.tbWindowName.TabIndex = 3;
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(168, 200);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "确定";
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(248, 200);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "取消";
            // 
            // btExpand
            // 
            this.btExpand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btExpand.Location = new System.Drawing.Point(8, 200);
            this.btExpand.Name = "btExpand";
            this.btExpand.Size = new System.Drawing.Size(56, 23);
            this.btExpand.TabIndex = 6;
            this.btExpand.Text = "↓↓";
            this.btExpand.Click += new System.EventHandler(this.btExpand_Click);
            // 
            // tbDescription
            // 
            this.tbDescription.BackColor = System.Drawing.SystemColors.Control;
            this.tbDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbDescription.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.tbDescription.Location = new System.Drawing.Point(8, 248);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ReadOnly = true;
            this.tbDescription.Size = new System.Drawing.Size(312, 168);
            this.tbDescription.TabIndex = 7;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.tbWndHandle);
            this.groupBox1.Controls.Add(this.cbMapUpperLower);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbMapFullword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbWndClass);
            this.groupBox1.Controls.Add(this.tbWindowName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(8, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(312, 152);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // label6
            // 
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(260, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 23);
            this.label6.TabIndex = 11;
            this.label6.Text = "16进制";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(16, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 23);
            this.label4.TabIndex = 9;
            this.label4.Text = "窗口句柄：";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbWndHandle
            // 
            this.tbWndHandle.Location = new System.Drawing.Point(96, 88);
            this.tbWndHandle.Name = "tbWndHandle";
            this.tbWndHandle.Size = new System.Drawing.Size(158, 21);
            this.tbWndHandle.TabIndex = 10;
            this.tbWndHandle.Text = "0";
            // 
            // cbMapUpperLower
            // 
            this.cbMapUpperLower.Location = new System.Drawing.Point(200, 120);
            this.cbMapUpperLower.Name = "cbMapUpperLower";
            this.cbMapUpperLower.Size = new System.Drawing.Size(88, 24);
            this.cbMapUpperLower.TabIndex = 6;
            this.cbMapUpperLower.Text = "大小写匹配";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 23);
            this.label3.TabIndex = 5;
            this.label3.Text = "标题匹配：";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbMapFullword
            // 
            this.cbMapFullword.Location = new System.Drawing.Point(96, 120);
            this.cbMapFullword.Name = "cbMapFullword";
            this.cbMapFullword.Size = new System.Drawing.Size(80, 24);
            this.cbMapFullword.TabIndex = 4;
            this.cbMapFullword.Text = "全字匹配";
            // 
            // pbFindWnd
            // 
            this.pbFindWnd.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.pbFindWnd.Image = ((System.Drawing.Image)(resources.GetObject("pbFindWnd.Image")));
            this.pbFindWnd.Location = new System.Drawing.Point(136, 160);
            this.pbFindWnd.Name = "pbFindWnd";
            this.pbFindWnd.Size = new System.Drawing.Size(32, 29);
            this.pbFindWnd.TabIndex = 7;
            this.pbFindWnd.TabStop = false;
            this.pbFindWnd.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbFindWnd_MouseDown);
            this.pbFindWnd.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbFindWnd_MouseMove);
            this.pbFindWnd.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbFindWnd_MouseUp);
            // 
            // cbHideMainFrm
            // 
            this.cbHideMainFrm.Location = new System.Drawing.Point(208, 160);
            this.cbHideMainFrm.Name = "cbHideMainFrm";
            this.cbHideMainFrm.Size = new System.Drawing.Size(88, 24);
            this.cbHideMainFrm.TabIndex = 8;
            this.cbHideMainFrm.Text = "隐藏主窗口";
            this.cbHideMainFrm.CheckedChanged += new System.EventHandler(this.cbHideMainFrm_CheckedChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(24, 160);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 23);
            this.label5.TabIndex = 11;
            this.label5.Text = "查找窗口工具：";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LableMousePos
            // 
            this.LableMousePos.Location = new System.Drawing.Point(72, 200);
            this.LableMousePos.Name = "LableMousePos";
            this.LableMousePos.Size = new System.Drawing.Size(88, 23);
            this.LableMousePos.TabIndex = 12;
            this.LableMousePos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FindWindowDlg
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(330, 231);
            this.Controls.Add(this.LableMousePos);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.btExpand);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.cbHideMainFrm);
            this.Controls.Add(this.pbFindWnd);
            this.Controls.Add(this.label5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindWindowDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "查找窗口";
            this.Load += new System.EventHandler(this.FindWindowDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFindWnd)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


		//--------------------------------
		//		外部接口
		//--------------------------------
		//窗口类名称
		public string WNDCLASS
		{
			get{return this.cbWndClass.Text.Length>0? this.cbWndClass.Text:null;}
		}
		//窗口名称（标题或者控件文本）
		public string WNDNAME
		{
			get{return this.tbWindowName.Text.Length>0? this.tbWindowName.Text:null;}
		}
		//标题是否全字匹配
		public bool MAP_FULLWORD
		{
			get{return this.cbMapFullword.Checked;}
		}
		//标题是否大小写匹配
		public bool MAP_UPPERLOWER
		{
			get{return this.cbMapUpperLower.Checked;}
		}
		//返回匹配标志
		public int MAP_FLAG
		{
			get
			{
				int fullword=this.cbMapFullword.Checked? MF_FULLWORD:MF_NONE;
				int upperlower=this.cbMapUpperLower.Checked? MF_UPPERLOWER:MF_NONE;
				return (fullword | upperlower);
			}
		}
		
		

		//获取用查找工具找到的窗口
		public int HWND
		{
			get{return this.m_HWND;}
		}
		//获取窗口信息
		public WndInfo WNDINFO
		{
			get{return this.m_WndInfo;}
		}

		//-----------------------------------
		//			内部辅助函数
		//-----------------------------------
		private void LoadCursor()
		{
			System.Reflection.Assembly asm=System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream stream=asm.GetManifestResourceStream(asm.GetName().Name+".FINDWND.CUR");
			this.m_Cursor=new Cursor(stream);
		}

		//-----------------------------------
		//			控件事件的处理
		//-----------------------------------

		//扩展按钮的处理函数
		private void btExpand_Click(object sender, System.EventArgs e)
		{
			if(this.btExpand.Text=="↓↓")
			{
				//注意控件坐标没有包括标题栏的高度(大约20象素高度)，而Height中需要包含非客户区尺寸
				this.Height=this.tbDescription.Bottom+35;
				this.btExpand.Text="↑↑";
			}
			else
			{
				//注意控件坐标没有包括标题栏的高度(大约20象素高度)
				this.Height=this.tbDescription.Top+10;
				this.btExpand.Text="↓↓";
			}
		}

		private void pbFindWnd_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//把图片box中间的那个图标暂时擦除！！！！
			Graphics g=this.pbFindWnd.CreateGraphics();
			//擦除光标的形状
			g.FillRectangle(Brushes.White,RC_CURSORPOS);
			//设置光标
			Cursor.Current=this.m_Cursor;
			//捕获鼠标
			this.pbFindWnd.Capture=true;
			g.Dispose();
			//正在查找窗口，获取本窗口的屏幕坐标！(鼠标位于本窗口内时不要查找窗口！)
			GetWindowRect((int)this.Handle, ref this.m_Bounds);
			//将两个交替的窗口句柄都置零
			this.m_PreHWND = 0;
			this.m_HWND = 0;
			this.b_FindingWnd=true;
		}

		//查找工具鼠标抬起
		private void pbFindWnd_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//不再查找窗口
			this.b_FindingWnd=false;
			//使PicureBox无效(重新显示出光标的形状)
            this.pbFindWnd.Invalidate(RC_CURSORPOS);
			//恢复光标
			Cursor.Current=Cursors.Default;
			this.pbFindWnd.Capture=false;
			//使最近被捕捉到的窗口进行重绘（以擦除我们绘制的反色边框）
			if (this.m_HWND != 0)
			{
				HighLightWindow(this.m_HWND);
			}
		}

		//查找工具鼠标移动
		private void pbFindWnd_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
            //如果不是处于查找窗口状态，则返回
			if(!this.b_FindingWnd)
				return;
			//this.m_ScreenPoint=this.pbFindWnd.PointToScreen(new Point(e.X,e.Y));
            //获取光标指针的屏幕坐标
            GetCursorPos(out this.m_POINT);
			//如果鼠标的位置在本对话框内部，则不要试图查找窗口！
			if(PtInRect(ref this.m_Bounds,this.m_POINT))
				return;            
			//this.m_POINT.X=this.m_ScreenPoint.X;
			//this.m_POINT.Y=this.m_ScreenPoint.Y;
			this.LableMousePos.Text=string.Format("X:{0} Y:{1}",
				this.m_POINT.X,
				this.m_POINT.Y);
			this.m_HWND=WindowFromPoint(this.m_POINT);
			if(this.m_HWND!=this.m_PreHWND)
			{
				if(this.m_HWND==0)
				{
					//未找到窗口，清空显示内容
					this.cbWndClass.Text=string.Empty;
					this.tbWindowName.Clear();
					this.tbWndHandle.Clear();
				}
				else
				{
					//显示该窗口的信息
					this.m_WndInfo=this.GetWndInfo(this.m_HWND);
					this.cbWndClass.Text=this.m_WndInfo.WndClassName;
					this.tbWindowName.Text=this.m_WndInfo.Caption;
					this.tbWndHandle.Text=Convert.ToString(this.m_WndInfo.hWnd,16).PadLeft(8,'0');
					//高亮新找到的窗口
					this.HighLightWindow(this.m_HWND);
				}
				//再次高亮前一次的窗口（擦除效果）
				if(this.m_PreHWND>0)
					this.HighLightWindow(this.m_PreHWND);
				//更新前一窗口到当前窗口
				this.m_PreHWND=this.m_HWND;
			}
		}

		//隐藏主窗口的处理函数

        private bool hideWindows = false;
		private void cbHideMainFrm_CheckedChanged(object sender, System.EventArgs e)
		{
            if (Windows != null)
            {
                if (hideWindows)
                {
                    foreach (Window w in Windows)
                    {
                        w.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    foreach (Window w in Windows)
                    {
                        w.Visibility = Visibility.Hidden;
                    }
                }
                hideWindows = !hideWindows;
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btOk_Click(object sender, EventArgs e)
		{
			string str=this.tbWndHandle.Text.Trim();
			try
			{
				this.m_HWND=Convert.ToInt32(str, 16);
				if (!this.Visible)
					this.Visible = true;
				this.DialogResult=DialogResult.OK;
			}
			catch(Exception ex)
			{
				System.Windows.MessageBox.Show("请输入合法的16进制数！");
				this.tbWndHandle.Focus();
				this.tbWndHandle.SelectAll();
				return;
			}
		}

        private void FindWindowDlg_Load(object sender, EventArgs e)
        {

        }
	}
}
