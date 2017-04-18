using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class RecurrentMonthlyByWeekMeetingViewModel : RecurrentMeetingOptionViewModel
    {
        private readonly IRecurrentMonthlyByWeekMeeting _recurrentMonthlyByWeekMeeting;

        public RecurrentMonthlyByWeekMeetingViewModel(MeetingViewModel meetingViewModel, IRecurrentMonthlyByWeekMeeting recurrentMonthlyByWeekMeeting)
            : base(meetingViewModel, recurrentMonthlyByWeekMeeting)
        {
            _recurrentMonthlyByWeekMeeting = recurrentMonthlyByWeekMeeting;
        }

        public DayOfWeek DayOfWeek
        {
            get { return _recurrentMonthlyByWeekMeeting.DayOfWeek; }
            set
            {
                _recurrentMonthlyByWeekMeeting.DayOfWeek = value;
                NotifyPropertyChanged(nameof(DayOfWeek));
            }
        }

        public WeekNumber WeekOfMonth
        {
            get { return _recurrentMonthlyByWeekMeeting.WeekOfMonth; }
            set
            {
                _recurrentMonthlyByWeekMeeting.WeekOfMonth = value;
                NotifyPropertyChanged(nameof(WeekOfMonth));
            }
        }

        public override RecurrentMeetingOptionViewModel ChangeRecurringMeetingOption(RecurrentMeetingType recurrentMeetingType)
        {
            if (recurrentMeetingType == RecurrentMeetingType.MonthlyByWeek)
            {
                var recurrentMeeting = new RecurrentMonthlyByWeekMeeting();
                recurrentMeeting.IncrementCount = IncrementCount;
                recurrentMeeting.DayOfWeek = DayOfWeek;
                recurrentMeeting.WeekOfMonth = WeekOfMonth;
                return new RecurrentMonthlyByWeekMeetingViewModel(MeetingViewModel, recurrentMeeting);
            }
            return base.ChangeRecurringMeetingOption(recurrentMeetingType);
        }

        public override RecurrentMeetingType RecurrentMeetingType
        {
            get { return RecurrentMeetingType.MonthlyByWeek; }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}