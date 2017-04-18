using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class RecurrentDailyMeetingViewModel : RecurrentMeetingOptionViewModel
    {
        public RecurrentDailyMeetingViewModel(MeetingViewModel meetingViewModel, IRecurrentDailyMeeting recurrentDailyMeeting)
            : base(meetingViewModel, recurrentDailyMeeting)
        {
        }

        public override RecurrentMeetingOptionViewModel ChangeRecurringMeetingOption(RecurrentMeetingType recurrentMeetingType)
        {
            if (recurrentMeetingType==RecurrentMeetingType.Daily)
            {
                var recurrentMeeting = new RecurrentDailyMeeting();
                recurrentMeeting.IncrementCount = IncrementCount;
                return new RecurrentDailyMeetingViewModel(MeetingViewModel, recurrentMeeting);
            }
            return base.ChangeRecurringMeetingOption(recurrentMeetingType);
        }

        public override RecurrentMeetingType RecurrentMeetingType
        {
            get { return RecurrentMeetingType.Daily; }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}