using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.ClassicLegacy;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopTeamBlock : IDayOffOptimizationDesktop
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimizationHelperExtended _resouceOptimizationHelperExtended;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;

		public DayOffOptimizationDesktopTeamBlock(IResourceCalculation resourceOptimizationHelper,
								ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService,
								Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
								IMatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
								TeamInfoFactoryFactory teamInfoFactoryFactory,
								IUserTimeZone userTimeZone,
								DaysOffBackToLegalState daysOffBackToLegalState,
								IScheduleDayEquator scheduleDayEquator,
								ScheduleBlankSpots scheduleBlankSpots,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceOptimizationHelperExtended = resouceOptimizationHelperExtended;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_userTimeZone = userTimeZone;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleDayEquator = scheduleDayEquator;
			_scheduleBlankSpots = scheduleBlankSpots;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IScheduleDay> selectedDays, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			GroupPageLight groupPageLight,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, true))
#pragma warning restore 618
			{
				var matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, filterAgentsWithEmptyDaysIfClassic(optimizationPreferences, selectedDays));
				var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
				if (optimizationPreferences.Extra.IsClassic())
				{
					var matrixListOriginalStateContainer = matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToArray();

					_daysOffBackToLegalState.Execute(matrixListOriginalStateContainer,
													backgroundWorker, stateHolder.CommonStateHolder.ActiveDayOffs.ToList()[0],
													schedulingOptions,
													dayOffOptimizationPreferenceProvider,
													optimizationPreferences,
													workShiftFinderResultHolder,
													resourceOptimizerPersonOptimized);
					var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();
					foreach (var matrixOriginalStateContainer in matrixListOriginalStateContainer)
					{
						workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling)));
					}
					_scheduleBlankSpots.Execute(matrixListOriginalStateContainer, optimizationPreferences);
				}
				_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);
				_resouceOptimizationHelperExtended.ResourceCalculateAllDays(backgroundWorker, false);
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder(), _userTimeZone);
				var teamInfoFactory = _teamInfoFactoryFactory.Create(stateHolder.AllPermittedPersons, stateHolder.Schedules, groupPageLight);

				_teamBlockDayOffOptimizerService.OptimizeDaysOff(matrixList,
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

		private static IEnumerable<IScheduleDay> filterAgentsWithEmptyDaysIfClassic(IOptimizationPreferences optimizationPreferences, IEnumerable<IScheduleDay> selectedDays) 
		{
			if (optimizationPreferences.Extra.IsClassic())
			{
				var nonFullyScheduledAgents = selectedDays.Where(x => !x.IsScheduled()).Select(x => x.Person).ToArray();
				return selectedDays.Where(x => !nonFullyScheduledAgents.Contains(x.Person)).ToArray();
			}
			return selectedDays;
		}
	}
}