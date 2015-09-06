using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PickClone
{
    [Serializable]
    public class Settings
    {
        
        public SelectionMethod SelectionMethod{get;set;}
        public int CloneCnt { get; set; }
        public Settings()
        {
            CloneCnt = 50;
            SelectionMethod = SelectionMethod.biggest;
        }
        public void Save()
        {
            string plateType = ConfigValues.PlateType;
            string sFile = FolderHelper.GetConfigFolder() + plateType + ".xml";
            Save(this, sFile);
        }

        private static void Save<T>(T settings, string sFile)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream stream = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, settings);
            stream.Close();
        }

        public static Settings Load()
        {
            string plateType = ConfigValues.PlateType;
            string sFile = FolderHelper.GetConfigFolder() + plateType + ".xml";
            if (!File.Exists(sFile))
                return null;
            string sContent = File.ReadAllText(sFile);
            return Deserialize<Settings>(sContent);
        }

        private static T Deserialize<T>(string xml)
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
    }

    public enum SelectionMethod
    {
        biggest = 0,
        random =  1
    }
}
