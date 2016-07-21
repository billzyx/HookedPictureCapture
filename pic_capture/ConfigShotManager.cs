using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace pic_capture
{
    class ConfigShotManager
    {
        public BitmapSource startShot()
        {
            CaptureWindow cp1 = new CaptureWindow();
            BitmapSource bits = null;
            if (AppConfig.GetAppConfig("lpszParentClass") == null || AppConfig.GetAppConfig("lpszParentClass").Equals(""))
            {
                if (AppConfig.GetAppConfig("Parent_hWnd") == null || AppConfig.GetAppConfig("Parent_hWnd").Equals(""))
                    return null;
                try
                {
                    if (AppConfig.GetAppConfig("Image_hWnd") == null || AppConfig.GetAppConfig("Image_hWnd").Equals(""))
                    { bits = cp1.shot(FormatConifgWnd(AppConfig.GetAppConfig("Parent_hWnd"))); }
                    else
                    { bits = cp1.shot(FormatConifgWnd(AppConfig.GetAppConfig("Image_hWnd"))); }
                }
                catch(Exception e)
                { return null; }
                
            }
            else
            {
                if (AppConfig.GetAppConfig("lpszClass_Image")==null||AppConfig.GetAppConfig("lpszClass_Image").Equals(""))
                { bits = cp1.shot(AppConfig.GetAppConfig("lpszParentClass"));  }
                else
                { bits = cp1.shot(AppConfig.GetAppConfig("lpszParentClass"), AppConfig.GetAppConfig("lpszClass_Image")); }
            }
            if(bits!=null)
                if (AppConfig.GetAppConfig("SavePic").Equals("True"))
                    FileManager.savePNG(bits);
            return bits;
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
            catch(Exception e)
            { return new IntPtr(0); }
        }
    }
}
