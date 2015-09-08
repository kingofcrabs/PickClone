using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace PickClone
{
    class Calibration
    {
        FourPoints pixelsRef;
        FourPoints physicalRef;
        public Calibration()
        {
            physicalRef = ReadCalibFile(ConfigValues.PlateType);
        }

        public void SetRefPixels(RefPositions refPos)
        {
            pixelsRef = new FourPoints(refPos.top, refPos.bottom, refPos.left, refPos.right);
        }

        private FourPoints ReadCalibFile(string calibType)
        {
            string sFile = FolderHelper.GetConfigFolder() + calibType + "_calib.xml";
            if(!File.Exists(sFile))
            {
                SerializeHelper.Save(new FourPoints(0, 1000, 0, 1000), sFile);
            }
            string allText = File.ReadAllText(sFile);
            return SerializeHelper.Deserialize<FourPoints>(allText);
        }

        private int GetValue(Dir dir, string[] strs)
        {
             int index = (int)dir;
             return int.Parse(strs[index]);
        }

        public int ConvertX(int xPixel)
        {
            int dis2Left = xPixel - pixelsRef.left;
            int pixelWidth = pixelsRef.right - pixelsRef.left;
            int physicalWidth = physicalRef.right - physicalRef.left;
            return physicalRef.left + physicalWidth * dis2Left / pixelWidth;
        }

        public int ConvertY(int yPixel)
        {
            int dis2Top = yPixel - pixelsRef.top;
            int pixelHeight = pixelsRef.bottom - pixelsRef.top;
            int physicalHeight = physicalRef.bottom - physicalRef.top;
            return physicalRef.top + physicalHeight * dis2Top / pixelHeight;
        }
    }

    enum Dir
    {
        top = 0,        
        left = 1,
        bottom = 2,
        right = 3
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
}
