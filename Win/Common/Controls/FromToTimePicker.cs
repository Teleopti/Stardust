using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public partial class FromToTimePicker : UserControl
    {
        public FromToTimePicker()
        {
            InitializeComponent();
        }

        public Office2007OutlookTimePicker StartTime
        {
            get { return office2007OutlookTimePickerStartTime; }
            set { office2007OutlookTimePickerStartTime = value; }
        }

        public Office2007OutlookTimePicker EndTime
        {
            get { return office2007OutlookTimePickerEndTime; }
            set { office2007OutlookTimePickerEndTime = value; }
        }

        public CheckBoxAdv WholeDay
        {
            get { return checkBoxAdvWholeDay; }
            set { checkBoxAdvWholeDay = value; }
        }

        [Localizable(true)]
        public string WholeDayText
        {
            get { return checkBoxAdvWholeDay.Text; }
            set { checkBoxAdvWholeDay.Text = value; }
        }
    }
}
