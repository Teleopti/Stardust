using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleCommand
	{
		void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver);
	}

	public class ScheduleCommand : IScheduleCommand
	{
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly ITeamBlockScheduleCommand _teamBlockScheduleCommand;
		private readonly IClassicScheduleCommand _classicScheduleCommand;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly Func<IWorkShiftFinderResultHolder> _workShiftFinderResultHolder;
		private readonly Func<IResourceOptimizationHelperExtended> _resourceOptimizationHelperExtended;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;

		public ScheduleCommand(Func<IPersonSkillProvider> personSkillProvider,
			IResourceOptimizationHelper resourceOptimizationHelper,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			ITeamBlockScheduleCommand teamBlockScheduleCommand,
			IClassicScheduleCommand classicScheduleCommand,
			IMatrixListFactory matrixListFactory,
			IOptimizerHelperHelper optimizerHelper,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Func<IResourceOptimizationHelperExtended> resourceOptimizationHelperExtended,
			IWeeklyRestSolverCommand weeklyRestSolverCommand
			)
		{
			_personSkillProvider = personSkillProvider;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamBlockScheduleCommand = teamBlockScheduleCommand;
			_classicScheduleCommand = classicScheduleCommand;
			_matrixListFactory = matrixListFactory;
			_optimizerHelper = optimizerHelper;
			_workShiftFinderResultHolder = workShiftFinderResultHolder;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
		}

		public void Execute(IOptimizerOriginalPreferences optimizerOriginalPreferences, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulerStateHolder schedulerStateHolder, IList<IScheduleDay> selectedScheduleDays,
			IGroupPagePerDateHolder groupPagePerDateHolder, IRequiredScheduleHelper requiredScheduleOptimizerHelper,
			IOptimizationPreferences optimizationPreferences, bool runWeeklyRestSolver)
		{
			setThreadCulture();
			var schedulingOptions = optimizerOriginalPreferences.SchedulingOptions;
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			bool lastCalculationState = schedulerStateHolder.SchedulingResultState.SkipResourceCalculation;
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = false;
			if (lastCalculationState)
			{
				_resourceOptimizationHelperExtended().ResourceCalculateAllDays(backgroundWorker, true);
			}

			//set to false for first scheduling and then use it for RemoveShiftCategoryBackToLegalState
			var useShiftCategoryLimitations = schedulingOptions.UseShiftCategoryLimitations;
			schedulingOptions.UseShiftCategoryLimitations = false;

			var selectedPersons = selectedScheduleDays.Select(x => x.Person).Distinct().ToList();

			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				schedulingOptions.OnlyShiftsWhenUnderstaffed = false;

				var minutesPerInterval = 15;
				if (schedulerStateHolder.SchedulingResultState.Skills.Count > 0)
				{
					minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
				}
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
				var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
				using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
				{
					if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
					{
						var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
							schedulingOptions.ConsiderShortBreaks);

						ISchedulePartModifyAndRollbackService rollbackService =
							new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
								_scheduleDayChangeCallback(),
								new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
						_workShiftFinderResultHolder().Clear();
						_workShiftFinderResultHolder().AddResults(_teamBlockScheduleCommand.Execute(schedulingOptions, backgroundWorker, selectedPersons, selectedScheduleDays,
								rollbackService, resourceCalculateDelayer).GetResults(),DateTime.Today);
					}
					else
					{
						_classicScheduleCommand.Execute(schedulingOptions, backgroundWorker, requiredScheduleOptimizerHelper, selectedScheduleDays, runWeeklyRestSolver);
					}
				}
			}
			else
			{
				requiredScheduleOptimizerHelper.ScheduleSelectedStudents(selectedScheduleDays, backgroundWorker, schedulingOptions);
			}

			//shiftcategorylimitations
			if (!backgroundWorker.CancellationPending)
			{
				schedulingOptions.UseShiftCategoryLimitations = useShiftCategoryLimitations;
				if (schedulingOptions.UseShiftCategoryLimitations)
				{
					IList<IScheduleMatrixPro> allMatrixes = new List<IScheduleMatrixPro>();
					var selectedPeriod = _optimizerHelper.GetSelectedPeriod(selectedScheduleDays);

					if (schedulingOptions.UseTeam)
					{
						allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
					}

					IList<IScheduleMatrixPro> matrixesOfSelectedScheduleDays =
						_matrixListFactory.CreateMatrixList(selectedScheduleDays, selectedPeriod);
					if (matrixesOfSelectedScheduleDays.Count == 0)
						return;

					
					requiredScheduleOptimizerHelper.RemoveShiftCategoryBackToLegalState(matrixesOfSelectedScheduleDays, backgroundWorker,
					optimizationPreferences,
					schedulingOptions,
					selectedPeriod, allMatrixes);	
					
					ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,_scheduleDayChangeCallback(), new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
					var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);
					_weeklyRestSolverCommand.Execute(schedulingOptions, optimizationPreferences, selectedPersons, rollbackService, resourceCalculateDelayer, selectedPeriod, matrixesOfSelectedScheduleDays,backgroundWorker);
				}
			}
			schedulerStateHolder.SchedulingResultState.SkipResourceCalculation = lastCalculationState;
		}

		private static void setThreadCulture()
		{
			Thread.CurrentThread.CurrentCulture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
			Thread.CurrentThread.CurrentUICulture = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;
		}
	}
}