using EngineDll;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PickClone
{
    public class MarkCanvas : Canvas
    {
        List<MPoint> pts = null;
        public MarkCanvas()
        {

        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);
            if (pts == null)
                return;

            foreach(var pt in pts)
            {
                DrawRect(pt,false,dc);
            }
        }

        private void DrawRect(MPoint pt, bool isCurrent, System.Windows.Media.DrawingContext dc)
        {
            Point ptUI = Convert2UICoord(pt);
            Brush brush = isCurrent ? Brushes.Blue : Brushes.Green;
            dc.DrawRectangle(null, new System.Windows.Media.Pen(brush, 1), GetBoundingRect(ptUI));
             FormattedText text = new FormattedText(pt.ID.ToString(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("SimSun"),
            12,
            Brushes.Blue);
            dc.DrawText(text,ptUI);
        }

        private Rect GetBoundingRect(Point ptUI)
        {
            Size sz = new Size(10,10);
            Point ptLeftTop = new Point(ptUI.X - sz.Width / 2,ptUI.Y - sz.Height/2);
            Point ptRightBottom = new Point(ptUI.X + sz.Width / 2, ptUI.Y + sz.Height/2);
            return new Rect(ptLeftTop, ptRightBottom);
        }

        private Point Convert2UICoord(MPoint pt)
        {
            return new Point((int)(pt.x / ImageSize.Width * this.ActualWidth),(int)( pt.y / ImageSize.Height * this.ActualHeight));
        }

        internal void SetMarkFlags(List<EngineDll.MPoint> pts)
        {
            this.pts = pts;
        }

        public System.Windows.Size ImageSize { get; set; }
    }
}
