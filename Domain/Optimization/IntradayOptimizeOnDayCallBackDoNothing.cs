using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizeOnDayCallBackDoNothing : IIntradayOptimizeOneDayCallback
	{
		public void Optimizing(IPerson person, DateOnly dateOnly)
		{
		}
	}
}