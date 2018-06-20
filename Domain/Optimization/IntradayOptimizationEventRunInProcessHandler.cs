using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.ResourcePlanner;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventRunInSyncInFatClientProcessHandler: IRunInSyncInFatClientProcess, IHandleEvent<IntradayOptimizationWasOrdered>
	{
		private readonly IntradayOptimizationExecutor _intradayOptimizationExecutor;
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;

		public IntradayOptimizationEventRunInSyncInFatClientProcessHandler(
			IntradayOptimizationExecutor intradayOptimizationExecutor,
			IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod)
		{
			_intradayOptimizationExecutor = intradayOptimizationExecutor;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
		}

		public void Handle(IntradayOptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				_intradayOptimizationExecutor.HandleEvent(@event, _blockPreferenceProviderForPlanningPeriod.Fetch(@event.PlanningPeriodId));
			}
		}
	}
}