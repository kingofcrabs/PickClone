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
        public Worklist()
        {
            int totalTipCnt = int.Parse(ConfigurationManager.AppSettings["tipCount"]); 
            for (int i = 0; i < totalTipCnt; i++)
            {
                tipSelections.Add((int)Math.Pow(2, i));
            }
        }

        public List<List<string>> Generate(List<MPoint> pts)
        {
            List<List<string>> strList = new List<List<string>>();
            int tipCnt = tipSelections.Count;
            while(pts.Count > 0)
            {
                var batchPts = pts.Take(tipCnt);
                strList.Add(Generate(batchPts));
                pts = pts.Skip(tipCnt).ToList();
            }
            return strList;
        }

        private List<string> Generate(IEnumerable<MPoint> batchPts)
        {
            List<string> strs = new List<string>();
            strs.Add("W;");
            foreach (MPoint pt in batchPts)
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
            string movX = string.Format("B;Command(\"C5 PAX{0}\",1,1,,,2,2,0);",Calibration.Instance.ConvertX(pt.x));
            strs.Add(movX);
            string movY = string.Format("B;Command(\"C5 PAY{0}\",1,1,,,2,2,0);", Calibration.Instance.ConvertY(pt.y));
            strs.Add(movY);
            strs.Add("W;");
            strs.Add("B;Command(\"C5 PAZ2100,2100,2100,2100,2100,2100,2100,2100\",1,1,,,2,2,0);");
            string paz = GetPAZ(index);
            strs.Add(string.Format("B;Command(\"C5 PAZ{0}\",1,1,,,2,2,0);",paz));
            strs.Add("B;Command(\"C5 SDM0,0\",1,1,,,2,2,0)");
            string mdt = string.Format("B;Command(\"C5 MDT{0},0,950,850\",1,1,,,2,2,0)", tipSelection);
            strs.Add(mdt);
            return strs;
        }

        private string GetPAZ(int index)
        {
            string wholeStr = "";
            var lowerestVal = ConfigurationManager.AppSettings["zValue"];
            for(int i = 0; i< 8; i++)
            {
                var sVal = i == index ? lowerestVal : "2100";
                if( i != 8 -1)
                {
                    sVal += ",";
                }
                wholeStr += sVal;
            }
            return wholeStr;
        }


    }
}
