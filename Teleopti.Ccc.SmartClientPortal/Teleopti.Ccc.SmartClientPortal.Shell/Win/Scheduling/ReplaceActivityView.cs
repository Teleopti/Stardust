using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class ReplaceActivityView : BaseDialogForm
	{
		public TimeSpan FromTimeSpan { get; private set; }
		public TimeSpan ToTimeSpan { get; private set; }
		public IActivity Activity { get; private set; }
		public IActivity ReplaceWithActivity { get; private set; }

	    public ReplaceActivityView(IList<IActivity> activities, TimePeriod defaultTimePeriod)
		{
	        InitializeComponent();
			SetTexts();
			update(defaultTimePeriod.StartTime, defaultTimePeriod.EndTime, activities);
		}

		private void update(TimeSpan? startTime, TimeSpan? endTime, IList<IActivity> activities)
		{
			comboBox1.DataSource = activities.Where(x => !x.IsDeleted).ToList();
			comboBox1.DisplayMember = "Name";
			comboBox2.DataSource = activities.Where(x => !x.IsDeleted).ToList();
			comboBox2.DisplayMember = "Name";
			outlookTimePickerFrom.SetTimeValue(startTime);
			if (endTime.HasValue && endTime.Value.Days > 0)
			{
				endTime = endTime.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvNextDay.Checked = true;
			}
			else
			{
				checkBoxAdvNextDay.Checked = false;
			}

			outlookTimePickerTo.SetTimeValue(endTime);
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			clearTimeErrorMessages();
			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked) endTime = endTime?.Add(TimeSpan.FromDays(1));
            if (!validateTimes()) return;
			if(startTime.HasValue)FromTimeSpan = startTime.Value;
			if(endTime.HasValue)ToTimeSpan = endTime.Value;
			Activity = (Activity) comboBox1.SelectedItem;
			ReplaceWithActivity = (Activity) comboBox2.SelectedItem;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private bool validateTimes()
		{
			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked)endTime = endTime?.Add(TimeSpan.FromDays(1));
			if (startTime == null && endTime == null)
			{
				setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
				setTimeErrorMessage(outlookTimePickerTo, Resources.MustSpecifyValidTime);
				return false;
			}

			if (startTime != null && endTime != null)
			{
				if (startTime.Value >= endTime.Value)
				{
					setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
					return false;
				}
			}

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

			clearTimeErrorMessages();
			return true;
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

		private void outlookTimePickerFromTextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) && string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo))) return;
			clearTimeErrorMessages();
			validateTimes();
		}

		private void outlookTimePickerToTextChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) && string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo))) return;
			clearTimeErrorMessages();
			validateTimes();
		}

		private void checkBoxAdvNextDayCheckedChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) && string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo))) return;
			clearTimeErrorMessages();
			validateTimes();
		}

		private void comboBox1_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
		}

		private void comboBox2_KeyDown(object sender, KeyEventArgs e)
		{
			e.SuppressKeyPress = true;
		}
	}
}
