using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ScheduleExecutor : ScheduleExecutorOld
	{
		public ScheduleExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper, IResourceCalculation resourceOptimizationHelper, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, TeamBlockScheduleCommand teamBlockScheduleCommand, ClassicScheduleCommand classicScheduleCommand, IMatrixListFactory matrixListFactory, Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended, IWeeklyRestSolverCommand weeklyRestSolverCommand, PeriodExtractorFromScheduleParts periodExtractor, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IUserTimeZone userTimeZone, DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime) : base(schedulerStateHolder, requiredScheduleOptimizerHelper, resourceOptimizationHelper, scheduleDayChangeCallback, teamBlockScheduleCommand, classicScheduleCommand, matrixListFactory, resourceOptimizationHelperExtended, weeklyRestSolverCommand, periodExtractor, resourceCalculationContextFactory, userTimeZone, doFullResourceOptimizationOneTime)
		{
		}

		protected override void DoScheduling(ISchedulingProgress backgroundWorker, IEnumerable<IScheduleDay> selectedScheduleDays, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, SchedulingOptions schedulingOptions)
		{
			_teamBlockScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedScheduleDays, dayOffOptimizationPreferenceProvider);
		}
	}

	public class ScheduleExecutorOld
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		protected readonly TeamBlockScheduleCommand _teamBlockScheduleCommand;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		private readonly ClassicScheduleCommand _classicScheduleCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public ScheduleExecutorOld(Func<ISchedulerStateHolder> schedulerStateHolder,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			IResourceCalculation resourceOptimizationHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			TeamBlockScheduleCommand teamBlockScheduleCommand,
			ClassicScheduleCommand classicScheduleCommand,
			IMatrixListFactory matrixListFactory,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			PeriodExtractorFromScheduleParts periodExtractor,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IUserTimeZone userTimeZone,
			DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockScheduleCommand = teamBlockScheduleCommand;
			_classicScheduleCommand = classicScheduleCommand;
			_matrixListFactory = matrixListFactory;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_periodExtractor = periodExtractor;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_userTimeZone = userTimeZone;
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		[TestLog]
		public virtual void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IScheduleDay> selectedScheduleDays,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var schedulingOptions = optimizerOriginalPreferences.SchedulingOptions;
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, false);
			}
			//TODO: This is wrong I guess... Should be done inside rescalccontext block below probably. investigate! #43767
#pragma warning disable 618
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();
#pragma warning restore 618

			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			var selectedPersons = selectedScheduleDays.Select(x => x.Person).Distinct().ToList();

#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true))
#pragma warning restore 618
			{
				if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
				{
					schedulingOptions.OnlyShiftsWhenUnderstaffed = false;

					DoScheduling(backgroundWorker, selectedScheduleDays, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider, schedulingOptions);
				}
				else
				{
					_requiredScheduleOptimizerHelper.ScheduleSelectedStudents(selectedScheduleDays, backgroundWorker, schedulingOptions);
				}

				//shiftcategorylimitations
				if (!backgroundWorker.CancellationPending)
				{
					schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
					if (schedulingOptions.UseShiftCategoryLimitations)
					{
						var selectedPeriod = _periodExtractor.ExtractPeriod(selectedScheduleDays);
						if (!selectedPeriod.HasValue)
							return;

						var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedScheduleDays);
						if (matrixesOfSelectedScheduleDays.Count == 0)
							return;

						_requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays,
							backgroundWorker,
							optimizationPreferences,
							schedulingOptions,
							selectedPeriod.Value);
						
						ExecuteWeeklyRestSolverCommand(schedulingOptions, optimizationPreferences, selectedPersons,
							selectedPeriod.Value, matrixesOfSelectedScheduleDays, backgroundWorker, dayOffOptimizationPreferenceProvider);
					}
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		protected virtual void DoScheduling(ISchedulingProgress backgroundWorker, IEnumerable<IScheduleDay> selectedScheduleDays,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			SchedulingOptions schedulingOptions)
		{
			if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
			{
				_teamBlockScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedScheduleDays,
					dayOffOptimizationPreferenceProvider);
			}
			else
			{
				_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedScheduleDays,
					dayOffOptimizationPreferenceProvider, runWeeklyRestSolver);
			}
		}


		[TestLog]
		protected virtual void ExecuteWeeklyRestSolverCommand(SchedulingOptions schedulingOptions,
															IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons,
															DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays,
															ISchedulingProgress backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer, selectedPeriod,
											matrixesOfSelectedScheduleDays, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}	
	}
}