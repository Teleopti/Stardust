using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IRequiredScheduleHelper
	{
		void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions);

		void RemoveShiftCategoryBackToLegalState(
			IList<IScheduleMatrixPro> matrixList,
			IBackgroundWorkerWrapper backgroundWorker, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes);

		void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
			IList<IScheduleMatrixPro> matrixListAll, bool useOccupancyAdjustment,
			IBackgroundWorkerWrapper backgroundWorker, ISchedulingOptions schedulingOptions);
	}

	public class RequiredScheduleHelper : IRequiredScheduleHelper
	{
		private readonly IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly IStudentSchedulingService _studentSchedulingService;
		private readonly Func<IOptimizationPreferences> _optimizationPreferences;
		private readonly IScheduleService _scheduleService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IGridlockManager _gridlockManager;
		private readonly IDaysOffSchedulingService _daysOffSchedulingService;
		private readonly Func<IWorkShiftFinderResultHolder> _allResults;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		public RequiredScheduleHelper(IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak, Func<ISchedulingResultStateHolder> resultStateHolder, Func<IFixedStaffSchedulingService> fixedStaffSchedulingService, IStudentSchedulingService studentSchedulingService, Func<IOptimizationPreferences> optimizationPreferences, IScheduleService scheduleService, IResourceOptimizationHelper resourceOptimizationHelper, IGridlockManager gridlockManager, IDaysOffSchedulingService daysOffSchedulingService, Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleDayEquator scheduleDayEquator, IMatrixListFactory matrixListFactory, ITeamBlockRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService, ITeamBlockScheduler teamBlockScheduler, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockSchedulingOptions teamBlockSchedulingOptions)
		{
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_resultStateHolder = resultStateHolder;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_studentSchedulingService = studentSchedulingService;
			_optimizationPreferences = optimizationPreferences;
			_scheduleService = scheduleService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_gridlockManager = gridlockManager;
			_daysOffSchedulingService = daysOffSchedulingService;
			_allResults = workShiftFinderResultHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_matrixListFactory = matrixListFactory;
			_teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
		}

		public void RemoveShiftCategoryBackToLegalState(
			IList<IScheduleMatrixPro> matrixList,
			IBackgroundWorkerWrapper backgroundWorker, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes)
		{
			if (matrixList == null)
				throw new ArgumentNullException("matrixList");
			if (backgroundWorker == null)
				throw new ArgumentNullException("backgroundWorker");
			if (schedulingOptions == null)
				throw new ArgumentNullException("schedulingOptions");
			using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
			{
				if (backgroundWorker.CancellationPending)
					return;

				var schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(_resultStateHolder(), _scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				var shiftNudgeDirective = new ShiftNudgeDirective();
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, schedulingOptions.ConsiderShortBreaks);

				var unresolvedScheduleDayPros = checkShiftCategoryLimitations(matrixList, schedulingOptions, optimizationPreferences, schedulePartModifyAndRollbackService, resourceCalculateDelayer, shiftNudgeDirective);
				if (unresolvedScheduleDayPros.IsEmpty()) return;
				
				scheduleUnresolvedDays(schedulingOptions,unresolvedScheduleDayPros,matrixList,schedulePartModifyAndRollbackService,resourceCalculateDelayer,shiftNudgeDirective);	
				checkShiftCategoryLimitations(matrixList, schedulingOptions, optimizationPreferences,schedulePartModifyAndRollbackService,resourceCalculateDelayer,shiftNudgeDirective);
			}
		}

		private IList<IScheduleDayPro> checkShiftCategoryLimitations(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ShiftNudgeDirective shiftNudgeDirective)
		{
			var unresolvedScheduleDayPros = new List<IScheduleDayPro>();
			
			for (var i = 0; i < 2; i++)
			{
				foreach (var matrix in matrixList)
				{
					var result = _teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, matrix, _resultStateHolder(), schedulePartModifyAndRollbackService, resourceCalculateDelayer, matrixList, shiftNudgeDirective, optimizationPreferences);
					if (!result.IsEmpty()) 
						unresolvedScheduleDayPros.AddRange(result);
				}
			}

			return unresolvedScheduleDayPros;
		}

		private void scheduleUnresolvedDays(ISchedulingOptions schedulingOptions, IEnumerable<IScheduleDayPro> unresolvedScheduleDayPros, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ShiftNudgeDirective shiftNudgeDirective)
		{
			var isSingleAgentTeam = _teamBlockSchedulingOptions.IsSingleAgentTeam(schedulingOptions);
			schedulingOptions.NotAllowedShiftCategories.Clear();

			foreach (var scheduleDayPro in unresolvedScheduleDayPros)
			{
				var dateOnly = scheduleDayPro.Day;
				var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
				var teamInfo = _teamInfoFactory.CreateTeamInfo(scheduleDayPro.DaySchedulePart().Person, dateOnlyPeriod, matrixList);
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinderTypeForAdvanceScheduling, isSingleAgentTeam);
				if(teamBlockInfo == null) continue;
				_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, dateOnly, schedulingOptions, schedulePartModifyAndRollbackService, resourceCalculateDelayer, _resultStateHolder(), shiftNudgeDirective);
			}	
		}

		public void ScheduleSelectedPersonDays(IList<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
			IList<IScheduleMatrixPro> matrixListAll, bool useOccupancyAdjustment,
			IBackgroundWorkerWrapper backgroundWorker, ISchedulingOptions schedulingOptions)
		{
			if (matrixList == null) throw new ArgumentNullException("matrixList");

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

			var unlockedSchedules = (from scheduleMatrixPro in matrixList
									 from scheduleDayPro in scheduleMatrixPro.UnlockedDays
									 select scheduleDayPro.DaySchedulePart()).ToList();

			if (!unlockedSchedules.Any()) return;

			var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();

			var selectedPeriod = allSelectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());

			schedulingOptions.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedPersons, period);

			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);

			var fixedStaffSchedulingService = _fixedStaffSchedulingService();
			fixedStaffSchedulingService.ClearFinderResults();
			var sendEventEvery = schedulingOptions.RefreshRate;
			var scheduledCount = 0;
			var scheduleRunCancelled = false;

			EventHandler<SchedulingServiceBaseEventArgs> onDayScheduled = (sender, e) =>
			{
				e.AppendCancelAction(() => scheduleRunCancelled = true);
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
				}
				if (e.IsSuccessful)
					scheduledCount++;
				if (scheduledCount >= sendEventEvery)
				{
					backgroundWorker.ReportProgress(1, e);
					scheduledCount = 0;
				}
				scheduleRunCancelled = e.Cancel;
			};

			DateTime schedulingTime = DateTime.Now;
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService =
				new DeleteAndResourceCalculateService(new DeleteSchedulePartService(_resultStateHolder), _resourceOptimizationHelper);
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_resultStateHolder(),
				_scheduleDayChangeCallback,
				tagSetter);
			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService =
				new NightRestWhiteSpotSolverService(new NightRestWhiteSpotSolver(),
					deleteAndResourceCalculateService,
					_scheduleService, _allResults,
					new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
						schedulingOptions.ConsiderShortBreaks));

			using (PerformanceOutput.ForOperation(string.Concat("Scheduling ", unlockedSchedules.Count, " days")))
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff =
					new SchedulePartModifyAndRollbackService(_resultStateHolder(), _scheduleDayChangeCallback,
						tagSetter);
				_daysOffSchedulingService.DayScheduled += onDayScheduled;
				_daysOffSchedulingService.Execute(matrixList, matrixListAll, schedulePartModifyAndRollbackServiceForContractDaysOff,
					schedulingOptions);
				_daysOffSchedulingService.DayScheduled -= onDayScheduled;

				//lock none selected days
				var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, period);
				matrixUnselectedDaysLocker.Execute();

				unlockedSchedules = (from scheduleMatrixPro in matrixList
									 from scheduleDayPro in scheduleMatrixPro.UnlockedDays
									 select scheduleDayPro.DaySchedulePart()).ToList();

				if (!unlockedSchedules.Any())
					return;


				IList<IScheduleMatrixOriginalStateContainer> originalStateContainers = new List<IScheduleMatrixOriginalStateContainer>();
				foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixList(allSelectedSchedules, new DateOnlyPeriod(selectedPeriod.First(), selectedPeriod.Last())))
					originalStateContainers.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, _scheduleDayEquator));

				foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
				{
					foreach (var day in scheduleMatrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
					{
						if (day.DaySchedulePart().IsScheduled())
							scheduleMatrixOriginalStateContainer.ScheduleMatrix.LockPeriod(new DateOnlyPeriod(day.Day, day.Day));
					}
				}

				fixedStaffSchedulingService.DayScheduled += onDayScheduled;
				if (!scheduleRunCancelled)
				{
					fixedStaffSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, useOccupancyAdjustment, false,
						rollbackService);
				}
				_allResults().AddResults(fixedStaffSchedulingService.FinderResults, schedulingTime);
				fixedStaffSchedulingService.FinderResults.Clear();
				fixedStaffSchedulingService.DayScheduled -= onDayScheduled;

				var progressChangeEvent = new TeleoptiProgressChangeMessage(Resources.TryingToResolveUnscheduledDaysDotDotDot);
				backgroundWorker.ReportProgress(0, progressChangeEvent);
				foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
				{
					int iterations = 0;
					while (
						nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix, schedulingOptions,
							rollbackService) && iterations < 10)
					{
						if (backgroundWorker.CancellationPending || scheduleRunCancelled)
							break;

						iterations++;
					}
					if (backgroundWorker.CancellationPending || scheduleRunCancelled)
						break;
				}

				if (schedulingOptions.RotationDaysOnly || schedulingOptions.PreferencesDaysOnly ||
					schedulingOptions.UsePreferencesMustHaveOnly || schedulingOptions.AvailabilityDaysOnly)
					schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();

			}
		}

		public void ScheduleSelectedStudents(IList<IScheduleDay> allSelectedSchedules, IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions)
		{
			if (allSelectedSchedules == null) throw new ArgumentNullException("allSelectedSchedules");
			if (schedulingOptions == null) throw new ArgumentNullException("schedulingOptions");

			IList<IScheduleDay> unlockedSchedules = new List<IScheduleDay>();
			foreach (var scheduleDay in allSelectedSchedules)
			{
				GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay);
				if (locks == null || locks.Count == 0)
					unlockedSchedules.Add(scheduleDay);
			}

			if (unlockedSchedules.Count == 0)
			{
				return;
			}

			var selectedPersons = new PersonListExtractorFromScheduleParts(unlockedSchedules).ExtractPersons();
			var selectedPeriod = unlockedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());
			var scheduledCount = 0;
			var sendEventEvery = schedulingOptions.RefreshRate;

			_optimizationPreferences().Rescheduling.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedPersons, period);
			
			EventHandler<SchedulingServiceBaseEventArgs> onDayScheduled = (sender, e) =>
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
				}
				if (e.IsSuccessful)
					scheduledCount++;
				if (scheduledCount >= sendEventEvery)
				{
					backgroundWorker.ReportProgress(1, e);
					scheduledCount = 0;
				}
			};
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
			_studentSchedulingService.ClearFinderResults();
			_studentSchedulingService.DayScheduled += onDayScheduled;
			DateTime schedulingTime = DateTime.Now;
			ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_resultStateHolder(),
				_scheduleDayChangeCallback,
				new ScheduleTagSetter
					(schedulingOptions
						.
						TagToUseOnScheduling));
			using (PerformanceOutput.ForOperation("Scheduling " + unlockedSchedules.Count))
			{
				_studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, false, rollbackService);
			}

			_allResults().AddResults(_studentSchedulingService.FinderResults, schedulingTime);
			_studentSchedulingService.DayScheduled -= onDayScheduled;
		}
	}

	public class TeleoptiProgressChangeMessage
	{
		public string Message { get; set; }

		public TeleoptiProgressChangeMessage(string message)
		{
			Message = message;
		}
	}

	public interface IScheduleOptimizerHelper
	{
		IEditableShift PrepareAndChooseBestShift(IScheduleDay schedulePart,
			ISchedulingOptions schedulingOptions,
			IWorkShiftFinderService finderService);

		void ResetWorkShiftFinderResults();

		void GetBackToLegalState(IList<IScheduleMatrixPro> matrixList,
			ISchedulerStateHolder schedulerStateHolder,
			IBackgroundWorkerWrapper backgroundWorker,
			ISchedulingOptions schedulingOptions,
			DateOnlyPeriod selectedPeriod,
			IList<IScheduleMatrixPro> allMatrixes);

		void DaysOffBackToLegalState(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainers,
			IBackgroundWorkerWrapper backgroundWorker,
			IDayOffTemplate dayOffTemplate,
			bool reschedule,
			ISchedulingOptions schedulingOptions,
			IDaysOffPreferences daysOffPreferences);

		void ReOptimize(IBackgroundWorkerWrapper backgroundWorker, IList<IScheduleDay> selectedDays, ISchedulingOptions schedulingOptions);

		IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod);
	}
}