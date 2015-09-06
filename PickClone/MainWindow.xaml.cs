using EngineDll;
using PickClone.userControls;
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
        StepViewModel stepViewModel = new StepViewModel();
        List<UserControl> subForms = new List<UserControl>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
         
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lstSteps.DataContext = stepViewModel.StepsModel;
            var acquireImageForm = new AcquireImageForm();
            var settingsForm = new SettingsForm();

            acquireImageForm.Visibility = Visibility.Hidden;
            settingsForm.Visibility = Visibility.Hidden;
            subForms.Add(acquireImageForm);
            subForms.Add(settingsForm);
            foreach(var subForm in subForms)
                userControlHost.Children.Add(subForm);

            //settingsForm.Visibility = System.Windows.Visibility.Visible;
        }

        //public MainWindow(string[] p)
        //    :this()
        //{
        //    // TODO: Complete member initialization
        //    if(p.Length >0)
        //    {
        //        bSilent = true;
        //    }
        //}


        //void CapturePicture()
        //{

        //}
    

        //void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    UpdateSubImage(e.GetPosition(this.resultCanvas));
        //    resultCanvas.LeftButtonUp(e.GetPosition(this.resultCanvas));
        //}

        //private void UpdateSubImage(Point point)
        //{
        //    BitmapImage img = ImageHelper.BitmapFromFile(imageName);
        //    var imageBrush = new ImageBrush(img);
        //    //imageBrush.Viewport = new Rect(0.1, 0.321, 0.7, 0.557);
        //    double xRatio = point.X / resultCanvas.ActualWidth;
        //    double yRatio = point.Y / resultCanvas.ActualWidth;
        //    //imageBrush.Viewport = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
        //    imageBrush.Viewbox = new Rect(new Point(xRatio - 0.05, yRatio - 0.05), new Size(0.1, 0.1));
        //    rectSubImage.Fill = imageBrush;
        //}


        //void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    UpdateBackgroundImage(imageName);
        //}

        //private void btnApply_Click(object sender, RoutedEventArgs e)
        //{
        //    #region check settings
        //    bool bok = CheckSettings();
        //    if (!bok)
        //        return;
        //    CreateOutputFolder();
          
        //    #endregion
            
        //}

        //private void CreateOutputFolder()
        //{
        //    string dir = ConfigurationManager.AppSettings["imageFolder"];
        //    string sOutputFolder = dir + "\\output\\";
        //    if (!Directory.Exists(sOutputFolder))
        //        Directory.CreateDirectory(sOutputFolder);
        //}

        //private void UpdateCloneInfos(List<MPoint> pts)
        //{
        //    cloneInfos.Clear();
        //    for(int i = 0; i < pts.Count; i++)
        //    {
        //        cloneInfos.Add(new CloneInfo(i+1,new Point(pts[i].x,pts[i].y)));
        //    }
        //    lvCloneInfos.ItemsSource = cloneInfos;
        //}

        //private bool CheckSettings()
        //{
        //    int cntConstrain;
        //    bool bok = int.TryParse(txtCnt.Text, out cntConstrain);
        //    if (!bok)
        //        SetInfo("克隆数量必须大于0");
        //    return bok;
        //}

        //private void SetInfo(string s, bool bError = true)
        //{
        //    txtInfo.Foreground = bError ? Brushes.Red : Brushes.Black;
        //    txtInfo.Text = s;
        //}

        //private List<MPoint> GetFirstNPts(MPoint[] points, int retCnt)
        //{
        //    int cntConstrain = int.Parse(txtCnt.Text);
        //    int min = Math.Min(retCnt, cntConstrain);
        //    var pts =  new List<MPoint>();
        //    if((bool)rdbRandom.IsChecked)
        //    {
        //        Random rnd = new Random();
        //        points = points.OrderBy(x => rnd.Next()).ToArray();
        //    }
        //    for (int i = 0; i < min; i++)
        //    {
        //        pts.Add(new MPoint(points[i].x, points[i].y, i + 1));
        //    }
        //    return pts;
        //}

        //private void UpdateBackgroundImage(string resFile)
        //{
        //    var imgSource = ImageHelper.BitmapFromFile(resFile);
        //    ImageBrush imgBrush = new ImageBrush();
        //    imgBrush.ImageSource = imgSource;
        //    resultCanvas.Background = imgBrush;
        //    resultCanvas.ImageSize = new Size(imgSource.Width, imgSource.Height);
        //}

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    bool bok = CheckSettings();
        //    if (!bok)
        //        return;

        //    Settings settings = new Settings();
        //    settings.cloneCnt = int.Parse(txtCnt.Text);
        //    settings.selectionMethod = (bool)rdbMaxArea.IsChecked ? SelectionMethod.biggest : SelectionMethod.random;
        //    string sFile = FolderHelper.GetConfigFolder() + "settings.xml"; 
        //    settings.Save(sFile);

        //    SetInfo(string.Format("配置文件已被保存到：{0}",sFile),false);
        //}

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

        private void lstSteps_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lstSteps, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                Stage stage2Go = ((StepDesc)item.Content).CorrespondingStage;
                NavigateTo(stage2Go);
            }
        }

        private void NavigateTo(Stage stage)
        {
           
            foreach(var subForm in subForms)
            {
                IStage istage = (IStage)subForm;
                istage.OnNavigateTo(stage);
            }
            if (lstSteps == null)
                return;

            lstSteps.SelectedIndex = (int)stage;
        }

      

    }
}
