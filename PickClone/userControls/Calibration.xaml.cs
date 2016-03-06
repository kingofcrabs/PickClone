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
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class CalibrationForm : UserControl
    {
        public CalibrationForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool bok = CheckSettings();
            if (!bok)
                return;

            Settings settings = new Settings();
            settings.cloneCnt = int.Parse(txtCnt.Text);
            settings.selectionMethod = (bool)rdbMaxArea.IsChecked ? SelectionMethod.biggest : SelectionMethod.random;
            string sFile = FolderHelper.GetConfigFolder() + "settings.xml";
            settings.Save(sFile);

            SetInfo(string.Format("配置文件已被保存到：{0}", sFile), false);
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

    }
}
