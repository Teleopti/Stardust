﻿using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationEventRunInProcessHandler: IntradayOptimizationEventBaseHandler, IRunInProcess, IHandleEvent<OptimizationWasOrdered>
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

		public void Handle(OptimizationWasOrdered @event)
		{
			HandleEvent(@event);
		}
	}
}