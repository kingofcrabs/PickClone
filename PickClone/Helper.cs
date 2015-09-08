using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;


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
       
    }

    public class SerializeHelper
    {
        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public static void Save<T>(T t, string sFile)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream stream = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, t);
            stream.Close();
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

        internal static string GetOutputFolder()
        {
            string sExeParent = GetExeParentFolder();
            string sOutputFolder = sExeParent + "Output\\";
            CreateIfNotExist(sOutputFolder);
            return sOutputFolder;
        }

        static public string GetLatestImagePath()
        {
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            string s = GetLatestImagePathInner();
            if (bUseTestImage)
                s = GetTestImagePath();

            if (!File.Exists(s))
                throw new Exception("未能采集到图片！");
            return s;
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

        private static string GetLatestImagePathInner()
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
