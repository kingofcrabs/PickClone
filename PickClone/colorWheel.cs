using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows;


namespace TestHSV
{
    class ColorWheel : FrameworkElement
    {
        HSLColor hslColor = new HSLColor();
        Color systemColor = new Color();
        System.Windows.Media.Imaging.BitmapSource imgSrc = null;
        Color pixelcolor = new Color();
        Bitmap bmp = new Bitmap(240, 220);
        public ColorWheel()
        {
            pixelcolor = Color.Red;
            hslColor.SetRGB(pixelcolor.R, pixelcolor.G, pixelcolor.B);

            for (int y = 0; y < 220; y++)
            {
                //pixelcolor = Color.Red;
                //hslColor.SetRGB(pixelcolor.R, pixelcolor.G, pixelcolor.B);
                hslColor.Hue = 0;
                for (int x = 0; x < 240; x++)
                {
                    systemColor = hslColor;
                    bmp.SetPixel(x, y, systemColor);
                    hslColor.Hue += 1;
                }
                hslColor.Saturation -= (y * 0.01);

            }
            imgSrc = CreateBitmapSourceFromGdiBitmap(bmp);
        }

        public System.Windows.Media.Color GetColor(System.Windows.Point pt)
        {
            int x = (int)(bmp.Width / this.RenderSize.Width * pt.X);
            int y = (int)(bmp.Height / this.RenderSize.Height * pt.Y);
            var color = bmp.GetPixel(x,y);
            return System.Windows.Media.Color.FromArgb(color.A,color.R,color.G,color.B);
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (imgSrc == null)
                return;
            
            drawingContext.DrawImage(imgSrc,new Rect(this.RenderSize));
        }

        public static System.Windows.Media.Imaging.BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return System.Windows.Media.Imaging.BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
