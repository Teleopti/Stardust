using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208)]
	public class DayOffOptimizationDirectCallCommandHandler : IDayOffOptimizationCommandHandler
	{
		private readonly DayOffOptimization _dayOffOptimization;

		public DayOffOptimizationDirectCallCommandHandler(DayOffOptimization dayOffOptimization)
		{
			_dayOffOptimization = dayOffOptimization;
		}
		
		public void Execute(DayOffOptimizationCommand command, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			ISchedulingProgress schedulingProgress,
			Guid planningPeriodId,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			//temp - move to eventhandler
			using (CommandScope.Create(command))
			{
				_dayOffOptimization.Execute(command.Period, 
					command.AgentsToOptimize, 
					dayOffOptimizationPreferenceProvider, 
					schedulingProgress,
					command.RunWeeklyRestSolver, 
					planningPeriodId,
					resourceOptimizerPersonOptimized);
			}			
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_DayOffOptimizationIslands_47208)]
	public interface IDayOffOptimizationCommandHandler
	{
		void Execute(DayOffOptimizationCommand command,
			//these must be removed!
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			ISchedulingProgress schedulingProgress, Guid planningPeriodId,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized);
	}
}