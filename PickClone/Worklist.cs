using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PickClone
{
    public enum EVOModel
    {
        EVO75,
        EVO100,
        EVO150,
        EVO200
    };

    class Worklist
    {
        int curTipIndex = 0;
        List<int> tipSelections = new List<int>();
        int zStart = int.Parse(ConfigurationManager.AppSettings["zStart"]);
        int zEnd = int.Parse(ConfigurationManager.AppSettings["zEnd"]);
        int wasteGrid = int.Parse(ConfigurationManager.AppSettings["wasteGrid"]);
        EVOModel model = GetModel(ConfigurationManager.AppSettings["EVOModel"]);
        int lightGrid = int.Parse(ConfigurationManager.AppSettings["lightGrid"]);
        private static EVOModel GetModel(string sModel)
        {
            Dictionary<string, EVOModel> strModelPairs = new Dictionary<string, EVOModel>();
            strModelPairs.Add("75", EVOModel.EVO75);
            strModelPairs.Add("100", EVOModel.EVO100);
            strModelPairs.Add("150", EVOModel.EVO150);
            strModelPairs.Add("200", EVOModel.EVO200);
            return strModelPairs[sModel];
        }
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
        int GetTipSelection(int cnt)
        {
            int ditiMask = 0; 
            for (int i = 0; i < cnt; i++)
                ditiMask += (int)Math.Pow(2, i);
            return ditiMask;
        }
        private List<string> Generate(IEnumerable<MPoint> batchPts)
        {
            List<string> strs = new List<string>();
            strs.Add("W;");
            int ditiMask = GetTipSelection(batchPts.Count());
            strs.Add(string.Format("B;GetDiti2({0},\"DiTi 200ul LiHa\",0,0,10,70);",ditiMask ));
            string sMoveLiha = string.Format("B;MoveLiha({0},{1},0,1,\"02021\",0,1,0,10,0,0);", 1, lightGrid);
            strs.Add(sMoveLiha);
            int i = 0;
            foreach(var pt in batchPts)
            {
                strs.AddRange(Generate(pt, i++));
            }
            strs.Add(string.Format("B;DropDiti({0},{1},2,10,70,0);", ditiMask,wasteGrid));
            return strs;
        }

        private IEnumerable<string> Generate(MPoint pt,int tipIndex)
        {
            List<string> strs = new List<string>();
            int index = curTipIndex % tipSelections.Count;
            curTipIndex++;
            int tipSelection = tipSelections[index];
            strs.Add("B;Command(\"C5 PAZ2100,2100,2100,2100,2100,2100,2100,2100\",1,1,,,2,2,0);");
            string movX = string.Format("B;Command(\"C5 PAX{0}\",1,1,,,2,2,0);",Calibration.Instance.ConvertX(pt.x));
            strs.Add(movX);
            int yOffset = GetTipYOffset(tipIndex);
            string movY = string.Format("B;Command(\"C5 PAY{0},90\",1,1,,,2,2,0);", Calibration.Instance.ConvertY(pt.y) - yOffset);
            strs.Add(movY);
            string paz = GetPAZ(index);
            strs.Add(string.Format("B;Command(\"C5 PAZ{0}\",1,1,,,2,2,0);",paz));
            strs.Add("B;Command(\"C5 SDM0,0\",1,1,,,2,2,0)");
            string mdt = string.Format("B;Command(\"C5 MDT{0},0,{1},{2}\",1,1,,,2,2,0)", tipSelection,zStart,zEnd);
            strs.Add(mdt);
            return strs;
        }

        private int GetTipYOffset(int tipIndex)
        {
            int unit = model == EVOModel.EVO75 ? 180 : 90;
            return tipIndex * unit;
        }

        private string GetPAZ(int index)
        {
            string wholeStr = "";
            
            for(int i = 0; i< 8; i++)
            {
                var sVal = i == index ? zStart.ToString() : "2100";
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
