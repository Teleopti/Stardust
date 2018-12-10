using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class FilterOvertimeAvailabilityView : BaseDialogForm
	{
		private readonly FilterOvertimeAvailabilityPresenter _presenter;

		public FilterOvertimeAvailabilityView(DateOnly defaultDate, SchedulingScreenState schedulerStateHolder)
		{
			InitializeComponent();
			outlookTimePickerFrom.EnableNull = false;
			outlookTimePickerTo.EnableNull = false;
			datePicker.Value = defaultDate.Date;
			datePicker.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			SetTexts();

			_presenter = new FilterOvertimeAvailabilityPresenter(schedulerStateHolder);
		}

		private void filterOvertimeAvailabilityViewLoad(object sender, EventArgs e)
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

		private void buttonOkClick(object sender, EventArgs e)
		{
			if (!validateTimes()) return;

			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromDays(1));
			_presenter.Filter(new TimePeriod(startTime.Value, endTime.Value), new DateOnly(datePicker.Value), radioButtonPart.Checked);
			DialogResult = DialogResult.OK;
			Close();
		}

		private void buttonCancelClick(object sender, EventArgs e)
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

		private void outlookTimePickerToTextChanged(object sender, EventArgs e)
		{

			if (!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) ||
				!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo)))
			{
				clearTimeErrorMessages();
				validateTimes();
			}
		}

		private void outlookTimePickerFromTextChanged(object sender, EventArgs e)
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
