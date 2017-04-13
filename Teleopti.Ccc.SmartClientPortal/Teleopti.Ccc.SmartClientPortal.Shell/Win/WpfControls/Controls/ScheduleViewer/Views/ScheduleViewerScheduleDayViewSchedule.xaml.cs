using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Win.WpfControls.Controls.ScheduleViewer.Models;

namespace Teleopti.Ccc.Win.WpfControls.Controls.ScheduleViewer.Views
{
    /// <summary>
    /// Interaction logic for ScheduleViewerScheduleDayViewSchedule.xaml
    /// </summary>
    public partial class ScheduleViewerScheduleDayViewSchedule : UserControl
    {
        public ScheduleViewerScheduleDayViewSchedule()
        {
            InitializeComponent();
        }

        //remove this, just because I'm lazy, no need to do this......
        private void Reload_Clicked(object sender, RoutedEventArgs e)
        {
            ScheduleViewerScheduleDayViewModel model = this.DataContext as ScheduleViewerScheduleDayViewModel;
            if (model != null)
            {
                LW1.ItemsSource = model.Layers;
            }
        }
    }
}
