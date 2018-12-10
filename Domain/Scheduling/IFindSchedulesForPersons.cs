using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IFindSchedulesForPersons
	{
		IScheduleDictionary FindSchedulesForPersons(IScenario scenario, IEnumerable<IPerson> personsInOrganization, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod visiblePeriod, IEnumerable<IPerson> visiblePersons, bool extendPeriodBasedOnVisiblePersons);
	}
}