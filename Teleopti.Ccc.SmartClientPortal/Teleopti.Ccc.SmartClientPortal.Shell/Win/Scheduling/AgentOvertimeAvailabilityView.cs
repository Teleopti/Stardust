using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class AgentOvertimeAvailabilityView : BaseDialogForm, IAgentOvertimeAvailabilityView
	{
		private readonly AgentOvertimeAvailabilityPresenter _presenter;
		private readonly IOvertimeAvailabilityCreator _dayCreator;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

	    public AgentOvertimeAvailabilityView(IScheduleDay scheduleDay, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
	        _schedulingResultStateHolder = schedulingResultStateHolder;
	        InitializeComponent();
			SetTexts();
			_dayCreator = new OvertimeAvailabilityCreator();
			_presenter = new AgentOvertimeAvailabilityPresenter(this, scheduleDay, _schedulingResultStateHolder, scheduleDayChangeCallback);
			_presenter.Initialize();
		}

		public void Update(TimeSpan? startTime, TimeSpan? endTime)
		{
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

            if (!validateTimes()) return;

			var commandToExecute = _presenter.CommandToExecute(startTime, endTime, _dayCreator);
		    if (commandToExecute == null) return;
		    
            _presenter.RunCommand(commandToExecute);
		    Close();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		private bool validateTimes()
		{
			var startTime = outlookTimePickerFrom.TimeValue();
			var endTime = outlookTimePickerTo.TimeValue();
			if (checkBoxAdvNextDay.Checked && endTime.HasValue)
				endTime = endTime.Value.Add(TimeSpan.FromDays(1));

			bool startTimeError;
			bool endTimeError;
			var result = _dayCreator.CanCreate(startTime, endTime,  out startTimeError, out endTimeError);

			if (!result && startTimeError != endTimeError)
			{
				if (startTimeError) setTimeErrorMessage(outlookTimePickerFrom, Resources.MustSpecifyValidTime);
				if(endTimeError) setTimeErrorMessage(outlookTimePickerTo, Resources.MustSpecifyValidTime);
			}

			if (!result && startTimeError && endTimeError) return true;

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

		private void checkBoxAdvNextDayCheckedChanged(object sender, EventArgs e)
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
