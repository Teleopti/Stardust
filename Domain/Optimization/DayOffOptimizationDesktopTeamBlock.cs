using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopTeamBlock : DayOffOptimizationDesktop
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public DayOffOptimizationDesktopTeamBlock(
								IResourceOptimizationHelper resourceOptimizationHelper,
								IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
								ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService,
								Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
								IMatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								DaysOffBackToLegalState daysOffBackToLegalState,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
								IScheduleDayEquator scheduleDayEquator,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								ScheduleBlankSpots scheduleBlankSpots) :
			base(matrixListFactory,
								optimizerHelperHelper,
								schedulerStateHolder,
								daysOffBackToLegalState,
								scheduleDayChangeCallback,
								scheduleDayEquator,
								resouceOptimizationHelperExtended,
								workShiftBackToLegalStateServiceProFactory,
								scheduleBlankSpots)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		protected override void Optimize(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization,
													DateOnlyPeriod selectedPeriod,
													ISchedulingProgress backgroundWorker,
													IOptimizationPreferences optimizationPreferences,
													IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var matrixListForDayOffOptimization = matrixOriginalStateContainerListForDayOffOptimization.Select(container => container.ScheduleMatrix).ToList();
			var selectedPersons = matrixListForDayOffOptimization.Select(matrixList => matrixList.Person).Distinct().ToList();

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder());
			_groupPersonBuilderWrapper.SetSingleAgentTeam();
			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);

			_teamBlockDayOffOptimizerService.OptimizeDaysOff(matrixListForDayOffOptimization,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulingOptions,
				resourceCalculateDelayer,
				dayOffOptimizationPreferenceProvider,
				teamInfoFactory,
				backgroundWorker);
		}
	}
}