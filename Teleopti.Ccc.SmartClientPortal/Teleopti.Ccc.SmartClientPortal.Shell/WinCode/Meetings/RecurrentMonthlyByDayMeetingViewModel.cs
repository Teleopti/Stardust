using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class RecurrentMonthlyByDayMeetingViewModel : RecurrentMeetingOptionViewModel
    {
        private readonly IRecurrentMonthlyByDayMeeting _recurrentMonthlyByDayMeeting;

        public RecurrentMonthlyByDayMeetingViewModel(MeetingViewModel meetingViewModel, IRecurrentMonthlyByDayMeeting recurrentMonthlyByDayMeeting)
            : base(meetingViewModel, recurrentMonthlyByDayMeeting)
        {
            _recurrentMonthlyByDayMeeting = recurrentMonthlyByDayMeeting;
        }

        public int DayInMonth
        {
            get { return _recurrentMonthlyByDayMeeting.DayInMonth; }
            set
            {
                _recurrentMonthlyByDayMeeting.DayInMonth = value;
                NotifyPropertyChanged(nameof(DayInMonth));
            }
        }

        public override RecurrentMeetingOptionViewModel ChangeRecurringMeetingOption(RecurrentMeetingType recurrentMeetingType)
        {
            if (recurrentMeetingType == RecurrentMeetingType.MonthlyByDay)
            {
                var recurrentMeeting = new RecurrentMonthlyByDayMeeting();
                recurrentMeeting.IncrementCount = IncrementCount;
                recurrentMeeting.DayInMonth = DayInMonth;
                return new RecurrentMonthlyByDayMeetingViewModel(MeetingViewModel, recurrentMeeting);
            }
            return base.ChangeRecurringMeetingOption(recurrentMeetingType);
        }

        public override RecurrentMeetingType RecurrentMeetingType
        {
            get { return RecurrentMeetingType.MonthlyByDay; }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}