using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizationCallback
	{
		void Optimizing(IPerson agent, bool wasSuccessful, int numberOfOptimizers, int executedOptimizers);
		bool IsCancelled();
	}
}