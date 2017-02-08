using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizeOneday
	{
		bool Execute(DateOnly dateOnly);
	}
}