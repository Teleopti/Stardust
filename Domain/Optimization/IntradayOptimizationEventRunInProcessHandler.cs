using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventRunInProcessHandler: IntradayOptimizationEventBaseHandler, IRunInProcess
	{
		public IntradayOptimizationEventRunInProcessHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder, IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult, IGridlockManager gridlockManager,
			IFillStateHolderWithMaxSeatSkills fillStateHolderWithMaxSeatSkills)
			: base(
				intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeIntradayOptimizationResult,
				gridlockManager, fillStateHolderWithMaxSeatSkills)
		{
		}
	}
}