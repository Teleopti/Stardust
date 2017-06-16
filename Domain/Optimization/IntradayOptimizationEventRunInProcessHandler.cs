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
		public IntradayOptimizationEventRunInSyncInFatClientProcessHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder, IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult, IGridlockManager gridlockManager)
			: base(intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeIntradayOptimizationResult,gridlockManager)
		{
		}

		public void Handle(IntradayOptimizationWasOrdered @event)
		{
			HandleEvent(@event);
		}
	}
}