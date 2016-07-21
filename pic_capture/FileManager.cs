using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace pic_capture
{
    class FileManager
    {
        public static void savePNG(BitmapSource bsrc)
        {





            String s = DateTime.Now.ToString("yyyyMMdd"); 


                PngBitmapEncoder pngE = new PngBitmapEncoder();

                pngE.Frames.Add(BitmapFrame.Create(bsrc));

                using (Stream stream = File.Create(System.Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyyMMdd") + DateTime.Now.ToString("hhmmss")   + ".png"))
                {
                    pngE.Save(stream);
                }

            
        }
    }
}
