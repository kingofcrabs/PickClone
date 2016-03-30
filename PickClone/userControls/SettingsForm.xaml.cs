using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TestHSV;
using System.Text.RegularExpressions;
using System;

namespace PickClone.userControls
{
    /// <summary>
    /// SettingsForm.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsForm : UserControl,IStage
    {
        Stage m_CurStage = Stage.Setting;
        public SettingsForm()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            rdbMaxArea.IsChecked = Settings.Instance.SelectionMethod == SelectionMethod.biggest;
            rdbRandom.IsChecked = !rdbMaxArea.IsChecked;
            this.DataContext = Settings.Instance;
            hsvGrid.Children.Add(new ColorWheel());
            hsvGrid.MouseEnter += hsvGrid_MouseEnter;
            hsvGrid.MouseLeave += hsvGrid_MouseLeave;
            hsvGrid.MouseLeftButtonUp += hsvGrid_MouseLeftButtonUp;
        }

        void hsvGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(hsvGrid);
            ColorWheel colorWheel = (ColorWheel)hsvGrid.Children[0];
            Color currentColor = colorWheel.GetColor(pt);
            Brush brush = new SolidColorBrush(currentColor);
            string colorText = currentColor.ToString();
            if((bool)rdbStart.IsChecked)
            {
                txtStartColor.Text = colorText;
                gridStartFill.Background = brush;
            }
            else
            {
                txtEndColor.Text = colorText;
                gridEndFill.Background = brush;
            }
        }

        void hsvGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        void hsvGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Cross;
        }

        public void OnNavigateTo(Stage stage)
        {
            this.Visibility = stage == m_CurStage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }


        private void CheckSettings()
        {
            if( Settings.Instance.CloneCnt <= 0)
                throw new Exception("克隆数量必须大于0！");
            if( Settings.Instance.MinArea <=0 )
                throw new Exception("最小面积必须大于0！");
            if( Settings.Instance.MaxArea <=0 )
                throw new Exception("最大面积必须大于0！");
            if( Settings.Instance.MaxArea <= Settings.Instance.MinArea)
                throw new Exception("最大面积必须大于最小面积！");
            
        }

        private void SetInfo(string s, bool hasError = true)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = hasError ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckSettings();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message);
                return;
            }
            Settings.Instance.SelectionMethod = (bool)rdbMaxArea.IsChecked ? SelectionMethod.biggest : SelectionMethod.random;
            Settings.Instance.Save();
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
