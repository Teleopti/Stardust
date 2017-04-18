using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class RecurrentWeeklyMeetingViewModel : RecurrentMeetingOptionViewModel
    {
        private readonly IRecurrentWeeklyMeeting _recurrentWeeklyMeeting;

        public RecurrentWeeklyMeetingViewModel(MeetingViewModel meetingViewModel, IRecurrentWeeklyMeeting recurrentWeeklyMeeting)
            : base(meetingViewModel, recurrentWeeklyMeeting)
        {
            _recurrentWeeklyMeeting = recurrentWeeklyMeeting;
        }

        public bool this[DayOfWeek dayOfWeek]
        {
            get { return _recurrentWeeklyMeeting[dayOfWeek]; }
            set
            {
                _recurrentWeeklyMeeting[dayOfWeek] = value;
                NotifyPropertyChanged(dayOfWeek.ToString());
            }
        }

        public override RecurrentMeetingOptionViewModel ChangeRecurringMeetingOption(RecurrentMeetingType recurrentMeetingType)
        {
            if (recurrentMeetingType == RecurrentMeetingType.Weekly)
            {
                var recurrentMeeting = new RecurrentWeeklyMeeting();
                recurrentMeeting.IncrementCount = IncrementCount;
                foreach (var dayOfWeek in _recurrentWeeklyMeeting.WeekDays)
                {
                    recurrentMeeting[dayOfWeek] = true;
                }
                return new RecurrentWeeklyMeetingViewModel(MeetingViewModel, recurrentMeeting);
            }
            return base.ChangeRecurringMeetingOption(recurrentMeetingType);
        }

        public override RecurrentMeetingType RecurrentMeetingType
        {
            get { return RecurrentMeetingType.Weekly; }
        }

        public override bool IsValid()
        {
            return _recurrentWeeklyMeeting.WeekDays.Count() > 0;
        }
    }
}