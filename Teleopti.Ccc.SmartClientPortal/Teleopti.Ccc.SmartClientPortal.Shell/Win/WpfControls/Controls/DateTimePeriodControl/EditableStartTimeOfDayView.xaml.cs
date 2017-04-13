using System.Windows.Controls;
using System.Windows.Input;

namespace Teleopti.Ccc.Win.WpfControls.Controls.DateTimePeriodControl
{
    /// <summary>
    /// Interaction logic for EditableStartTimeOfDay.xaml
    /// </summary>
    public partial class EditableStartTimeOfDayView : UserControl
    {
        public EditableStartTimeOfDayView()
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
