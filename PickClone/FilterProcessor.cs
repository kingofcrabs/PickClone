using EngineDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PickClone
{
    class FilterProcessor
    {
        public List<MPoint> GetInterestedPts(MPoint[] points, int cnt)
        {
            List<MPoint> inPoints = new List<MPoint>();
            for(int i = 0; i< cnt; i++)
            {
                inPoints.Add(points[i]);
            }
            switch(Settings.Instance.SelectionMethod)
            {
                case SelectionMethod.biggest:
                    inPoints = inPoints.OrderBy(x => x.size).ToList();
                    break;
                case SelectionMethod.random:
                    inPoints = Randomize(inPoints).ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

             int cntWanted = Settings.Instance.CloneCnt;
             return inPoints.Take(cntWanted).ToList();
        }


        private IEnumerable<T> Randomize<T>(IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }
    }
}
