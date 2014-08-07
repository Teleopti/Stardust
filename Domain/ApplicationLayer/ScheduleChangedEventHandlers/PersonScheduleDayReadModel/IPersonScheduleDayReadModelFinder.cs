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
		IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging);
		IEnumerable<PersonScheduleDayReadModel> ForPersonsByFilteredTimes(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, IEnumerable<TimePeriod> filteredStartTimes );
	}
}