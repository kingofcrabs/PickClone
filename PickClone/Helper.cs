using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
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

    public class GlobalVars
    {
        static GlobalVars instance = null;
        public Settings Settings { get; set; }
        public static GlobalVars Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalVars();
                return instance;
            }
        }

        private GlobalVars()
        {
            Settings = Settings.Load();
        }

    }

    public class ConfigValues
    {
        static bool useTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
        static string plateType = ConfigurationManager.AppSettings["plateType"];
        static public bool UseTestImage
        {
            get
            {
                return useTestImage;
            }
        }


        public static string PlateType
        {
            get
            {
                return plateType;
            }
        }
    }

    public class Mics
    {
        static public Bitmap LoadLatestImage(bool bUseTestImage, string testImagePath)
        {

            Bitmap img;
            string s = FolderHelper.GetLatestImagePath();
            if (bUseTestImage)
                s = testImagePath;

            if (!File.Exists(s))
                throw new Exception("未能采集到图片！");

            using (var bmpTemp = new Bitmap(s))
            {
                img = new Bitmap(bmpTemp);
            }
            return img;
        }
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
            string sConfigFolder = GetExeParentFolder() + "Config\\";
            CreateIfNotExist(sConfigFolder);
            return sConfigFolder;
        }

        static public string GetDataFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Data\\";
            CreateIfNotExist(sDataFolder);
            return sDataFolder;
        }

        //static public string GetLatestImage()
        //{
        //    string dir = ConfigurationManager.AppSettings["imageFolder"];
        //    var files = Directory.EnumerateFiles(dir, "*.jpg");
        //    List<FileInfo> fileInfos = new List<FileInfo>();
        //    foreach (var file in files)
        //    {
        //        fileInfos.Add(new FileInfo(file));
        //    }
        //    var latest = fileInfos.OrderBy(x => x.CreationTime).Last();
        //    return latest.FullName;
        //}

        private static void CreateIfNotExist(string sFolder)
        {
            if (!Directory.Exists(sFolder))
                Directory.CreateDirectory(sFolder);
        }

        static public string GetLatestImagePath()
        {

            return GetDataFolder() + "latest.jpg";
        }

        internal static string GetTestImagePath()
        {
            return GetDataFolder() + "test.jpg";
        }

        internal static string GetIconFolder()
        {
            string sDataFolder = GetExeParentFolder() + "Icons\\";
            return sDataFolder;
            
        }
    }
}
