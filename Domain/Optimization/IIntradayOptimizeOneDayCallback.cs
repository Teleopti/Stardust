using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizeOneDayCallback
	{
		void Optimizing(IPerson person, DateOnly dateOnly);
	}
}