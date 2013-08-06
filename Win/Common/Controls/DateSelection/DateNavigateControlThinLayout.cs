using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateNavigateControlThinLayout : BaseUserControl
    {
        public DateNavigateControlThinLayout()
        {
            InitializeComponent();
            dateTimePickerAdv1.SetCultureInfoSafe(CultureInfo.CurrentCulture);
        }

        public event EventHandler<CustomEventArgs<DateOnly>> SelectedDateChanged;
        
        private void ForwardDayButtonClicked(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(1d);
        }

        private void BackDayButtonClick(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-1d);
        }

        private void ForwardWeekButtonClick(object sender,EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(7d);
        }

        private void BackwardWeekButtonClick(object sender, EventArgs e)
        {
            dateTimePickerAdv1.Value = dateTimePickerAdv1.Value.AddDays(-7d);
        }

        private void DropDownDateSelected(object sender, EventArgs e)
        {
            InvokeDateChanged();
        }

        private void InvokeDateChanged()
        {
        	var handler = SelectedDateChanged;
            if (handler!=null)
            {
            	handler.Invoke(this, new CustomEventArgs<DateOnly>(new DateOnly(dateTimePickerAdv1.Value)));
            }
        }

        public void SetSelectedDate(DateOnly dateTime )
        {
            dateTimePickerAdv1.ValueChanged -= DropDownDateSelected;
            dateTimePickerAdv1.Value = dateTime.Date;
            dateTimePickerAdv1.ValueChanged += DropDownDateSelected;

            InvokeDateChanged();
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