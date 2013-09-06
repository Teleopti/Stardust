using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class FilterHourlyAvailabilityView : BaseDialogForm
    {
	    private readonly FilterHourlyAvailabilityPresenter _presenter;
	    public FilterHourlyAvailabilityView(DateOnly defaultDate, ISchedulerStateHolder schedulerStateHolder)
        {
	        InitializeComponent();
			tableLayoutPanel2.BackColor = Color.FromArgb(191, 219, 254);
			datePicker.Value = defaultDate;
			datePicker.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			SetTexts();
            _presenter = new FilterHourlyAvailabilityPresenter(schedulerStateHolder);
        }

		private void FilterOvertimeAvailabilityView_Load(object sender, EventArgs e)
		{
			var timeOfDay = DateTime.Now.TimeOfDay;
			var nextFullHour = TimeSpan.FromHours(Math.Ceiling(timeOfDay.TotalHours));
			outlookTimePickerFrom.SetTimeValue(nextFullHour);
			outlookTimePickerTo.SetTimeValue(nextFullHour.Add(TimeSpan.FromHours(1)));
		}

		public DateOnly SelectedDate
		{
			get { return new DateOnly(datePicker.Value); }
		}

	    private void buttonOk_Click(object sender, EventArgs e)
	    {
		    if (!validateTimes()) return;

		    var startTime = outlookTimePickerFrom.TimeValue();
		    var endTime = outlookTimePickerTo.TimeValue();
		    if (checkBoxAdvNextDay.Checked && endTime.HasValue)
			    endTime = endTime.Value.Add(TimeSpan.FromDays(1));
            _presenter.Filter(startTime.Value, endTime.Value, new DateOnly(datePicker.Value));
			DialogResult = DialogResult.OK;
		    Close();
	    }

	    private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void clearTimeErrorMessages()
		{
			setTimeErrorMessage(outlookTimePickerFrom, null);
			setTimeErrorMessage(outlookTimePickerTo, null);
		}

		private void setTimeErrorMessage(Control timeControl, string value)
		{
			errorProvider1.SetIconPadding(timeControl, 2);
			errorProvider1.SetError(timeControl, value);
		}

		private bool validateTimes()
		{
			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromDays(1));
			if (startTime == null)
			{
				setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
				return false;
			}
			if (endTime == null)
			{
				setTimeErrorMessage(outlookTimePickerTo, Resources.MustSpecifyValidTime);
				return false;
			}
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromHours(1));

			if (startTime > endTime)
			{
				setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
				return false;
			}

			return true;
		}

		private void outlookTimePickerTo_TextChanged(object sender, EventArgs e)
		{

			if (!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) ||
				!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo)))
			{
				clearTimeErrorMessages();
				validateTimes();
			}
		}

		private void outlookTimePickerFrom_TextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) ||
				!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo)))
			{
				clearTimeErrorMessages();
				validateTimes();
			}
		}
    }
}
