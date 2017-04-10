using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.Win.WpfControls.Controls.AddToSchedule
{
    /// <summary>
    /// Interaction logic for AddOvertimeView.xaml
    /// </summary>
    public partial class AddOvertimeView : UserControl
    {
        public AddOvertimeView()
        {
            InitializeComponent();
        }

	    private void AddOvertimeView_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    Payloads.Focus();
		    Keyboard.Focus(Payloads);
	    }
    }
}
