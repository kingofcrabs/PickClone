using EngineDll;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        ObservableCollection<CloneInfo> cloneInfos = new ObservableCollection<CloneInfo>();
        public MainWindow()
        {
            InitializeComponent();
            ImageHelper.ImagePath = FolderHelper.GetLatestImage();
            this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            this.Loaded += MainWindow_Loaded;
            lvCloneInfos.ItemsSource = cloneInfos;
            lvCloneInfos.SelectionChanged += lvCloneInfos_SelectionChanged;
        }

        void lvCloneInfos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(lvCloneInfos.SelectedItem.ToString());
            e.Handled = true;
            if (lvCloneInfos == null || lvCloneInfos.Items.Count == 0)
                return;
            if (lvCloneInfos.SelectedItem == null)
                return;
            var cloneInfo = (CloneInfo)lvCloneInfos.SelectedItem;
            Point ptInUICoord = resultCanvas.Convert2UICoord(cloneInfo.PositionString);
            OnMouseLeftButtonDown(ptInUICoord);
            
        }

        private void OnMouseLeftButtonDown(Point ptInUICoord)
        {
            EditType editType = EditType.view;
            if ((bool)rdbAdd.IsChecked)
            {
                editType = EditType.add;
            }
            else if ((bool)rdbDelete.IsChecked)
            {
                editType = EditType.delete;
            }
            UpdateSubImage(ptInUICoord);
            resultCanvas.LeftButtonUp(ptInUICoord, editType);
        }

        void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lvCloneInfos.IsMouseOver)
                return;
            OnMouseLeftButtonDown(e.GetPosition(this.resultCanvas));
        }

        private void UpdateSubImage(Point point)
        {
            BitmapImage img = ImageHelper.BitmapFromFile(ImageHelper.ImagePath);
            var imageBrush = new ImageBrush( img);
            //imageBrush.Viewport = new Rect(0.1, 0.321, 0.7, 0.557);
            double xRatio = point.X / resultCanvas.ActualWidth;
            double yRatio = point.Y / resultCanvas.ActualWidth;
            //imageBrush.Viewport = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
            imageBrush.Viewbox = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
            rectSubImage.Fill = imageBrush;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBackgroundImage();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            #region check settings
            bool bok = CheckSettings();
            if (!bok)
                return;

            #endregion
            IEngine iEngine = new IEngine();
            iEngine.Load(ImageHelper.ImagePath);
            MPoint[] points = new MPoint[100];
            int cnt = 0;
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200),ref cnt, ref points);
            UpdateBackgroundImage();
            List<MPoint> firstNPts = GetFirstNPts(points,cnt);
            cloneInfos.Clear();
            firstNPts.ForEach(x=>cloneInfos.Add(CloneInfo.FromMPoint(x))); //convert all firstNPts to cloneInfos
            
            resultCanvas.SetMarkFlags(firstNPts);
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

        private void UpdateBackgroundImage()
        {
            var imgSource = ImageHelper.BitmapFromFile(ImageHelper.ImagePath);
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = imgSource;
            resultCanvas.Background = imgBrush;
            resultCanvas.ImageSize = new Size(imgSource.Width, imgSource.Height);
        }
    }
}
