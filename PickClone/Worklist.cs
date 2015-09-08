using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PickClone
{
    class Worklist
    {
        int curTipIndex = 0;
        List<int> tipSelections = new List<int>();
        Calibration calib = new Calibration();
        public Worklist(RefPositions refPositions)
        {
            int totalTipCnt = int.Parse(ConfigurationManager.AppSettings["tipCount"]); 
            for (int i = 0; i < totalTipCnt; i++)
            {
                tipSelections.Add((int)Math.Pow(2, i));
            }
            calib.SetRefPixels(refPositions);
        }

        public List<string> Generate(List<MPoint> pts)
        {
            List<string> strs = new List<string>();
            strs.Add("W;");
            foreach(MPoint pt in pts)
            {
                strs.AddRange(Generate(pt));
            }
            return strs;
        }

        private IEnumerable<string> Generate(MPoint pt)
        {
            List<string> strs = new List<string>();
            int index = curTipIndex % tipSelections.Count;
            curTipIndex++;
            int tipSelection = tipSelections[index];
            strs.Add("B;Command(\"C5 PAZ2100,2100,2100,2100,2100,2100,2100,2100\",1,1,,,2,2,0);");
            string movX = string.Format("B;Command(\"C5 PAX{0}\",1,1,,,2,2,0);",calib.ConvertX(pt.x));
            strs.Add(movX);
            string movY = string.Format("B;Command(\"C5 PAY{0}\",1,1,,,2,2,0);",calib.ConvertY(pt.y));
            strs.Add(movY);
            strs.Add("W;");
            strs.Add("B;Command(\"C5 PAZ2100,2100,2100,2100,2100,2100,2100,2100\",1,1,,,2,2,0);");
            strs.Add("B;Command(\"C5 PAZ950,1050,1050,1050,1050,1050,1050,1050\",1,1,,,2,2,0);");
            strs.Add("B;Command(\"C5 SDM0,0\",1,1,,,2,2,0)");
            string mdt = string.Format("B;Command(\"C5 MDT{0},0,950,850\",1,1,,,2,2,0)", tipSelection);
            strs.Add(mdt);
            return strs;
        }


    }
}
