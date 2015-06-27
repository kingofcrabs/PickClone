using EngineDll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PickClone
{
    public class MarkCanvas : Canvas
    {
        List<MPoint> pts = null;
        public MarkCanvas()
        {
            //this.MouseLeftButtonUp += MarkCanvas_MouseLeftButtonUp;
        }

        internal void LeftButtonUp(Point point, EditType editType)
        {
            if(editType == EditType.view)
            {
                ViewThePoint(point);
            }
        }

        private void ViewThePoint(Point point)
        {
            if (pts.Count == 0)
                return;
            Point ptInImage = Convert2BitmapCoord(point);
            pts.ForEach(x => x.isCurrent = false);
            var shortestDis = pts.Min(x => GetDistance(ptInImage, x));
            var closest = pts.First(x => GetDistance(ptInImage,x) == shortestDis);
            closest.isCurrent = true;
            InvalidateVisual();
        }

        private double GetDistance(Point ptInImage, MPoint ptThis)
        {
            double disX = ptInImage.X - ptThis.x;
            double disY = ptInImage.Y - ptThis.y;
            return Math.Sqrt(disX * disX + disY * disY);
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            base.OnRender(dc);
            if (pts == null)
                return;

            foreach(var pt in pts)
            {
                DrawRect(pt,dc);
            }
        }

        private void DrawRect(MPoint pt, System.Windows.Media.DrawingContext dc)
        {
            Point ptUI = Convert2UICoord(pt);
            Brush brush = pt.isCurrent ? Brushes.Red : Brushes.DarkBlue;
     
            dc.DrawRectangle(null, new System.Windows.Media.Pen(brush, 1), GetBoundingRect(ptUI));
             FormattedText text = new FormattedText(pt.ID.ToString(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("SimSun"),
            18,
            Brushes.Black);
            dc.DrawText(text,ptUI);
        }

        private Rect GetBoundingRect(Point ptUI)
        {
            Size sz = new Size(20,20);
            Point ptLeftTop = new Point(ptUI.X - sz.Width / 2,ptUI.Y - sz.Height/2);
            Point ptRightBottom = new Point(ptUI.X + sz.Width / 2, ptUI.Y + sz.Height/2);
            return new Rect(ptLeftTop, ptRightBottom);
        }

        private Point Convert2UICoord(MPoint pt)
        {
            return new Point((int)(pt.x / ImageSize.Width * this.ActualWidth),(int)( pt.y / ImageSize.Height * this.ActualHeight));
        }

        private Point Convert2BitmapCoord(Point pt)
        {
            return new Point((int)(pt.X / this.ActualWidth * ImageSize.Width), (int)(pt.Y / this.ActualHeight * ImageSize.Height));
        }

        internal void SetMarkFlags(List<EngineDll.MPoint> pts)
        {
            this.pts = pts;
        }

        public System.Windows.Size ImageSize { get; set; }

      
    }
}
