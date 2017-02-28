using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IFindSchedulesForPersons
	{
		IScheduleDictionary FindSchedulesForPersons(
			IScheduleDateTimePeriod period,
			IScenario scenario,
			IPersonProvider personsProvider,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);
	}
}