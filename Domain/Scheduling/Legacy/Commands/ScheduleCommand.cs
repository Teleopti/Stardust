using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleCommand
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly ITeamBlockScheduleCommand _teamBlockScheduleCommand;
		private readonly IClassicScheduleCommand _classicScheduleCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public ScheduleCommand(Func<ISchedulerStateHolder> schedulerStateHolder,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			IResourceCalculation resourceOptimizationHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			ITeamBlockScheduleCommand teamBlockScheduleCommand,
			IClassicScheduleCommand classicScheduleCommand,
			IMatrixListFactory matrixListFactory,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
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
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
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
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary();

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

					if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
					{
						var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
							schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);

						ISchedulePartModifyAndRollbackService rollbackService =
							new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
								_scheduleDayChangeCallback(),
								new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
						_workShiftFinderResultHolder().Clear();
						_workShiftFinderResultHolder()
							.AddResults(
								_teamBlockScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedPersons, selectedScheduleDays,
									rollbackService, resourceCalculateDelayer, dayOffOptimizationPreferenceProvider).GetResults(), DateTime.Today);
					}
					else
					{
						_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, _requiredScheduleOptimizerHelper,
							selectedScheduleDays, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider);
					}
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
						IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
						var selectedPeriod = _periodExtractor.ExtractPeriod(selectedScheduleDays);
						if (!selectedPeriod.HasValue) return;

						if (schedulingOptions.UseTeam)
						{
							allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPeriod.Value);
						}

						IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays =
							_matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedScheduleDays);
						if (matrixesOfSelectedScheduleDays.Count == 0)
							return;


						_requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays,
							backgroundWorker,
							optimizationPreferences,
							schedulingOptions,
							selectedPeriod.Value, allMatrixes);
						
						ExecuteWeeklyRestSolverCommand(schedulingOptions, optimizationPreferences, selectedPersons,
							selectedPeriod.Value, matrixesOfSelectedScheduleDays, backgroundWorker, dayOffOptimizationPreferenceProvider);
					}
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}


		[TestLog]
		protected virtual void ExecuteWeeklyRestSolverCommand(ISchedulingOptions schedulingOptions,
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