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
        public FourPoints physicalRef;
        public SelectionMethod SelectionMethod{get;set;}
        public int CloneCnt { get; set; }
        public Settings()
        {
            CloneCnt = 50;
            SelectionMethod = SelectionMethod.biggest;
            physicalRef = new FourPoints();
        }
        public void Save()
        {
            string plateType = ConfigValues.PlateType;
            string sFile = FolderHelper.GetConfigFolder() + plateType + ".xml";
            SerializeHelper.Save(this, sFile);
        }
        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Settings();
                return _instance;
            }
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



        public FourPoints PhysicalRef
        { 
            get
            {
                return physicalRef;
            }
        }
    }

    [Serializable]
    public class FourPoints
    {
        public int top;
        public int bottom;
        public int left;
        public int right;

        public FourPoints()
        {
            top = 0;
            bottom = 1000;
            left = 0;
            right = 1000;
        }
        public FourPoints(int top, int bottom, int left, int right)
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
        }
    }


    public enum SelectionMethod
    {
        biggest = 0,
        random =  1
    }
}
