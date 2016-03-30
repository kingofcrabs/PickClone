using CameraControl;
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
            renderGrid.SizeChanged += renderGrid_SizeChanged;
            Settings.Instance.Load();
            sliderExposureTime.Value = Settings.Instance.ExposureTime;
        }
        public void OnNavigateTo(Stage stage)
        {
            this.Visibility = stage == m_CurStage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

        }

        #region ui controls
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            StartAcquire();
        }

        private void sliderExposureTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Instance.ExposureTime = (ulong)sliderExposureTime.Value;
        }

        void renderGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resultCanvas.Resize();
        }
        #endregion

        #region acquisition
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
            string extraHint = imageAcquirer.IsInitialed ? "" : "第一次初始化较慢，";
            SetInfo(string.Format("开始采集，{0}请耐心等待。",extraHint),false);
            resultCanvas.Markers = null;
            //bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            if (!ConfigValues.UseTestImage)
            {
                imageAcquirer.SetExposureTime(Settings.Instance.ExposureTime);
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
                resultCanvas.Markers = null;
                imageAcquirer_onFinished("");
            }
        }

        void imageAcquirer_onFinished(string errMsg)
        {
            this.Dispatcher.Invoke(
             (Action)delegate()
             {
                 EnableButtons(true);
                 if (errMsg != "")
                 {
                     resultCanvas.Children.Clear();
                     SetInfo(errMsg + "请重新连接相机线。关闭程序，再来一次。", System.Windows.Media.Brushes.Red);
                     return;
                 }
                 RefreshImage();
                 resultCanvas.Resize();
                 SetInfo("开始分析", false);
                 DoEvents();
                 Analysis();
                 resultCanvas.Resize();

             });

        }
        #endregion

        #region analysis
        private void GenerateWorklist(List<MPoint> pts)
        {
            Worklist worklist = new Worklist();
            var strLists = worklist.Generate(pts);
            string cntFile = FolderHelper.GetOutputFolder() + "count.txt";
            File.WriteAllText(cntFile,strLists.Count.ToString());
            for (int i = 0; i < strLists.Count; i++ )
            {
                var strs = strLists[i];
                string sFile = FolderHelper.GetOutputFolder() + string.Format("{0}.gwl",i+1);
                File.WriteAllLines(sFile, strs);
            }
        }

        private List<MPoint> GetFirstNPts(MPoint[] points, int retCnt)
        {

            int cntConstrain = Settings.Instance.CloneCnt;
            int min = Math.Min(retCnt, cntConstrain);
            var pts = new List<MPoint>();
            if (Settings.Instance.SelectionMethod == SelectionMethod.random)
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
#if DEBUG
            File.Copy(sImgPath, FolderHelper.GetDataFolder() + "test.jpg",true);
#endif
            iEngine.Load(sImgPath);
            MPoint[] points = new MPoint[200];
            int cnt = 0;
            RefPositions refPositions = new RefPositions();
            string markedImageFile = iEngine.MarkClones(new ConstrainSettings(Settings.Instance.MinArea, Settings.Instance.MaxArea),
                refPositions, ref cnt, ref points);
            try
            {
                CheckRefPoints(refPositions);
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }

            Calibration.Instance.SetRefPixels(refPositions);
            if (cnt > 0)
            {
                FilterProcessor filterProcessor = new FilterProcessor();
                var pts = filterProcessor.GetInterestedPts(points, cnt); ;
                resultCanvas.Markers = pts;
                UpdateBackgroundImage(sImgPath);
                GenerateWorklist(pts);
            }
            var timeSpan = DateTime.Now - startTime;
            int seconds = (int)timeSpan.TotalSeconds;
            SetInfo(string.Format("找到{0}个克隆。用时:{1}秒", cnt, seconds), false);
        }

        private void CheckRefPoints(RefPositions refPositions)
        {
            bool numInvalid = (refPositions.bottom == refPositions.top || refPositions.left == refPositions.right);//check validity
            int width = refPositions.right - refPositions.left;
            bool invalidWidth = ( Math.Abs(width - 1300) > 50);
            int height = refPositions.bottom - refPositions.top;
            bool invalidHeight = (Math.Abs(height - 1300) > 50);
            if (invalidHeight || invalidWidth || numInvalid)
                throw new Exception("未能找到参考点！");
                
        }
        #endregion

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

        private void UpdateBackgroundImage(string sImage)
        {
            System.Drawing.Bitmap latestImage = ImageHelper.LoadLatestImage(sImage);
            resultCanvas.UpdateBackGroundImage(latestImage);
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

     
        
        #endregion

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
                StartAcquire();
            }

            if(sCommand == "a")
            {
               
            }
        }
        #endregion
    }


}
