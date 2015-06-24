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
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EditType editType = EditType.view;
            if((bool)rdbAdd.IsChecked)
            {
                editType = EditType.add;
            }
            else if((bool)rdbDelete.IsChecked)
            {
                editType = EditType.delete;
            }
            resultCanvas.LeftButtonUp(e.GetPosition(this.resultCanvas), editType);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBackgroundImage(@"F:\temp\test.jpg");
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            #region check settings
            bool bok = CheckSettings();
            if (!bok)
                return;

            #endregion
            IEngine iEngine = new IEngine();
            iEngine.Load(@"F:\temp\test.jpg");
            MPoint[] points = new MPoint[100];
            int cnt = 0;
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200),ref cnt, ref points);
            UpdateBackgroundImage(markedImageFile);
            List<MPoint> pts = GetFirstNPts(points,cnt);
            resultCanvas.SetMarkFlags(pts);
        }

        private bool CheckSettings()
        {
            int cntConstrain;
            bool bok = int.TryParse(txtCnt.Text, out cntConstrain);
            if (!bok)
                SetInfo("克隆数量必须大于0");
            return bok;
        }

        private void SetInfo(string s, bool bError = true)
        {
            txtInfo.Foreground = bError ? Brushes.Red : Brushes.Black;
            txtInfo.Text = s;
        }

        private List<MPoint> GetFirstNPts(MPoint[] points, int retCnt)
        {
            int cntConstrain = int.Parse(txtCnt.Text);
            int min = Math.Min(retCnt, cntConstrain);
            var pts =  new List<MPoint>();
            if((bool)rdbRandom.IsChecked)
            {
                Random rnd = new Random();
                points = points.OrderBy(x => rnd.Next()).ToArray();
            }
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
