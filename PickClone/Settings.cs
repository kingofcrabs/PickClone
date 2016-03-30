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
    public class Settings : BindableBase
    {
        public FourPoints physicalRef;
        public ulong ExposureTime{get;set;}
        public SelectionMethod SelectionMethod{get;set;}
        int cloneCnt;
        public int CloneCnt
        {
            get { return cloneCnt; }
            set
            {
                SetProperty(ref cloneCnt, value);
            }
        }
        int maxArea;
        public int MaxArea
        {
            get
            {
                return maxArea;
            }
            set
            {
                SetProperty(ref maxArea, value);
            }
        }
        int minArea;
        public int MinArea
        {
            get
            {
                return minArea;
            }
            set
            {
                SetProperty(ref minArea, value);
            }
        }
        public Settings()
        {
            CloneCnt = 50;
            MaxArea = 200;
            MinArea = 10;
            ExposureTime = 140;
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
       

        public void Load()
        {
            string plateType = ConfigValues.PlateType;
            string sFile = FolderHelper.GetConfigFolder() + plateType + ".xml";
            if (!File.Exists(sFile))
            {
                _instance = new Settings();
                return;
            }
            string sContent = File.ReadAllText(sFile);
            _instance = SerializeHelper.Deserialize<Settings>(sContent);
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
