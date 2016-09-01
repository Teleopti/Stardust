using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class DoFullResourceOptimizationOneTime
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;

		public DoFullResourceOptimizationOneTime(Func<ISchedulingResultStateHolder> schedulingResultStateHolder, Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
		}

		public void ExecuteIfNecessary(ISchedulingProgress schedulingProgress, bool doIntraInteralCalculation)
		{
			if (!_schedulingResultStateHolder().GuessResourceCalculationHasBeenMade())
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(schedulingProgress, doIntraInteralCalculation);
			}
		}
	}
}