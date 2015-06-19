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
            iEngine.MarkClones(new ConstrainSettings(10, 200),ref cnt, ref points);
        }

        private void UpdateBackgroundImage(string resFile)
        {
            var imgSource = Helper.BitmapFromFile(resFile);
            imgHolder.Background = new ImageBrush(imgSource);
        }
    }
}
