using EngineDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PickClone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBackgroundImage(@"F:\temp\test.jpg");
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            IEngine iEngine = new IEngine();
            iEngine.Load(@"F:\temp\test.jpg");
            MPoint[] points = new MPoint[100];
            int cnt = 0;
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200),ref cnt, ref points);
            UpdateBackgroundImage(markedImageFile);
            List<MPoint> pts = GetFirst5Pts(points,cnt);
            resultCanvas.SetMarkFlags(pts);
        }

        private List<MPoint> GetFirst5Pts(MPoint[] points,int cnt)
        {
            int cntConstrain;
            bool bok = int.TryParse(txtCnt.Text, out cntConstrain);
            
            int min = Math.Min(cnt, 5);
            var pts =  new List<MPoint>();
            Random rnd=new Random();
            points = points.OrderBy(x => rnd.Next()).ToArray();
            for (int i = 0; i < min; i++)
            {
                pts.Add(new MPoint(points[i].x, points[i].y, i + 1));
            }
            return pts;
        }

        private void UpdateBackgroundImage(string resFile)
        {
            var imgSource = Helper.BitmapFromFile(resFile);
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = imgSource;
            resultCanvas.Background = imgBrush;
            resultCanvas.ImageSize = new Size(imgSource.Width, imgSource.Height);
        }
    }
}
