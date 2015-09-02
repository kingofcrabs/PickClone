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
            physicalRef = ReadCalibFile(ConfigurationManager.AppSettings["plateType"]);
        }

        public void SetRefPixels(RefPositions refPos)
        {
            pixelsRef = new FourPoints(refPos.top, refPos.bottom, refPos.left, refPos.right);
        }

        private FourPoints ReadCalibFile(string calibType)
        {
            string sFile = FolderHelper.GetConfigFolder() + calibType + ".txt";
            var strs = File.ReadAllLines(sFile);
            int top = GetValue(Dir.top,strs);//int.Parse(strs[(int)Dir.top]);
            int right = GetValue(Dir.right, strs);
            int bottom = GetValue(Dir.bottom, strs);
            int left = GetValue(Dir.left, strs);
            return new FourPoints(top, bottom, left, right);
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

    class FourPoints
    {
        public int top;
        public int bottom;
        public int left;
        public int right;
        public FourPoints(int top, int bottom, int left, int right)
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
        }
    }
}
