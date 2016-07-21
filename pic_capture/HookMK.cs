using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLevelHooks.Keyboard;
using LowLevelHooks.Mouse;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

namespace pic_capture
{
    class HookMK
    {
        private readonly KeyboardHook kHook;
        private readonly MouseHook mHook;
        private Image image = null;
        

        public HookMK()
        {
            kHook = new KeyboardHook();
            mHook = new MouseHook();
            kHook.KeyEvent += KHookKeyEvent;
            mHook.MouseEvent += MHookMouseEvent;
            mHook.Hook();
            //kHook.Hook();
        }

        public HookMK(Image image)
        {
            kHook = new KeyboardHook();
            mHook = new MouseHook();
            kHook.KeyEvent += KHookKeyEvent;
            mHook.MouseEvent += MHookMouseEvent;
            mHook.Hook();
            //kHook.Hook();
            this.image = image;
            
        }

        ~HookMK()
        {
            kHook.Dispose();
            mHook.Dispose();
        }

        private void MHookMouseEvent(object sender, MouseHookEventArgs e)
        {
            WriteMouse(e);
        }

        private void WriteMouse(MouseHookEventArgs e)
        {
            if (e.MouseEventName.ToString().Equals("LeftButtonDown"))
            {
                CaptureWindow cp = new CaptureWindow();
                if (cp.isInWindow(e.Position, AppConfig.GetAppConfig("lpszParentClass")))
                {
                    if (AppConfig.GetAppConfig("lpszClass_Button") == null || AppConfig.GetAppConfig("lpszClass_Button").Equals(""))
                    {
                        if ((AppConfig.GetAppConfig("lpszClass_Thumbnail") == null) || AppConfig.GetAppConfig("lpszClass_Thumbnail").Equals(""))
                        { startShot(); return; }
                        else
                        {
                            if (cp.isInWindow(e.Position, AppConfig.GetAppConfig("lpszParentClass"), AppConfig.GetAppConfig("lpszClass_Thumbnail")))
                            { startShot(); return; }
                        }
                    }
                    else
                    {
                        if (cp.isInWindow(e.Position, AppConfig.GetAppConfig("lpszParentClass"), AppConfig.GetAppConfig("lpszClass_Button")))
                        { startShot(); return; }
                        else
                        {
                            if (!((AppConfig.GetAppConfig("lpszClass_Thumbnail") == null) || AppConfig.GetAppConfig("lpszClass_Thumbnail").Equals("")))
                                if (cp.isInWindow(e.Position, AppConfig.GetAppConfig("lpszParentClass"), AppConfig.GetAppConfig("lpszClass_Thumbnail")))
                                { startShot(); return; }
                        } 
                    }   
                }
                else
                {
                    if(cp.isInWindow(e.Position, FormatConifgWnd(AppConfig.GetAppConfig("Parent_hWnd"))))
                    {
                        { startShot(); return; }
                    }
                }
            }
        }

        private void startShot()
        {
            new Thread(delegate()
                    {
                        
                        Thread.Sleep(2000);
                        ConfigShotManager csm = new ConfigShotManager();
                        BitmapSource bits = csm.startShot();
                        if(bits == null)
                        {
                            if (image != null)
                            {
                                image.Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal,

                                        new Action(() =>
                                        {
                                            image.Source = new BitmapImage(new Uri(@"image/error.png", UriKind.Relative));
                                        }));
                            }
                            return; 
                        }
                        if (image != null)
                        {
                            bits.Freeze();
                            image.Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal,

                                    new Action(() =>
                                    {
                                        image.Source = bits;
                                    }));    
                        }
                    }).Start();
        }

        private void KHookKeyEvent(object sender, KeyboardHookEventArgs e)
        {
            //WriteKey(e);
        }

        public void Unhook()
        {
            mHook.Unhook();
        }

        private IntPtr FormatConifgWnd(string configStr)
        {
            try
            {
                if (configStr.Substring(0, 2) == "00")
                    return new IntPtr(Int32.Parse(configStr, System.Globalization.NumberStyles.HexNumber));
                else
                    return new IntPtr(Int32.Parse(configStr));
            }
            catch (Exception e)
            { return new IntPtr(0); }
        }
    }
}
