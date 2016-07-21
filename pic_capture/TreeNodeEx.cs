using System;
using System.Windows.Forms;

namespace DesktopWndView
{
	/// <summary>
	/// ������Ϣ
	/// </summary>
	public struct WndInfo
	{
		public int hWnd;			//���ھ��
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public string Caption;		//���ڱ��⣨�ؼ��ı���
		public string WndClassName;	//����������
		public bool IsVisible;		//0-���ɼ����ڣ�1-�ɼ�����
	}

	/// <summary>
	/// ������Ϣ
	/// </summary>
	public struct ProcessInfo
	{
		public int ID;					//����ID
		public int BasePriority;		//�������ȼ�
        public int MainWindowHandle;	//�����ھ��
		public int VirtualMemorySize;	//ռ�������ڴ��С
		public string MainWindowTitle;	//�����ڱ���
		public string ProcessName;		//��������
		public string StartTime;		//����ʱ��
		public bool IsResponding;		//�Ƿ�������Ӧ
	}

	/// <summary>
	/// �߳���Ϣ
	/// </summary>
	public struct ThreadInfo
	{
		public int ID;
		public int BasePriority;
		public string StartTime;
	}

	/// <summary>
	/// �ڵ�����
	/// </summary>
	public enum NodeType : int
	{
		Window=1,					//1Ϊ�ɼ����ڣ�0Ϊ���ɼ�����
		Process=2,
		Thread=3
	}
	/// <summary>
	/// �������ϵ�һ�����ڵ㣬������һ�����ڣ�һ������/�߳�
	/// </summary>
	public class TreeNodeEx : TreeNode
	{
        public bool b_ChildAdded;
		private NodeType m_Type;

		//�ⲿ�ɻ�ȡ�ڵ�����
		public NodeType NODETYPE
		{
			get{return this.m_Type;}
		}
		/// <summary>
		/// ���캯��
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
					//ת��Ϊ"0x********"��16���Ʊ�ʾ��ʽ
					ProcessInfo proinfo=(ProcessInfo)info;
					this.Text=string.Format("PID:0x{0} {1}",
						Convert.ToString(proinfo.ID,16).ToUpper().PadLeft(8,'0'),
						proinfo.ProcessName);
					break;
				case NodeType.Thread:
					//ת��Ϊ"0x********"��16���Ʊ�ʾ��ʽ
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
			//ע�⣬�������Զ�Ҫ�������ã�
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
					//ת��Ϊ"0x********"��16���Ʊ�ʾ��ʽ
					ProcessInfo proinfo=(ProcessInfo)info;
					this.Text=string.Format("PID:0x{0} {1}",
						Convert.ToString(proinfo.ID,16).ToUpper().PadLeft(8,'0'),
						proinfo.ProcessName);
					break;
				case NodeType.Thread:
					//ת��Ϊ"0x********"��16���Ʊ�ʾ��ʽ
					ThreadInfo thdinfo=(ThreadInfo)info;
					this.Text="TID:0x"+Convert.ToString(thdinfo.ID,16).ToUpper().PadLeft(8,'0');
					break;
			}
			if(hasChild)
				this.Nodes.Add("TempNode");
		}

	}
}
