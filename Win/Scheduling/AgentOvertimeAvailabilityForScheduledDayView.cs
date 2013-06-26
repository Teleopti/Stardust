﻿using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class AgentOvertimeAvailabilityForScheduledDayView : BaseRibbonForm, IAgentOvertimeAvailabilityView
	{
		private readonly IAgentOvertimeAvailabilityPresenter _presenter;
		private readonly IOvertimeAvailabilityCreator _dayCreator;
		private bool _isDirty;
		private TimePeriod _shiftTimePeriod;

		public AgentOvertimeAvailabilityForScheduledDayView(IScheduleDay scheduleDay)
		{
			InitializeComponent();
			SetTexts();
			_dayCreator = new OvertimeAvailabilityCreator();
			_presenter = new AgentOvertimeAvailabilityPresenter(this, scheduleDay);
			_presenter.Initialize();

			var shiftTimePeriod = scheduleDay.ProjectionService().CreateProjection().Period().GetValueOrDefault();
			_shiftTimePeriod = shiftTimePeriod.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
		}

		public IScheduleDay ScheduleDay
		{
			get{return _isDirty ? _presenter.ScheduleDay : null;}
		}

		public void ShowPreviousSavedOvertimeAvailability(string timePeriod)
		{
			labelPreviousSavedOvertimeAvailabilityColon.Visible = true;
			labelPreviousSavedOvertimeAvailability.Text = timePeriod;
			buttonAdvOk.Text = Resources.OverWrite;
		}

		public void Update(TimeSpan? startTime, TimeSpan? endTime)
		{
			outlookTimePickerFrom.SetTimeValue(startTime);
			labelShiftStartsAt.Text = OutlookTimePicker.TimeOfDayFromTimeSpan(_shiftTimePeriod.StartTime);

			if (endTime.HasValue && endTime.Value.Days > 0)
			{
				endTime = endTime.Value.Subtract(TimeSpan.FromDays(1));
				checkBoxAdvNextDay.Checked = true;
			}
			else
			{
				checkBoxAdvNextDay.Checked = false;
			}

			labelShiftEndsAt.Text = OutlookTimePicker.TimeOfDayFromTimeSpan(_shiftTimePeriod.EndTime);
			outlookTimePickerTo.SetTimeValue(endTime);
		}
		
		private void agentOvertimeAvailabilityViewLoad(object sender, EventArgs e)
		{
			_presenter.UpdateView();
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			clearTimeErrorMessages();

			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromDays(1));

			var commandToExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);

			if (commandToExecute == AgentOvertimeAvailabilityExecuteCommand.Remove)
			{
				var removeCommand = new AgentOvertimeAvailabilityRemoveCommand(_presenter.ScheduleDay);
				_presenter.Remove(removeCommand);
				_isDirty = true;
				Hide();
				return;	
			}

			if (!validateTimes()) return;

			if (commandToExecute == AgentOvertimeAvailabilityExecuteCommand.Add)
			{
				var addCommand = new AgentOvertimeAvailabilityAddCommand(_presenter.ScheduleDay, startTime, endTime, _dayCreator);
				addCommand.Initialize();
				_presenter.Add(addCommand);
				_isDirty = true;
				Hide();
				return;
			}

			if (commandToExecute == AgentOvertimeAvailabilityExecuteCommand.Edit)
			{
				var editCommand = new AgentOvertimeAvailabilityEditCommand(_presenter.ScheduleDay, startTime, endTime, _dayCreator);
				editCommand.Initialize();
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
			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromDays(1));

			bool startTimeError;
			bool endTimeError;
			var result = _dayCreator.CanCreate(startTime, endTime, _shiftTimePeriod.StartTime,_shiftTimePeriod.EndTime, out startTimeError, out endTimeError);

			if (!result)
			{
				if(startTimeError) setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
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

		private void outlookTimePickerFromTextChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerFrom)) ||
				!string.IsNullOrEmpty(errorProvider1.GetError(outlookTimePickerTo)))
			{
				clearTimeErrorMessages();
				validateTimes();
			}
				
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

		private void checkBoxAdvNextDayCheckedChanged(object sender, Syncfusion.Windows.Forms.Tools.CheckedChangedEventArgs e)
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
