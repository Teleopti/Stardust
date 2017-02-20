using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FindSchedulesForPersons : ScheduleStorage, IFindSchedulesForPersons
	{
		public FindSchedulesForPersons(ICurrentUnitOfWork currentUnitOfWork, IRepositoryFactory repositoryFactory, IPersistableScheduleDataPermissionChecker dataPermissionChecker, IScheduleStorageRepositoryWrapper scheduleStorageRepositoryWrapper) 
			:base(currentUnitOfWork, repositoryFactory, dataPermissionChecker, scheduleStorageRepositoryWrapper)
		{
		}

		protected override void LoadScheduleForAll(IScenario scenario, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> selectedPersons)
		{
			var periodBasedOnSelectedPersons = new ScheduleDateTimePeriod(period.VisiblePeriod, selectedPersons);
			DoLoadScheduleForAll(scenario, scheduleDictionary, periodBasedOnSelectedPersons.LongLoadedDateOnlyPeriod(), scheduleDictionaryLoadOptions);
		}

		protected override void LoadSchedulesByPersons(IScenario scenario, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDictionary scheduleDictionary, IScheduleDateTimePeriod period, IEnumerable<IPerson> personsInOrganization, IEnumerable<IPerson> selectedPersons)
		{
			DoLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, period.LongLoadedDateOnlyPeriod(), selectedPersons);
			DoLoadSchedulesPerPersons(scenario, scheduleDictionaryLoadOptions, scheduleDictionary, period.LongVisibleDateOnlyPeriod(), personsInOrganization.Except(selectedPersons));
		}
	}
}