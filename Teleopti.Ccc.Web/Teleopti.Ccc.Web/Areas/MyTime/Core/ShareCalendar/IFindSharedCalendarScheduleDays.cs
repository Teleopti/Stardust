using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface IFindSharedCalendarScheduleDays
    {
        IEnumerable<PersonScheduleDayReadModel> GetScheduleDays(CalendarLinkId calendarLinkId, IUnitOfWork uow, DateTime? schedulePublishedToDate);
    }
}