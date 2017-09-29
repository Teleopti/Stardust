using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventRunInSyncInFatClientProcessHandler: IntradayOptimizationEventBaseHandler, IRunInSyncInFatClientProcess, IHandleEvent<IntradayOptimizationWasOrdered>
	{
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;

		public IntradayOptimizationEventRunInSyncInFatClientProcessHandler(IIntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder, IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland, IGridlockManager gridlockManager,
			IOptimizationPreferencesProvider optimizationPreferencesProvider)
			: base(intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeSchedulesAfterIsland,gridlockManager)
		{
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
		}

		public void Handle(IntradayOptimizationWasOrdered @event)
		{
			HandleEvent(@event);
		}

		protected override IBlockPreferenceProvider GetBlockPreferenceProvider(Guid? planningPeriodId)
		{
			return new FixedBlockPreferenceProvider(_optimizationPreferencesProvider.Fetch().Extra);
		}
	}
}