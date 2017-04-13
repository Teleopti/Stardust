using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.AddToSchedule
{
    /// <summary>
    /// Interaction logic for AddActivityView.xaml
    /// </summary>
    public partial class AddActivityView : UserControl
    {
        public AddActivityView()
        {
            InitializeComponent();
        }

	    private void AddActivityView_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    Payloads.Focus();
		    Keyboard.Focus(Payloads);
	    }
    }
}
