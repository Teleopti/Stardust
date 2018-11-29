using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class MeetingRecurrencePresenter
    {
        private readonly IMeetingRecurrenceView _view;
        private readonly IMeetingViewModel _meetingViewModel;
        private RecurrentMeetingOptionViewModel _originalRecurrenceOption;
        private DateOnly _startDate;
        private DateOnly _recurrenceEndDate;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private RecurrentMeetingOptionViewModel _currentRecurrenceOption;

        public MeetingRecurrencePresenter(IMeetingRecurrenceView view, IMeetingViewModel meetingViewModel)
        {
            _view = view;
            _meetingViewModel = meetingViewModel;
        }

        public void Initialize()
        {
            _startDate = _meetingViewModel.StartDate;
            _recurrenceEndDate = _meetingViewModel.RecurringEndDate;
            _startTime = _meetingViewModel.StartTime;
            _endTime = _meetingViewModel.EndTime;
            _originalRecurrenceOption = _meetingViewModel.RecurringOption;

            RecurrentMeetingType recurrentMeetingType =
                _originalRecurrenceOption.RecurrentMeetingType;

            _view.SetStartTime(_startTime);
            _view.SetEndTime(_endTime);
            
            _view.SetStartDate(_startDate);
            _view.SetRecurringEndDate(_recurrenceEndDate);
            _view.SetRecurringOption(recurrentMeetingType);
            _view.SetRecurringExists(_recurrenceEndDate>_startDate);
        }

        public void SaveRecurrenceForMeeting()
        {
            if (ValidateControls())
            {
                _meetingViewModel.StartDate = _startDate;
                _meetingViewModel.StartTime = _startTime;
                _meetingViewModel.EndTime = _endTime;
                _meetingViewModel.RecurringEndDate = _recurrenceEndDate;
                _meetingViewModel.RecurringOption = _currentRecurrenceOption;
                _view.AcceptAndClose();
            }
            else
            {
                _view.ShowErrorMessage(
                    UserTexts.Resources.TheRecurrencePatternIsNotValid,
                    UserTexts.Resources.ErrorMessage);
            }
        }

        private bool ValidateControls()
        {
            if (!_currentRecurrenceOption.IsValid() ||
                _recurrenceEndDate < _startDate) 
                return false;

            return true;
        }

        public void RemoveRecurrence()
        {
            _meetingViewModel.RemoveRecurrence();
            _view.Close();
        }

        public void SetRecurringEndDate(DateOnly endDate)
        {
            _recurrenceEndDate = endDate;
        }

        public void ChangeRecurringType(RecurrentMeetingType recurrentMeetingType)
        {
            _currentRecurrenceOption =
                (_currentRecurrenceOption ?? _originalRecurrenceOption).ChangeRecurringMeetingOption(recurrentMeetingType);
            _view.RefreshRecurrenceOption(_currentRecurrenceOption);
        }

        public void SetStartDate(DateOnly startDate)
        {
            _startDate = startDate;
            if (_recurrenceEndDate<_startDate)
            {
                SetRecurringEndDate(_startDate);
                _view.SetRecurringEndDate(_recurrenceEndDate);
            }
        }

        public void SetStartTime(TimeSpan startTime)
        {
            _startTime = startTime;
            _view.SetStartTime(startTime);
        }

        public void SetEndTime(TimeSpan endTime)
        {
            _endTime = endTime;
            _view.SetEndTime(endTime);
        }

        public void SetMeetingDuration(TimeSpan duration)
        {
            if (duration >= TimeSpan.FromDays(1)) duration = TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(-1));

            _endTime = _startTime.Add(duration);
            if (_endTime > TimeSpan.FromDays(1)) _endTime = _endTime.Subtract(TimeSpan.FromDays(1));
            _view.SetEndTime(_endTime);
        }

		public TimeSpan GetStartTime
		{
			get { return _meetingViewModel.StartTime; }
		}

		public TimeSpan GetEndTime
		{
			get { return _meetingViewModel.EndTime; }
		}

		public TimeSpan GetDurationTime
		{
			get { return _meetingViewModel.MeetingDuration; }
		}

		public void OnOutlookTimePickerStartLeave(string inputText)
		{
			TimeSpan? timeSpan;

			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
                if (timeSpan.HasValue)
                {
                    if (timeSpan.Value >= TimeSpan.Zero)
                    {
                        SetStartTime(timeSpan.Value);
                    }
                    else
                        _view.SetStartTime(GetStartTime);
                }

			}
			else
				_view.SetStartTime(GetStartTime);
		}

		public void OnOutlookTimePickerEndLeave(string inputText)
		{
			TimeSpan? timeSpan;

			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
				if (timeSpan.HasValue)
				{
                    if (timeSpan.Value >= TimeSpan.Zero)
                        SetEndTime(timeSpan.Value);
                    else
                        SetMeetingDuration(GetDurationTime); 
				}
			}
			else
				SetMeetingDuration(GetDurationTime); 
		}

		public void OnOutlookTimePickerStartKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerStartLeave(inputText);
		}

		public void OnOutlookTimePickerEndKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerEndLeave(inputText);
		}

		public void OnOutlookTimePickerStartSelectedIndexChanged(string inputText)
		{
			OnOutlookTimePickerStartLeave(inputText);
		}

		public void OnOutlookTimePickerEndSelectedIndexChanged(string inputText)
		{
			OnOutlookTimePickerEndLeave(inputText);
		}

    }
}