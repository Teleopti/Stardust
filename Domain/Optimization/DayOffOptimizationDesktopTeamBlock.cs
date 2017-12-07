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
	public class DayOffOptimizationDesktopTeamBlock
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly MatrixListFactory _matrixListFactory;
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
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);

			using (_resourceCalculationContextFactory.Create(stateHolder.SchedulingResultState, true, selectedPeriod.Inflate(1)))
			{
				IEnumerable<IScheduleMatrixPro> matrixList;
				if (optimizationPreferences.Extra.IsClassic())
				{
					//TO SIMULATE OLD CLASSIC BEHAVIOR (diff behavior between classic and teamblock)
					var scheduleDays = _schedulerStateHolder().Schedules.SchedulesForPeriod(selectedPeriod, selectedAgents.ToArray());
					var nonFullyScheduledAgents = scheduleDays.Where(x => !x.IsScheduled()).Select(x => x.Person);
					var filteredAgents = selectedAgents.Except(nonFullyScheduledAgents).ToArray();
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, filteredAgents, selectedPeriod);
					var matrixListOriginalStateContainer = matrixList.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator)).ToArray();
					_daysOffBackToLegalState.Execute(matrixListOriginalStateContainer,
													backgroundWorker, stateHolder.CommonStateHolder.ActiveDayOffs.ToList()[0],
													schedulingOptions,
													dayOffOptimizationPreferenceProvider,
													optimizationPreferences,
													resourceOptimizerPersonOptimized);
					var workShiftBackToLegalStateService = _workShiftBackToLegalStateServiceProFactory.Create();
					foreach (var matrixOriginalStateContainer in matrixListOriginalStateContainer)
					{
						workShiftBackToLegalStateService.Execute(matrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling)));
					}
					_scheduleBlankSpots.Execute(matrixListOriginalStateContainer, optimizationPreferences);
					//////////////////
				}
				else
				{
					matrixList = _matrixListFactory.CreateMatrixListForSelection(stateHolder.Schedules, selectedAgents, selectedPeriod);
				}
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder(), _userTimeZone);
				var teamInfoFactory = _teamInfoFactoryFactory.Create(selectedAgents, stateHolder.Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

				_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
					selectedPeriod,
					selectedPersons,
					optimizationPreferences,
					schedulingOptions,
					resourceCalculateDelayer,
					dayOffOptimizationPreferenceProvider,
					new FixedBlockPreferenceProvider(optimizationPreferences.Extra), 
					teamInfoFactory,
					backgroundWorker);
			}
		}
	}
}