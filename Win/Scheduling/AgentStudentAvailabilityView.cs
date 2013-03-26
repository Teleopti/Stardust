using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class AgentStudentAvailabilityView : BaseRibbonForm, IAgentStudentAvailabilityView
	{
		private readonly IAgentStudentAvailabilityPresenter _presenter;
		private readonly IAgentStudentAvailabilityDayCreator _dayCreator;
		private bool _isDirty = false;

		public AgentStudentAvailabilityView(IScheduleDay scheduleDay)
		{
			InitializeComponent();
			SetTexts();
			_dayCreator = new AgentStudentAvailabilityDayCreator();
			_presenter = new AgentStudentAvailabilityPresenter(this, scheduleDay);
		}

		public IScheduleDay ScheduleDay
		{
			get{return _isDirty ? _presenter.ScheduleDay : null;}
		}

		public void Update(TimeSpan? startTime, TimeSpan? endTime, bool endNextDay)
		{
			outlookTimePickerFrom.SetTimeValue(startTime);
			outlookTimePickerTo.SetTimeValue(endTime);
			checkBoxAdvNextDay.Checked = endNextDay;
		}

		private void agentStudentAvailabilityViewLoad(object sender, EventArgs e)
		{
			_presenter.UpdateView();
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			clearTimeErrorMessages();
			if (!validateTimes()) return;

			if (outlookTimePickerFrom.TimeValue() == null && outlookTimePickerTo.TimeValue() == null)
			{
				_presenter.Remove(new AgentStudentAvailabilityRemoveCommand(_presenter.ScheduleDay));
				_isDirty = true;
				Hide();
				return;
			}
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			_isDirty = false;
			Hide();
		}

		private bool validateTimes()
		{
			bool startTimeError;
			bool endTimeError;
			var result = _dayCreator.CanCreate(outlookTimePickerFrom.TimeValue(), outlookTimePickerTo.TimeValue(), checkBoxAdvNextDay.Checked, out startTimeError, out endTimeError);

			if (!result)
			{
				if (startTimeError) setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
				if(endTimeError) setTimeErrorMessage(outlookTimePickerTo, Resources.MustSpecifyValidTime);
			}

			return result;
		}


		private void clearTimeErrorMessages()
		{
			setTimeErrorMessage(outlookTimePickerFrom, null);
			setTimeErrorMessage(outlookTimePickerTo, null);
		}

		private void setTimeErrorMessage(Control timeControl, string value)
		{
			errorProvider1.SetIconPadding(timeControl, 3);
			errorProvider1.SetError(timeControl, value);
		}
	}
}
