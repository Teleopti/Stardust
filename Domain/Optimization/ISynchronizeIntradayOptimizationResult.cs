using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ISynchronizeIntradayOptimizationResult
	{
		void Execute(IScheduleDictionary modifiedScheduleDictionary);
	}
}