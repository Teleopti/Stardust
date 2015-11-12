using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelFinder
	{
		IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId);

		IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds);
		IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTime scheduleDate, DateTimePeriod period, IEnumerable<Guid> personIds, Paging paging);

		IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging);
		IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null, string timeSortOrder = "");
	}
}