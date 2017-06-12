using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization.ClassicLegacy
{
	public class ScheduleBlankSpots
	{
		private readonly IScheduleService _scheduleService;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;

		public ScheduleBlankSpots(IScheduleService scheduleService,
			ISchedulePartModifyAndRollbackService rollbackService,
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IResourceCalculation resourceOptimizationHelper,
			IUserTimeZone userTimeZone)
		{
			_scheduleService = scheduleService;
			_rollbackService = rollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
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
				    var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, stateHolder, _userTimeZone);

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