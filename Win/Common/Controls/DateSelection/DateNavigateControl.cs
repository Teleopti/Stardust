using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    public partial class DateNavigateControl : BaseUserControl
    {
        public DateNavigateControl()
        {
            InitializeComponent();
	        SetCulture(CultureInfo.CurrentCulture);
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

        public void SetAvailableTimeSpan(DateOnlyPeriod dateTimePair)
        {
            dateTimePickerAdv1.SetAvailableTimeSpan(dateTimePair);
            dateTimePickerAdv1.Value = dateTimePickerAdv1.MinValue;
        }

        public void SetSelectedDateNoInvoke(DateOnly dateTime)
        {
            dateTimePickerAdv1.ValueChanged -= DropDownDateSelected;
            dateTimePickerAdv1.Value = dateTime.Date;
            dateTimePickerAdv1.ValueChanged += DropDownDateSelected;
        }

        public void SetSelectedDate(DateOnly dateTime )
        {
            dateTimePickerAdv1.ValueChanged -= DropDownDateSelected;
            dateTimePickerAdv1.Value = dateTime.Date;
            dateTimePickerAdv1.ValueChanged += DropDownDateSelected;

            InvokeDateChanged();
        }

		public void SetCulture(CultureInfo cultureInfo)
		{
			dateTimePickerAdv1.SetCultureInfoSafe(cultureInfo);
		}

        public DateOnly SelectedDate
        {
            get { return new DateOnly(dateTimePickerAdv1.Value); }
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