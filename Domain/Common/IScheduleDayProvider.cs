using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IScheduleDayProvider
	{
		IScheduleDictionary GetScheduleDictionary(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null);
		IScheduleDay GetScheduleDay(DateOnly date, IPerson person, ScheduleDictionaryLoadOptions loadOptions = null);
	}
}