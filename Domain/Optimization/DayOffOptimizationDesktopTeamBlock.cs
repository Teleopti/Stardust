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
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly DaysOffBackToLegalState _daysOffBackToLegalState;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly ScheduleBlankSpots _scheduleBlankSpots;
		private readonly WorkShiftBackToLegalStateServiceProFactory _workShiftBackToLegalStateServiceProFactory;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;

		public DayOffOptimizationDesktopTeamBlock(IResourceCalculation resourceOptimizationHelper,
								TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
								Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			MatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
								TeamInfoFactoryFactory teamInfoFactoryFactory,
								IUserTimeZone userTimeZone,
								DaysOffBackToLegalState daysOffBackToLegalState,
								IScheduleDayEquator scheduleDayEquator,
								ScheduleBlankSpots scheduleBlankSpots,
								WorkShiftBackToLegalStateServiceProFactory workShiftBackToLegalStateServiceProFactory,
								IResourceCalculation resourceCalculation,
								Func<IScheduleDayChangeCallback> scheduleDayChangeCallback)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_userTimeZone = userTimeZone;
			_daysOffBackToLegalState = daysOffBackToLegalState;
			_scheduleDayEquator = scheduleDayEquator;
			_scheduleBlankSpots = scheduleBlankSpots;
			_workShiftBackToLegalStateServiceProFactory = workShiftBackToLegalStateServiceProFactory;
			_resourceCalculation = resourceCalculation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IPerson> selectedAgents, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			GroupPageLight groupPageLight,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
			using (_resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, true, selectedPeriod.Inflate(1)))
			{
				var matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, filterAgentsWithEmptyDaysIfClassic(optimizationPreferences, selectedAgents, selectedPeriod), selectedPeriod);
				var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
				if (optimizationPreferences.Extra.IsClassic())
				{
					//TO SIMULATE OLD CLASSIC BEHAVIOR (diff behavior between classic and teamblock)
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
					//////////////////
				}
				_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder(), _userTimeZone);
				var teamInfoFactory = _teamInfoFactoryFactory.Create(stateHolder.AllPermittedPersons, stateHolder.Schedules, groupPageLight);

				_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
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

		private IEnumerable<IPerson> filterAgentsWithEmptyDaysIfClassic(IOptimizationPreferences optimizationPreferences, IEnumerable<IPerson> agents, DateOnlyPeriod period) 
		{
			if (optimizationPreferences.Extra.IsClassic())
			{
				var scheduleDays = _schedulerStateHolder().Schedules.SchedulesForPeriod(period, agents.ToArray());
				var nonFullyScheduledAgents = scheduleDays.Where(x => !x.IsScheduled()).Select(x => x.Person);
				return agents.Except(nonFullyScheduledAgents).ToArray();
			}
			return agents;
		}
	}
}