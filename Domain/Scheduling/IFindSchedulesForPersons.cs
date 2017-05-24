using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IFindSchedulesForPersons
	{
		IScheduleDictionary FindSchedulesForPersons(
			IScheduleDateTimePeriod period,
			IScenario scenario,
			IPersonProvider personsProvider,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);
	}
}