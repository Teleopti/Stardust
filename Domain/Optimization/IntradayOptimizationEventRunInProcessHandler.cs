using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;

namespace Teleopti.Ccc.Domain.Optimization
{
	[InstancePerLifetimeScope]
	public class IntradayOptimizationEventRunInSyncInFatClientProcessHandler: IRunInSyncInFatClientProcess, IHandleEvent<IntradayOptimizationWasOrdered>
	{
		private readonly IntradayOptimizationExecutor _intradayOptimizationExecutor;

		public IntradayOptimizationEventRunInSyncInFatClientProcessHandler(
			IntradayOptimizationExecutor intradayOptimizationExecutor)
		{
			_intradayOptimizationExecutor = intradayOptimizationExecutor;
		}

		public void Handle(IntradayOptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				_intradayOptimizationExecutor.HandleEvent(@event);
			}
		}
	}
}