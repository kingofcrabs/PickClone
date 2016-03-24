using EngineDll;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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

        internal void LeftButtonUp(Point point)
        {
            //ViewClone(point);
            this.InvalidateVisual();
        }

        //private void ViewClone(Point point)
        //{
        //    if(pts == null || pts.Count == 0 )
        //        return;
            
        //    for(int i = 0; i< pts.Count; i++)
        //    {
        //        pts[i].isCurrent = false;
        //    }
        //    Point ptInImage = Convert2BitmapCoord(point);
        //    double minDistance = 10000;
        //    int minIndex = 0;
        //    for(int i = 0; i< pts.Count; i++)
        //    {
        //        double tmpDistance = GetDistance(ptInImage, pts[i]);
        //        if(tmpDistance < minDistance)
        //        {
        //            minDistance = tmpDistance;
        //            minIndex = i;
        //        }
        //    }
        //    pts[minIndex].isCurrent = true;
        //}

        private double GetDistance(Point ptInImage, MPoint mPoint)
        {
            int xDis = (int)(ptInImage.X - mPoint.x);
            int yDis = (int)(ptInImage.Y - mPoint.y);
            return Math.Sqrt(xDis * xDis + yDis * yDis);
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
         
            double screenRatio = this.ActualWidth / this.ActualHeight;
            double realRatio = ImageSize.Width / ImageSize.Height;
            double usableHeight, usableWidth;
            if (realRatio > screenRatio)//x方向占满
            {
                usableHeight = this.ActualHeight / (realRatio / screenRatio);
                usableWidth = this.ActualWidth;
                this.Height = usableHeight;
            }
            else //y方向占满
            {
                usableWidth = this.ActualWidth / (screenRatio / realRatio);
                usableHeight = this.ActualHeight;
                this.Width = usableWidth;
            }
            
            return new Point((int)(pt.x / ImageSize.Width * usableWidth), (int)(pt.y / ImageSize.Height * usableHeight));
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



        
        public void UpdateBackGroundImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            System.Drawing.Bitmap cloneBitmpa = (System.Drawing.Bitmap)bitmap.Clone();
            using(MemoryStream memory = new MemoryStream())
            {
                cloneBitmpa.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = bitmapImage;
            imgBrush.Stretch = Stretch.Uniform;
            ImageSize = new Size(bitmap.Size.Width,bitmap.Size.Height);
            Background = imgBrush;
        }
    }
}
