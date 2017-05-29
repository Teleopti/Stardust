using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IRequiredScheduleHelper
	{
		void ScheduleSelectedStudents(IEnumerable<IScheduleDay> allSelectedSchedules, ISchedulingProgress backgroundWorker,
			SchedulingOptions schedulingOptions);

		void RemoveShiftCategoryBackToLegalState(
			IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod);

		void ScheduleSelectedPersonDays(IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, SchedulingOptions schedulingOptions);
	}

	public class RequiredScheduleHelper : IRequiredScheduleHelper
	{
		private readonly ISchedulePeriodListShiftCategoryBackToLegalStateService _shiftCategoryBackToLegalState;
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly IStudentSchedulingService _studentSchedulingService;
		private readonly Func<IOptimizationPreferences> _optimizationPreferences;
		private readonly IGridlockManager _gridlockManager;
		private readonly IDaysOffSchedulingService _daysOffSchedulingService;
		private readonly Func<IWorkShiftFinderResultHolder> _allResults;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly TeamBlockRetryRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;
		private readonly INightRestWhiteSpotSolverServiceFactory _nightRestWhiteSpotSolverServiceFactory;

		public RequiredScheduleHelper(ISchedulePeriodListShiftCategoryBackToLegalStateService shiftCategoryBackToLegalState, 
				RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak, 
				Func<ISchedulingResultStateHolder> resultStateHolder, 
				Func<IFixedStaffSchedulingService> fixedStaffSchedulingService, 
				IStudentSchedulingService studentSchedulingService, 
				Func<IOptimizationPreferences> optimizationPreferences, 
				IGridlockManager gridlockManager, 
				IDaysOffSchedulingService daysOffSchedulingService, 
				Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, 
				IScheduleDayChangeCallback scheduleDayChangeCallback, 
				IScheduleDayEquator scheduleDayEquator,
			MatrixListFactory matrixListFactory,
				TeamBlockRetryRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService,
				INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory)
		{
			_shiftCategoryBackToLegalState = shiftCategoryBackToLegalState;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_resultStateHolder = resultStateHolder;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_studentSchedulingService = studentSchedulingService;
			_optimizationPreferences = optimizationPreferences;
			_gridlockManager = gridlockManager;
			_daysOffSchedulingService = daysOffSchedulingService;
			_allResults = workShiftFinderResultHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_matrixListFactory = matrixListFactory;
			_teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
			_nightRestWhiteSpotSolverServiceFactory = nightRestWhiteSpotSolverServiceFactory;
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		public void RemoveShiftCategoryBackToLegalState(
			IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod)
		{
			if (matrixList == null)
				throw new ArgumentNullException(nameof(matrixList));
			if (backgroundWorker == null)
				throw new ArgumentNullException(nameof(backgroundWorker));
			if (schedulingOptions == null)
				throw new ArgumentNullException(nameof(schedulingOptions));
			using (PerformanceOutput.ForOperation("ShiftCategoryLimitations"))
			{
				if (backgroundWorker.CancellationPending)
					return;

				if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
				{
					_teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, _resultStateHolder(), matrixList, optimizationPreferences, backgroundWorker);
				}
				else
				{
					_shiftCategoryBackToLegalState.Execute(matrixList, schedulingOptions, optimizationPreferences);
				}
			}
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
		public void ScheduleSelectedPersonDays(IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, SchedulingOptions schedulingOptions)
		{
			if (matrixList == null) throw new ArgumentNullException(nameof(matrixList));

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

			//lock none selected days
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, selectedPeriod);
			matrixUnselectedDaysLocker.Execute();
			var unlockedSchedules = (from scheduleMatrixPro in matrixList
									 from scheduleDayPro in scheduleMatrixPro.UnlockedDays
									 select scheduleDayPro.DaySchedulePart()).ToList();

			if (!unlockedSchedules.Any()) return;

			var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();
			schedulingOptions.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedPersons, selectedPeriod);

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
					scheduleRunCancelled = true;
				}
				if (e.IsSuccessful)
					scheduledCount++;
				if (scheduledCount >= sendEventEvery)
				{
					backgroundWorker.ReportProgress(1, e);
					scheduledCount = 0;
				}
			};

			DateTime schedulingTime = DateTime.Now;
				ISchedulePartModifyAndRollbackService rollbackService = new SchedulePartModifyAndRollbackService(_resultStateHolder(),
				_scheduleDayChangeCallback,
				tagSetter);

			using (PerformanceOutput.ForOperation(string.Concat("Scheduling ", unlockedSchedules.Count, " days")))
			{
				ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackServiceForContractDaysOff =
					new SchedulePartModifyAndRollbackService(_resultStateHolder(), _scheduleDayChangeCallback,
						tagSetter);
				_daysOffSchedulingService.DayScheduled += onDayScheduled;
				_daysOffSchedulingService.Execute(matrixList, schedulePartModifyAndRollbackServiceForContractDaysOff,
					schedulingOptions, tagSetter);
				_daysOffSchedulingService.DayScheduled -= onDayScheduled;

				IList<IScheduleMatrixOriginalStateContainer> originalStateContainers = new List<IScheduleMatrixOriginalStateContainer>();
				foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixListForSelection(_resultStateHolder().Schedules, selectedAgents, selectedPeriod))
					originalStateContainers.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, _scheduleDayEquator));

				foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
				{
					foreach (var day in scheduleMatrixOriginalStateContainer.ScheduleMatrix.EffectivePeriodDays)
					{
						if (day.DaySchedulePart().IsScheduled())
							scheduleMatrixOriginalStateContainer.ScheduleMatrix.LockDay(day.Day);
					}
				}

				fixedStaffSchedulingService.DayScheduled += onDayScheduled;
				if (!scheduleRunCancelled)
				{
					fixedStaffSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, rollbackService);
				}
				_allResults().AddResults(fixedStaffSchedulingService.FinderResults, schedulingTime);
				fixedStaffSchedulingService.FinderResults.Clear();
				fixedStaffSchedulingService.DayScheduled -= onDayScheduled;

				if (!scheduleRunCancelled)
				{
					var nightRestWhiteSpotSolverService = _nightRestWhiteSpotSolverServiceFactory.Create(schedulingOptions.ConsiderShortBreaks);
					var progressChangeEvent = new TeleoptiProgressChangeMessage(Resources.TryingToResolveUnscheduledDaysDotDotDot);
					backgroundWorker.ReportProgress(0, progressChangeEvent);
					foreach (var scheduleMatrixOriginalStateContainer in originalStateContainers)
					{
						nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService);
					}
				}

				if (schedulingOptions.RotationDaysOnly || schedulingOptions.PreferencesDaysOnly ||
					schedulingOptions.UsePreferencesMustHaveOnly || schedulingOptions.AvailabilityDaysOnly)
					schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();
			}
		}

		public void ScheduleSelectedStudents(IEnumerable<IScheduleDay> allSelectedSchedules, ISchedulingProgress backgroundWorker,
			SchedulingOptions schedulingOptions)
		{
			if (allSelectedSchedules == null) throw new ArgumentNullException(nameof(allSelectedSchedules));
			if (schedulingOptions == null) throw new ArgumentNullException(nameof(schedulingOptions));

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

			var selectedPersons = new PersonListExtractorFromScheduleParts().ExtractPersons(unlockedSchedules);
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
				_studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, rollbackService);
			}

			_allResults().AddResults(_studentSchedulingService.FinderResults, schedulingTime);
			_studentSchedulingService.DayScheduled -= onDayScheduled;
		}
	}
}