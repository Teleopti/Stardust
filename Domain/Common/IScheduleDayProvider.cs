using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IScheduleDayProvider
	{
		IScheduleDictionary GetScheduleDictionary(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null);
		IScheduleDictionary GetScheduleDictionary(DateOnlyPeriod period, IEnumerable<IPerson> persons, ScheduleDictionaryLoadOptions loadOptions = null);
		IScheduleDictionary GetScheduleDictionary(DateOnly date, IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions loadOptions = null);
		IScheduleDay GetScheduleDay(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null);
		IEnumerable<IScheduleDay> GetScheduleDays(DateOnlyPeriod period, IEnumerable<IPerson> persons, IScenario scenario);


	}
}