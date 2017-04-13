using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.DateTimePeriodControl
{
    /// <summary>
    /// Interaction logic for TimeBoxView.xaml
    /// </summary>
    public partial class EditableEndTimeOfDayView : UserControl
    {
        public EditableEndTimeOfDayView()
        {
            InitializeComponent();
            
        }

        //OK for now, just selects all text when tabbed into the textbox
        private void textBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OldFocus != e.NewFocus)
            {
                TextBox target = e.NewFocus as TextBox;
                if (target != null) target.SelectAll();
            }
        }
    }
}
