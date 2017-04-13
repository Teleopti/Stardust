using System.Windows.Controls;

namespace Teleopti.Ccc.Win.WpfControls.Controls.DateTimePeriodControl
{
    /// <summary>
    /// Interaction logic for EditableDateView.xaml
    /// </summary>
    public partial class EditableEndDateView : UserControl
    {
        public EditableEndDateView()
        {
            InitializeComponent();
        }

        private void DatePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Hmm this is some ugly fix for the datetime picker, when it suddenly writes a strange text in the field when you delete the whole date
            if (string.IsNullOrEmpty(endDatePicker.Text))
            {
                if (endDatePicker.SelectedDate != null)
                    endDatePicker.Text = endDatePicker.SelectedDate.Value.ToShortDateString();
            }
        }
    }
}
