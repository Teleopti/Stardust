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
	public interface IScheduleExecutor
	{
		void Execute(ISchedulingCallback schedulingCallback, 
			SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ScheduleExecutor : ScheduleExecutorOld
	{
		public ScheduleExecutor(Func<ISchedulerStateHolder> schedulerStateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, IScheduling teamBlockScheduling, ClassicScheduleCommand classicScheduleCommand, MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IUserTimeZone userTimeZone, IResourceCalculation resourceCalculation) 
			: base(schedulerStateHolder, requiredScheduleOptimizerHelper, scheduleDayChangeCallback, teamBlockScheduling, classicScheduleCommand, matrixListFactory, weeklyRestSolverCommand, resourceCalculationContextFactory, userTimeZone, resourceCalculation)
		{
		}

		protected override void DoScheduling(ISchedulingCallback schedulingCallback, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, SchedulingOptions schedulingOptions)
		{
			TeamBlockScheduling.Execute(schedulingCallback, schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider);
		}
	}

	[RemoveMeWithToggle("Rename and keep this one merge with new one", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class ScheduleExecutorOld : IScheduleExecutor
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IRequiredScheduleHelper _requiredScheduleOptimizerHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		[RemoveMeWithToggle("make private", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		protected readonly IScheduling TeamBlockScheduling;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		private readonly ClassicScheduleCommand _classicScheduleCommand;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IResourceCalculation _resourceCalculation;

		public ScheduleExecutorOld(Func<ISchedulerStateHolder> schedulerStateHolder,
			IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			IScheduling teamBlockScheduling,
			ClassicScheduleCommand classicScheduleCommand,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			IUserTimeZone userTimeZone,
			IResourceCalculation resourceCalculation)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleOptimizerHelper = requiredScheduleOptimizerHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			TeamBlockScheduling = teamBlockScheduling;
			_classicScheduleCommand = classicScheduleCommand;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_userTimeZone = userTimeZone;
			_resourceCalculation = resourceCalculation;
		}

		[TestLog]
		public virtual void Execute(ISchedulingCallback schedulingCallback, 
			SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			var lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true, selectedPeriod.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, schedulerStateHolder.ConsiderShortBreaks, false));
				if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
				{
					schedulingOptions.OnlyShiftsWhenUnderstaffed = false;

					DoScheduling(schedulingCallback, backgroundWorker, selectedAgents, selectedPeriod, runWeeklyRestSolver, dayOffOptimizationPreferenceProvider, schedulingOptions);
				}
				else
				{
					//lets remove this one later
					var selectedScheduleDays = schedulerStateHolder.Schedules.SchedulesForPeriod(selectedPeriod, selectedAgents.ToArray());
					_requiredScheduleOptimizerHelper.ScheduleSelectedStudents(selectedScheduleDays, backgroundWorker, schedulingOptions);
				}

				//shiftcategorylimitations
				if (!backgroundWorker.CancellationPending)
				{
					schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
					if (schedulingOptions.UseShiftCategoryLimitations)
					{
						var matrixesOfSelectedScheduleDays = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, selectedAgents, selectedPeriod);
						if (!matrixesOfSelectedScheduleDays.Any())
							return;

						_requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays,
							backgroundWorker,
							optimizationPreferences,
							schedulingOptions,
							selectedPeriod);
						
						ExecuteWeeklyRestSolverCommand(schedulingOptions, optimizationPreferences, selectedAgents.ToArray(),
							selectedPeriod, matrixesOfSelectedScheduleDays, backgroundWorker, dayOffOptimizationPreferenceProvider);
					}
				}
			}

			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		protected virtual void DoScheduling(ISchedulingCallback schedulingCallback, ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod,
			bool runWeeklyRestSolver, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			SchedulingOptions schedulingOptions)
		{
			if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
			{
				TeamBlockScheduling.Execute(new NoSchedulingCallback(), schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider);
			}
			else
			{
				_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod, dayOffOptimizationPreferenceProvider, runWeeklyRestSolver);
			}
		}


		[TestLog]
		protected virtual void ExecuteWeeklyRestSolverCommand(SchedulingOptions schedulingOptions,
															IOptimizationPreferences optimizationPreferences, IList<IPerson> selectedPersons,
															DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> matrixesOfSelectedScheduleDays,
															ISchedulingProgress backgroundWorker, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState, _scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);
			_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer, selectedPeriod,
											matrixesOfSelectedScheduleDays, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}	
	}
}