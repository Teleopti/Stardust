using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.Win.WpfControls.Controls.AddToSchedule
{
    /// <summary>
    /// Interaction logic for AddPersonalActivityView.xaml
    /// </summary>
    public partial class AddPersonalActivityView : UserControl
    {
        public AddPersonalActivityView()
        {
            InitializeComponent();
        }

	    private void AddPersonalActivityView_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    Payloads.Focus();
		    Keyboard.Focus(Payloads);
	    }
    }
}
