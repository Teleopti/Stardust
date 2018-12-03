using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelFinder
	{
		IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnlyPeriod period, Guid personId);

		PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId);

		bool IsInitialized();
		IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds);		
		IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging);
		IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null, string timeSortOrder = "");
		IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<PersonInfoForShiftTradeFilter> personInfos);
	}
}