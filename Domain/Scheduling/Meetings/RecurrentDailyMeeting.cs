using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
    public class RecurrentDailyMeeting : RecurrentMeetingOption, IRecurrentDailyMeeting
    {
        public override IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate)
        {
            IList<DateOnly> meetingDays = new List<DateOnly>();
            for (DateOnly currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(IncrementCount))
            {
                meetingDays.Add(currentDate);
            }
            return meetingDays;
        }
    }
}