using System;
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
		private bool _isDirty;

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

			var commandToExecute = _presenter.CommandToExecute(outlookTimePickerFrom.TimeValue(), outlookTimePickerTo.TimeValue(), checkBoxAdvNextDay.Checked, _dayCreator);

			if (commandToExecute == AgentStudentAvailabilityExecuteCommand.Remove)
			{
				var removeCommand = new AgentStudentAvailabilityRemoveCommand(_presenter.ScheduleDay);
				_presenter.Remove(removeCommand);
				_isDirty = true;
				Hide();
				return;	
			}

			if (!validateTimes()) return;

			if (commandToExecute == AgentStudentAvailabilityExecuteCommand.Add)
			{
				var addCommand = new AgentStudentAvailabilityAddCommand(_presenter.ScheduleDay, outlookTimePickerFrom.TimeValue(), outlookTimePickerTo.TimeValue(), checkBoxAdvNextDay.Checked, _dayCreator);
				_presenter.Add(addCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (commandToExecute == AgentStudentAvailabilityExecuteCommand.Edit)
			{
				var editCommand = new AgentStudentAvailabilityEditCommand(_presenter.ScheduleDay, outlookTimePickerFrom.TimeValue(), outlookTimePickerTo.TimeValue(), checkBoxAdvNextDay.Checked, _dayCreator);
				_presenter.Edit(editCommand);
				_isDirty = true;
				Hide();
				return;
			}

			_isDirty = false;
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

			if (!result && startTimeError != endTimeError)
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
			errorProvider1.SetIconPadding(timeControl, 2);
			errorProvider1.SetError(timeControl, value);
		}
	}
}
