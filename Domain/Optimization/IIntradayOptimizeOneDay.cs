using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizeOneDay
	{
		bool Execute(DateOnly dateOnly);
	}
}