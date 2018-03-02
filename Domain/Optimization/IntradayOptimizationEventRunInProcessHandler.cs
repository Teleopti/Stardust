using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventRunInSyncInFatClientProcessHandler: IRunInSyncInFatClientProcess, IHandleEvent<IntradayOptimizationWasOrdered>
	{
		private readonly IntradayOptimizationExecutor _intradayOptimizationExecutor;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;

		public IntradayOptimizationEventRunInSyncInFatClientProcessHandler(
			IntradayOptimizationExecutor intradayOptimizationExecutor,
			IOptimizationPreferencesProvider optimizationPreferencesProvider)
		{
			_intradayOptimizationExecutor = intradayOptimizationExecutor;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
		}

		public void Handle(IntradayOptimizationWasOrdered @event)
		{
			using (CommandScope.Create(@event))
			{
				_intradayOptimizationExecutor.HandleEvent(new FixedBlockPreferenceProvider(_optimizationPreferencesProvider.Fetch().Extra), @event, null);
			}
		}
	}
}