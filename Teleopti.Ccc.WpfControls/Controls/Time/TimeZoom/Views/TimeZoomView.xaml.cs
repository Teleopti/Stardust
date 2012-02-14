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
using Teleopti.Ccc.WinCode.Common.Time;

namespace Teleopti.Ccc.WpfControls.Controls.Time.TimeZoom.Views
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
