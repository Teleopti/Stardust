using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_Deadlock_48170)]
	public class IntradayOptimizationExecutorWithDeadlockRetries : IntradayOptimizationExecutor
	{
		private const int maxNumberOfTries = 3;
		private static readonly ILog log = LogManager.GetLogger(typeof(IntradayOptimizationExecutorWithDeadlockRetries));
		
		public IntradayOptimizationExecutorWithDeadlockRetries(IntradayOptimization intradayOptimization, Func<ISchedulerStateHolder> schedulerStateHolder, FillSchedulerStateHolder fillSchedulerStateHolder, ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland, IGridlockManager gridlockManager) : base(intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeSchedulesAfterIsland, gridlockManager)
		{
		}

		//if below is needed on more places - make an attribute/"something" instead?
		[TestLog]
		public override void HandleEvent(IBlockPreferenceProvider blockPreferenceProvider, IntradayOptimizationWasOrdered @event, Guid? planningPeriodId)
		{
			var numberOfTries = 0;
			while (true)
			{
				try
				{
					numberOfTries++;
					base.HandleEvent(blockPreferenceProvider, @event, planningPeriodId);
					return;
				}
				catch (DeadLockVictimException deadLockEx)
				{
					if (numberOfTries < maxNumberOfTries)
					{
						log.Warn($"Deadlock during intraday optimization. Attempt {numberOfTries} - retrying... {deadLockEx}");	
					}
					else
					{
						log.Warn($"Deadlock during intraday optimization. Attempt {numberOfTries} - giving up... {deadLockEx}");
						throw;
					}
				}
			}
		}
	}
}