using System;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.Time;

namespace Teleopti.Ccc.Win.WpfControls.Controls.Time.TimeZoom.Views
{
    /// <summary>
    /// Interaction logic for TimeZoomView.xaml
    /// </summary>
    public partial class TimeZoomView : UserControl
    {
        public TimeZoomView()
        {
            InitializeComponent();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TimeZoomViewModel model = this.DataContext as TimeZoomViewModel;
            if (model != null)
            {

                model.ScrollDateTime = ((DateTime) e.AddedItems[0]).ToUniversalTime();
            }
        }
    }
}
