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
            this.Closed += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Settings.Instance.Save();
            Pipeserver.Close();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lstSteps.DataContext = stepViewModel.StepsModel;
            var acquireImageForm = new AcquireImageForm();
            var settingsForm = new SettingsForm();
            acquireImageForm.Visibility = System.Windows.Visibility.Visible;
            settingsForm.Visibility = Visibility.Hidden;
            subForms.Add(acquireImageForm);
            subForms.Add(settingsForm);
            foreach(var subForm in subForms)
                userControlHost.Children.Add(subForm);
            lstSteps.SelectedIndex = 0;
            lstSteps.Focus();
        }

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
