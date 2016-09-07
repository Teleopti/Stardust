using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IScheduleDayProvider
	{
		IScheduleDictionary GetScheduleDictionary(DateOnly date, IPerson person);
		IScheduleDay GetScheduleDay(DateOnly date, IPerson person);
	}
}