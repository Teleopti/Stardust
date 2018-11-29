using System;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public partial class FromToTimePicker : UserControl
    {
        public FromToTimePicker()
        {
            InitializeComponent();
        }

	    public bool WholeDayCheckboxVisible
	    {
		    get { return checkBoxAdvWholeDay.Visible; }
		    set { checkBoxAdvWholeDay.Visible = value; }
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

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
        public CheckBoxAdv WholeDay
        {
            get { return checkBoxAdvWholeDay; }
            set { checkBoxAdvWholeDay = value; }
        }

        public MinMax<TimeSpan> MinMaxStartTime
        {
            get
            {
                return new MinMax<TimeSpan>(office2007OutlookTimePickerStartTime.MinValue,
                                            office2007OutlookTimePickerStartTime.MaxValue);
            }
            set
            {
                office2007OutlookTimePickerStartTime.MinValue = value.Minimum;
                office2007OutlookTimePickerStartTime.MaxValue = value.Maximum;
            }
        }

        public MinMax<TimeSpan> MinMaxEndTime
        {
            get
            {
                return new MinMax<TimeSpan>(office2007OutlookTimePickerEndTime.MinValue,
                                            office2007OutlookTimePickerEndTime.MaxValue);
            }
            set
            {
                office2007OutlookTimePickerEndTime.MinValue = value.Minimum;
                office2007OutlookTimePickerEndTime.MaxValue = value.Maximum;
            }
        }

        [Localizable(true)]
        public string WholeDayText
        {
            get { return checkBoxAdvWholeDay.Text; }
            set { checkBoxAdvWholeDay.Text = value; }
        }
    }
}
