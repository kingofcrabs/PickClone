using Do3Acquier;
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
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            if (!bUseTestImage)
            {
                StartRealAcquire();
            }
            else
            {
                HideLoadingImage();
                RefreshImage();
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
                     myCanvas.Children.Clear();
                     SetInfo(errMsg + "请重新连接相机线。关闭程序，再来一次。", System.Windows.Media.Brushes.Red);
                     return;
                 }
                 RefreshImage();
                 EnableButtons(true);
             });
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
            bool bUseTestImage = bool.Parse(ConfigurationManager.AppSettings["useTestImage"]);
            try
            {
                _latestFrame = Mics.LoadLatestImage(bUseTestImage, FolderHelper.GetTestImagePath());
                myCanvas.UpdateBackGroupImage(_latestFrame);
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
            myCanvas.Visibility = System.Windows.Visibility.Visible;
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
