using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FindSchedulesForPersons : ScheduleStorage, IFindSchedulesForPersons
	{
		public FindSchedulesForPersons(ICurrentUnitOfWork currentUnitOfWork, IRepositoryFactory repositoryFactory, IPersistableScheduleDataPermissionChecker dataPermissionChecker, IScheduleStorageRepositoryWrapper scheduleStorageRepositoryWrapper) 
			:base(currentUnitOfWork, repositoryFactory, dataPermissionChecker, scheduleStorageRepositoryWrapper)
		{
		}

		protected override void LoadScheduleForAll(IScenario scenario, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> selectedPersons)
		{
			var periodBasedOnSelectedPersons = new ScheduleDateTimePeriod(period.VisiblePeriod, selectedPersons);
			DoLoadScheduleForAll(scenario, scheduleDictionary, periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), scheduleDictionaryLoadOptions);
		}

		protected override void LoadSchedulesByPersons(IScenario scenario, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> personsInOrganization, IEnumerable<IPerson> selectedPersons)
		{
			var periodBasedOnSelectedPersons = new ScheduleDateTimePeriod(period.VisiblePeriod, selectedPersons);
			DoLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), selectedPersons);
			DoLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, periodBasedOnSelectedPersons.LongVisibleDateOnlyPeriod(), personsInOrganization.Except(selectedPersons));
		}
	}
}