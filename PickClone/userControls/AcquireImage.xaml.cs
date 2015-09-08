using Do3Acquier;
using EngineDll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PickClone.userControls
{
    /// <summary>
    /// AcquireImage.xaml 的交互逻辑
    /// </summary>
    public partial class AcquireImageForm : UserControl,IStage
    {
        Stage m_CurStage = Stage.Acquire;
        ImageAcquirer imageAcquirer = new ImageAcquirer();
        DateTime startTime;
        
        public AcquireImageForm()
        {
            InitializeComponent();
            CreateNamedPipeServer();
            imageAcquirer.onFinished += imageAcquirer_onFinished;
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

       
        private void StartAcquire()
        {
            EnableButtons(false);
            SetInfo("开始采集",false);
            //bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            if (!ConfigValues.UseTestImage)
            {
                StartRealAcquire();
            }
            else
            {
                //simulate delay
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                    DoEvents();
                }
                resultCanvas.SetMarkFlags(null);
                imageAcquirer_onFinished("");
            }
        }

        private void GenerateWorklist(List<MPoint> pts, RefPositions refPositions)
        {
            Worklist worklist = new Worklist(refPositions);
            var strs = worklist.Generate(pts);
            string sFile = FolderHelper.GetOutputFolder() + "latest.gwl";
            File.WriteAllLines(sFile, strs);
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
        private void Analysis()
        {
            startTime = DateTime.Now;
            IEngine iEngine = new IEngine();
            string sImgPath = FolderHelper.GetLatestImagePath();
            iEngine.Load(sImgPath);
            MPoint[] points = new MPoint[200];
            int cnt = 0;
            RefPositions refPositions = new RefPositions();
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(10, 200), refPositions, ref cnt, ref points);
            if (cnt > 0)
            {
                var pts = GetFirstNPts(points, cnt);
                resultCanvas.SetMarkFlags(pts);
                UpdateBackgroundImage(sImgPath);
                GenerateWorklist(pts, refPositions);
            }
            var timeSpan = DateTime.Now - startTime;
            int seconds = (int)timeSpan.TotalSeconds;
            SetInfo(string.Format("找到{0}个克隆。用时:{1}秒", cnt, seconds), false);
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
                 SetInfo("开始分析",false);
                 DoEvents();
                 Thread.Sleep(100);
                 DoEvents();
                 Analysis();
                 EnableButtons(true);
             });
        }
        #region refresh helper
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        #endregion
        
        #region ui helper

        private void UpdateBackgroundImage(string markedImageFile)
        {
            var imgSource = ImageHelper.BitmapFromFile(markedImageFile);
            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource = imgSource;
            imgBrush.Stretch = Stretch.Uniform;
            imgBrush.AlignmentX = AlignmentX.Left;
            imgBrush.AlignmentY = AlignmentY.Top;
            resultCanvas.Background = imgBrush;
            resultCanvas.ImageSize = new System.Windows.Size(imgSource.Width, imgSource.Height);
        }


        private void SetInfo(string s, bool hasError = true)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = hasError ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

       
        private void EnableButtons(bool bEnable)
        {
            btnRefresh.IsEnabled = bEnable;
        }

        private void RefreshImage()
        {
            try
            {
                UpdateBackgroundImage(FolderHelper.GetLatestImagePath());
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

        void ShowLoadingImage()
        {
            loading.Visibility = Visibility.Visible;
            resultCanvas.Visibility = System.Windows.Visibility.Hidden;
            renderGrid.InvalidateVisual();
        }


        #endregion

        public void OnNavigateTo(Stage stage)
        {
            this.Visibility = stage == m_CurStage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            
        }
        #region pipeLine
        private void CreateNamedPipeServer()
        {
            Pipeserver.owner = this;
            Pipeserver.ownerInvoker = new Invoker(this);
            ThreadStart pipeThread = new ThreadStart(Pipeserver.createPipeServer);
            Thread listenerThread = new Thread(pipeThread);
            listenerThread.SetApartmentState(ApartmentState.STA);
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        internal void ExecuteCommand(string sCommand)
        {
            //StartAcquire();
            if(sCommand == "s")
            {
                ShowLoadingImage();
                
                StartAcquire();
            }

            if(sCommand == "a")
            {
                ShowLoadingImage();
            }
        }
        #endregion

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            StartAcquire();
        }


    }



    //public static class ExtCanvas
    //{
    //     public static void UpdateBackGroupImage(this Canvas canvas, Bitmap bitmap)
    //    {
    //        BitmapImage bitmapImage;
    //        System.Drawing.Bitmap cloneBitmpa = (System.Drawing.Bitmap)bitmap.Clone();
    //        using (MemoryStream memory = new MemoryStream())
    //        {
    //            cloneBitmpa.Save(memory, ImageFormat.Png);
    //            memory.Position = 0;
    //            bitmapImage = new BitmapImage();
    //            bitmapImage.BeginInit();
    //            bitmapImage.StreamSource = memory;
    //            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
    //            bitmapImage.EndInit();
    //        }
    //        ImageBrush imgBrush = new ImageBrush();
    //        imgBrush.ImageSource = bitmapImage;
    //        imgBrush.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
    //        imgBrush.Viewbox = new Rect(0, 0, 1, 1);
    //        imgBrush.Stretch = Stretch.Uniform;
    //        canvas.Background = imgBrush;
    //    }
    //}
}
