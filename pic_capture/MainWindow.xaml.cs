using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

namespace pic_capture
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cp = new CaptureWindow();
        }

        CaptureWindow cp;
        HookMK hook;
        bool Hooked = false;

        private void button_shot_Click(object sender, RoutedEventArgs e)
        {
            Image_shot.Source = new ConfigShotManager().startShot();
            if (Image_shot.Source == null)
                MessageBox.Show("找不到窗口！");
        }

        private void button_hook_shot_Click(object sender, RoutedEventArgs e)
        {
            if (Hooked)
            {
                hook.Unhook();
                button_hook_shot.Content = "挂钩截图";
            } 
            else
            {
                hook = new HookMK(Image_shot);
                button_hook_shot.Content = "卸载挂钩";
            }
            Hooked = !Hooked;
        }

        private void button_setting_Click(object sender, RoutedEventArgs e)
        {
            new Window_Setting(this).Show();
        }

        
    }
}
