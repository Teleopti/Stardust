using System;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Scheduling;  

namespace Teleopti.Ccc.WpfControls.Controls.DateTimePeriodControl
{
    /// <summary>
    /// Interaction logic for EditableStartDateView.xaml
    /// </summary>
    public partial class EditableStartDateView : UserControl
    {
        public EditableStartDateView()
        {
            InitializeComponent();
        }

        private void DatePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Hmm this is some ugly fix for the datetime picker, when it suddenly writes a strange text in the field when you delete the whole date
            if (string.IsNullOrEmpty(startDatePicker.Text))
            {
                if (startDatePicker.SelectedDate != null)
                    startDatePicker.Text = startDatePicker.SelectedDate.Value.ToShortDateString();
            }
        }
    }
}
