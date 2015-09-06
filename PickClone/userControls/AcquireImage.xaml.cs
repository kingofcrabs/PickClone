using Do3Acquier;
using EngineDll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
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

namespace PickClone.userControls
{
    /// <summary>
    /// AcquireImage.xaml 的交互逻辑
    /// </summary>
    public partial class AcquireImageForm : UserControl,IStage
    {
        Stage m_CurStage = Stage.Acquire;
        ImageAcquirer imageAcquirer = new ImageAcquirer();
        Bitmap _latestFrame = null;
        public AcquireImageForm()
        {
            InitializeComponent();
            imageAcquirer.onFinished += imageAcquirer_onFinished;
        }

        

        private void StartAcquire()
        {
            EnableButtons(false);
            //bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            if (!ConfigValues.UseTestImage)
            {
                StartRealAcquire();
            }
            else
            {
                HideLoadingImage();
                RefreshImage();
                Analysis();
                EnableButtons(true);
            }
        }

        void imageAcquirer_onFinished(string errMsg)
        {
            this.Dispatcher.BeginInvoke(
             (Action)delegate()
             {
                 HideLoadingImage();
                 if (errMsg != "")
                 {
                     resultCanvas.Children.Clear();
                     SetInfo(errMsg + "请重新连接相机线。关闭程序，再来一次。", System.Windows.Media.Brushes.Red);
                     return;
                 }
                 RefreshImage();
                 Analysis();
                 EnableButtons(true);
             });
        }

        private void Analysis()
        {
            IEngine iEngine = new IEngine();
            string sImgPath = ConfigValues.UseTestImage ? FolderHelper.GetTestImagePath() : FolderHelper.GetLatestImagePath();
            iEngine.Load(sImgPath);
            MPoint[] points = new MPoint[200];
            int cnt = 0;
            RefPositions refPositions = new RefPositions();
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200), refPositions, ref cnt, ref points);
            if( cnt > 0)
            {
               var pts = GetFirstNPts(points, cnt);
               resultCanvas.SetMarkFlags(pts);
               UpdateBackgroundImage(sImgPath);
            }
            
            SetInfo(string.Format("找到{0}个克隆。", cnt), false);
           
        }

        private void UpdateBackgroundImage(string markedImageFile)
        {
            var imgSource = ImageHelper.BitmapFromFile(markedImageFile);
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = imgSource;
            //imgBrush.Stretch = Stretch.Uniform;
            resultCanvas.Background = imgBrush;
            resultCanvas.ImageSize = new System.Windows.Size(imgSource.Width, imgSource.Height);
        }

        private List<MPoint> GetFirstNPts(MPoint[] points, int retCnt)
        {
            
            int cntConstrain = GlobalVars.Instance.Settings.CloneCnt;
            int min = Math.Min(retCnt, cntConstrain);
            var pts = new List<MPoint>();
            if (GlobalVars.Instance.Settings.SelectionMethod == SelectionMethod.random)
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

        private void SetInfo(string s, bool hasError = true)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = hasError ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            StartAcquire();
        }

        private void EnableButtons(bool bEnable)
        {
            btnRefresh.IsEnabled = bEnable;
        }

        private void RefreshImage()
        {
            try
            {
                _latestFrame = Mics.LoadLatestImage(ConfigValues.UseTestImage, FolderHelper.GetTestImagePath());
                resultCanvas.UpdateBackGroupImage(_latestFrame);
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, System.Windows.Media.Brushes.Red);
            }
       

        }

        private void SetInfo(string s, System.Windows.Media.Brush color)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = color;
        }

        void HideLoadingImage()
        {
            loading.Visibility = Visibility.Hidden;
            resultCanvas.Visibility = System.Windows.Visibility.Visible;
        }

        void StartRealAcquire()
        {
            System.Threading.Thread t1 = new System.Threading.Thread
              (delegate()
              {
                  imageAcquirer.Start(FolderHelper.GetLatestImagePath(), 1);
              });
            t1.Start();
        }


        public void OnNavigateTo(Stage stage)
        {
            this.Visibility = stage == m_CurStage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            
        }
    }



    public static class ExtCanvas
    {
         public static void UpdateBackGroupImage(this Canvas canvas, Bitmap bitmap)
        {
            BitmapImage bitmapImage;
            System.Drawing.Bitmap cloneBitmpa = (System.Drawing.Bitmap)bitmap.Clone();
            using (MemoryStream memory = new MemoryStream())
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
            imgBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            imgBrush.Viewbox = new Rect(0, 0, 1, 1);
            imgBrush.Stretch = Stretch.Uniform;
            canvas.Background = imgBrush;
        }
    }
}
