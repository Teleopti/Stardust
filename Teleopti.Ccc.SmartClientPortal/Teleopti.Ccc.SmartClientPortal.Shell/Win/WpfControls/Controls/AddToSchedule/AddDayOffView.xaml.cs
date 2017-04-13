using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.AddToSchedule
{
    /// <summary>
    /// Interaction logic for AddDayOffView.xaml
    /// </summary>
    public partial class AddDayOffView : UserControl
    {
        public AddDayOffView()
        {
            InitializeComponent();
        }

	    private void AddDayOffView_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    Keyboard.Focus(Payloads);
		    Payloads.Focus();
	    }
    }
}
