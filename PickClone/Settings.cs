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
            SerializeHelper.Save(this, sFile);
        }

       

        public static Settings Load()
        {
            string plateType = ConfigValues.PlateType;
            string sFile = FolderHelper.GetConfigFolder() + plateType + ".xml";
            if (!File.Exists(sFile))
                return new Settings();
            string sContent = File.ReadAllText(sFile);
            return SerializeHelper.Deserialize<Settings>(sContent);
        }

        
    }

    public enum SelectionMethod
    {
        biggest = 0,
        random =  1
    }
}
