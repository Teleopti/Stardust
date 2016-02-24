using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ScheduleOptimizationTests
{
	public class TrackIntradayOptimizationCallback : IIntradayOptimizationCallback
	{
		public void Optimizing(IPerson agent, bool wasSuccessful, int numberOfOptimizers, int executedOptimizers)
		{
			throw new System.NotImplementedException();
		}

		public bool IsCancelled()
		{
			return false;
		}
	}
}