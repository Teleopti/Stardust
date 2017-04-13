using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public static class RecurrentMeetingOptionViewModelFactory
    {
        public static RecurrentMeetingOptionViewModel CreateRecurrentMeetingOptionViewModel(MeetingViewModel meetingViewModel, IRecurrentMeetingOption recurrentMeetingOption)
        {
            if (typeof (IRecurrentDailyMeeting).IsInstanceOfType(recurrentMeetingOption))
                return new RecurrentDailyMeetingViewModel(meetingViewModel,
                                                          (IRecurrentDailyMeeting) recurrentMeetingOption);
            if (typeof (IRecurrentWeeklyMeeting).IsInstanceOfType(recurrentMeetingOption))
                return new RecurrentWeeklyMeetingViewModel(meetingViewModel,
                                                           (IRecurrentWeeklyMeeting) recurrentMeetingOption);
            if (typeof (IRecurrentMonthlyByDayMeeting).IsInstanceOfType(recurrentMeetingOption))
                return new RecurrentMonthlyByDayMeetingViewModel(meetingViewModel,
                                                                 (IRecurrentMonthlyByDayMeeting)
                                                                 recurrentMeetingOption);
            if (typeof (IRecurrentMonthlyByWeekMeeting).IsInstanceOfType(recurrentMeetingOption))
                return new RecurrentMonthlyByWeekMeetingViewModel(meetingViewModel,
                                                                  (IRecurrentMonthlyByWeekMeeting)
                                                                  recurrentMeetingOption);

            return null;
        }
    }
}
