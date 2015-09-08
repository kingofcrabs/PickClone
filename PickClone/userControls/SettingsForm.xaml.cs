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

namespace PickClone.userControls
{
    /// <summary>
    /// SettingsForm.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsForm : UserControl,IStage
    {
        Stage m_CurStage = Stage.Setting;
        Settings settings;
        public SettingsForm()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            settings = Settings.Load();
     
            rdbMaxArea.IsChecked = settings.SelectionMethod == SelectionMethod.biggest;
            rdbRandom.IsChecked = !rdbMaxArea.IsChecked;
            txtCnt.DataContext = settings;
        }

        public void OnNavigateTo(Stage stage)
        {
            this.Visibility = stage == m_CurStage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }


        private bool CheckSettings()
        {
            int cntConstrain;
            bool bok = int.TryParse(txtCnt.Text, out cntConstrain);
            if (!bok)
                SetInfo("克隆数量必须大于0");
            return bok;
        }

        private void SetInfo(string s, bool hasError = true)
        {
            txtInfo.Text = s;
            txtInfo.Foreground = hasError ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool bok = CheckSettings();
            if (!bok)
                return;
            //settings.cloneCnt = int.Parse(txtCnt.Text);
            settings.SelectionMethod = (bool)rdbMaxArea.IsChecked ? SelectionMethod.biggest : SelectionMethod.random;
            settings.Save();
            
        }
    }
}
