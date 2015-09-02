using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        List<CloneInfo> cloneInfos = new List<CloneInfo>();
        string imageName = "";
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            imageName = GetLatestImage();
            this.Loaded += MainWindow_Loaded;
        }

        private string GetLatestImage()
        {
            string dir = ConfigurationManager.AppSettings["imageFolder"];
            var files = Directory.EnumerateFiles(dir, "*.jpg");
            List<FileInfo> fileInfos = new List<FileInfo>();
            foreach(var file in files)
            {
                fileInfos.Add(new FileInfo(file)); 
            }
            var latest = fileInfos.OrderBy(x => x.CreationTime).Last();
            return latest.FullName;
        }

        void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UpdateSubImage(e.GetPosition(this.resultCanvas));
            resultCanvas.LeftButtonUp(e.GetPosition(this.resultCanvas));
        }

        private void UpdateSubImage(Point point)
        {
            BitmapImage img = ImageHelper.BitmapFromFile(imageName);
            var imageBrush = new ImageBrush(img);
            //imageBrush.Viewport = new Rect(0.1, 0.321, 0.7, 0.557);
            double xRatio = point.X / resultCanvas.ActualWidth;
            double yRatio = point.Y / resultCanvas.ActualWidth;
            //imageBrush.Viewport = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
            imageBrush.Viewbox = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
            rectSubImage.Fill = imageBrush;
        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBackgroundImage(imageName);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            #region check settings
            bool bok = CheckSettings();
            if (!bok)
                return;
            CreateOutputFolder();
          
            #endregion
            IEngine iEngine = new IEngine();
            iEngine.Load(imageName);
            MPoint[] points = new MPoint[200];
            int cnt = 0;
            RefPositions refPositions = new RefPositions();
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200),refPositions,ref cnt, ref points);
            UpdateBackgroundImage(markedImageFile);
            SetInfo(string.Format("找到{0}个克隆。", cnt),false);
            List<MPoint> pts = GetFirstNPts(points,cnt);
            UpdateCloneInfos(pts);
            resultCanvas.SetMarkFlags(pts);
        }

        private void CreateOutputFolder()
        {
            string dir = ConfigurationManager.AppSettings["imageFolder"];
            string sOutputFolder = dir + "\\output\\";
            if (!Directory.Exists(sOutputFolder))
                Directory.CreateDirectory(sOutputFolder);
        }

        private void UpdateCloneInfos(List<MPoint> pts)
        {
            cloneInfos.Clear();
            for(int i = 0; i < pts.Count; i++)
            {
                cloneInfos.Add(new CloneInfo(i+1,new Point(pts[i].x,pts[i].y)));
            }
            lvCloneInfos.ItemsSource = cloneInfos;
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
            var imgSource = ImageHelper.BitmapFromFile(resFile);
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = imgSource;
            resultCanvas.Background = imgBrush;
            resultCanvas.ImageSize = new Size(imgSource.Width, imgSource.Height);
        }

        #region commands
        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpForm helpForm = new HelpForm();
            helpForm.ShowDialog();
        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

    }
}
