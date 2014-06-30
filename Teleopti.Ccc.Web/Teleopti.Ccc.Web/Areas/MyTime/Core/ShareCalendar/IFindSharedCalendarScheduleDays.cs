using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface IFindSharedCalendarScheduleDays
    {
        IEnumerable<PersonScheduleDayReadModel> GetScheduleDays(CalendarLinkId calendarLinkId, IUnitOfWork uow, DateTime? schedulePublishedToDate);
    }
}