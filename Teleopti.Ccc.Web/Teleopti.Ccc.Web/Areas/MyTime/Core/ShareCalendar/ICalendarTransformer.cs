using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface ICalendarTransformer
    {
        string Transform(IEnumerable<PersonScheduleDayReadModel> scheduleDays);
    }
}