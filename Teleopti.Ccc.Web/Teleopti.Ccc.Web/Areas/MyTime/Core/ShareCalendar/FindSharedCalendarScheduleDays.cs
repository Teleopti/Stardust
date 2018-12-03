using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class FindSharedCalendarScheduleDays : IFindSharedCalendarScheduleDays
    {
        private const int sharingStartDay = -60;
        private const int sharingEndDay = 180;
        
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly INow _now;

        public FindSharedCalendarScheduleDays(IRepositoryFactory repositoryFactory, INow now)
        {
            _repositoryFactory = repositoryFactory;
            _now = now;
        }

        public IEnumerable<PersonScheduleDayReadModel> GetScheduleDays(CalendarLinkId calendarLinkId, IUnitOfWork uow, DateTime? schedulePublishedToDate)
        {
            var endDate = _now.ServerDate_DontUse().AddDays(sharingEndDay);
            if (schedulePublishedToDate.HasValue)
            {
                var publishedToDate = new DateOnly(schedulePublishedToDate.Value);
                if (publishedToDate < endDate)
                    endDate = publishedToDate;
            }
            var personScheduleDayReadModelFinder = _repositoryFactory.CreatePersonScheduleDayReadModelFinder(uow);
            var scheduleDays = personScheduleDayReadModelFinder.ForPerson(new DateOnlyPeriod(_now.ServerDate_DontUse().AddDays(sharingStartDay),
                endDate),
                calendarLinkId.PersonId);
            return scheduleDays;
        }
    }
}