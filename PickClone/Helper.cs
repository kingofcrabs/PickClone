using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;


namespace PickClone
{
    class ImageHelper
    {
        public static BitmapImage BitmapFromFile(string source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(source);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public static string ImagePath { get; set; }
    }


    partial class SerialHelper
    {
        
    }

    public class FolderHelper
    {
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        static public string GetConfigFolder()
        {
            return GetExeParentFolder() + "Config\\";
        }

        static public string GetDataFolder()
        {
            return GetExeParentFolder() + "Data\\";
        }

        static public string GetLatestImage()
        {
            return GetDataFolder() + "latest.jpg";
        }
    }
}
