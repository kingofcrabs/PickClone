using EngineDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PickClone
{

    enum EditType
    {
        view = 0,
        add = 1,
        delete = 2
    }
    class CloneInfo
    {
        Point position;
        public CloneInfo(int ID, Point pos)
        {
            this.ID = ID;
            this.position = pos;
        }
        public int ID { get; set; }
        public string PositionString
        {
            get
            {
                return string.Format("{0}:{1}", position.X, position.Y);
            }
        }

        public static CloneInfo FromMPoint(MPoint pt)
        {
            return new CloneInfo(pt.ID, new Point(pt.x, pt.y));
        }
    }
}
