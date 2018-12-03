using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection
{
    public partial class DateNavigateControlThinLayout : BaseUserControl
    {
        public DateNavigateControlThinLayout()
        {
            InitializeComponent();
            dateTimePickerAdv1.SetCultureInfoSafe(CultureInfo.CurrentCulture);
        }

        public event EventHandler<CustomEventArgs<DateOnly>> SelectedDateChanged;
        
        private void forwardDayButtonClicked(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(1d);
        }

        private void backDayButtonClick(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-1d);
        }

        private void forwardWeekButtonClick(object sender,EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(7d);
        }

        private void backwardWeekButtonClick(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-7d);
        }

        private void dropDownDateSelected(object sender, EventArgs e)
        {
            invokeDateChanged();
        }

        private void invokeDateChanged()
        {
        	var handler = SelectedDateChanged;
            if (handler!=null)
            {
            	handler.Invoke(this, new CustomEventArgs<DateOnly>(new DateOnly(dateTimePickerAdv1.Value)));
            }
        }

        public void SetSelectedDate(DateOnly dateTime )
        {
            dateTimePickerAdv1.ValueChanged -= dropDownDateSelected;
            dateTimePickerAdv1.Value = dateTime.Date;
            dateTimePickerAdv1.ValueChanged += dropDownDateSelected;

            invokeDateChanged();
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }
    }
}