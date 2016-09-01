using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
	public class ScheduleBlankSpots
	{
		private readonly IScheduleService _scheduleService;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public ScheduleBlankSpots(IScheduleService scheduleService,
			ISchedulePartModifyAndRollbackService rollbackService,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_scheduleService = scheduleService;
			_rollbackService = rollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public void Execute(IEnumerable<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers, IOptimizationPreferences optimizerPreferences)
		{
			var schedulingOptionsSynchronizer = new SchedulingOptionsCreator();
			var schedulingOptions = schedulingOptionsSynchronizer.CreateSchedulingOptions(optimizerPreferences);
		    var stateHolder = _schedulingResultStateHolder();

		    foreach (var matrixOriginalStateContainer in matrixOriginalStateContainers)
			{
				if (!matrixOriginalStateContainer.StillAlive)
					continue;

			    foreach (var scheduleDayPro in matrixOriginalStateContainer.ScheduleMatrix.UnlockedDays)
				{
				    var scheduleDay = scheduleDayPro.DaySchedulePart();
				    if (scheduleDay.IsScheduled())
						continue;

					var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
				    var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, stateHolder);

					if (!_scheduleService.SchedulePersonOnDay(scheduleDay, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, _rollbackService))
					{
						matrixOriginalStateContainer.StillAlive = false;
						break;
					}
				}
			}
		}
	}
}