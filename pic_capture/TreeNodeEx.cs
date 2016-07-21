using System;
using System.Windows.Forms;

namespace DesktopWndView
{
	/// <summary>
	/// 窗口信息
	/// </summary>
	public struct WndInfo
	{
		public int hWnd;			//窗口句柄
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public string Caption;		//窗口标题（控件文本）
		public string WndClassName;	//窗口类名称
		public bool IsVisible;		//0-不可见窗口，1-可见窗口
	}

	/// <summary>
	/// 进程信息
	/// </summary>
	public struct ProcessInfo
	{
		public int ID;					//进程ID
		public int BasePriority;		//进程优先级
        public int MainWindowHandle;	//主窗口句柄
		public int VirtualMemorySize;	//占用虚拟内存大小
		public string MainWindowTitle;	//主窗口标题
		public string ProcessName;		//进程名称
		public string StartTime;		//启动时间
		public bool IsResponding;		//是否正在响应
	}

	/// <summary>
	/// 线程信息
	/// </summary>
	public struct ThreadInfo
	{
		public int ID;
		public int BasePriority;
		public string StartTime;
	}

	/// <summary>
	/// 节点类型
	/// </summary>
	public enum NodeType : int
	{
		Window=1,					//1为可见窗口，0为不可见窗口
		Process=2,
		Thread=3
	}
	/// <summary>
	/// 窗口树上的一个树节点，代表了一个窗口，一个进程/线程
	/// </summary>
	public class TreeNodeEx : TreeNode
	{
        public bool b_ChildAdded;
		private NodeType m_Type;

		//外部可获取节点类型
		public NodeType NODETYPE
		{
			get{return this.m_Type;}
		}
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="hasChild"></param>		
		public TreeNodeEx(NodeType nodetype,object info)
		{
			this.b_ChildAdded=false;
			this.m_Type=nodetype;
			this.Tag=info;
			this.ImageIndex=(int)nodetype;
			this.SelectedImageIndex=(int)nodetype;
			switch(this.m_Type)
			{
				case NodeType.Window:
					WndInfo wndinfo=(WndInfo)info;
					this.Text=string.Format("{0} \"{1}\"",wndinfo.WndClassName,wndinfo.Caption);
					if(!wndinfo.IsVisible)
					{
						this.ImageIndex--;
						this.SelectedImageIndex--;
					}
					break;

				case NodeType.Process:
					//转化为"0x********"的16进制表示形式
					ProcessInfo proinfo=(ProcessInfo)info;
					this.Text=string.Format("PID:0x{0} {1}",
						Convert.ToString(proinfo.ID,16).ToUpper().PadLeft(8,'0'),
						proinfo.ProcessName);
					break;
				case NodeType.Thread:
					//转化为"0x********"的16进制表示形式
					ThreadInfo thdinfo=(ThreadInfo)info;
					this.Text="TID:0x"+Convert.ToString(thdinfo.ID,16).ToUpper().PadLeft(8,'0');
					break;
			}
			this.Nodes.Add("TempNode");
		}

		public TreeNodeEx(NodeType nodetype,object info,bool hasChild)
		{
			this.b_ChildAdded=false;
			this.m_Type=nodetype;
			//注意，两个属性都要进行设置！
			this.ImageIndex=(int)nodetype;
			this.SelectedImageIndex=(int)nodetype;
			this.Tag=info;
			switch(this.m_Type)
			{
				case NodeType.Window:
					WndInfo wndinfo=(WndInfo)info;
					this.Text=string.Format("{0} \"{1}\"",wndinfo.WndClassName,wndinfo.Caption);
					if(!wndinfo.IsVisible)
					{
						this.ImageIndex--;
						this.SelectedImageIndex--;
					}
					break;
				case NodeType.Process:
					//转化为"0x********"的16进制表示形式
					ProcessInfo proinfo=(ProcessInfo)info;
					this.Text=string.Format("PID:0x{0} {1}",
						Convert.ToString(proinfo.ID,16).ToUpper().PadLeft(8,'0'),
						proinfo.ProcessName);
					break;
				case NodeType.Thread:
					//转化为"0x********"的16进制表示形式
					ThreadInfo thdinfo=(ThreadInfo)info;
					this.Text="TID:0x"+Convert.ToString(thdinfo.ID,16).ToUpper().PadLeft(8,'0');
					break;
			}
			if(hasChild)
				this.Nodes.Add("TempNode");
		}

	}
}
