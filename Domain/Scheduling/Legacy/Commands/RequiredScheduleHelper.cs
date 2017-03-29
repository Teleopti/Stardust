using System;
using System.Collections.Generic;
using System.Linq;
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
			ISchedulingOptions schedulingOptions);

		void RemoveShiftCategoryBackToLegalState(
			IList<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions, DateOnlyPeriod selectedPeriod, IList<IScheduleMatrixPro> allMatrixes);

		void ScheduleSelectedPersonDays(IEnumerable<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, ISchedulingOptions schedulingOptions);
	}

	public class RequiredScheduleHelper : IRequiredScheduleHelper
	{
		private readonly ISchedulePeriodListShiftCategoryBackToLegalStateService _shiftCategoryBackToLegalState;
		private readonly IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly IStudentSchedulingService _studentSchedulingService;
		private readonly Func<IOptimizationPreferences> _optimizationPreferences;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IGridlockManager _gridlockManager;
		private readonly IDaysOffSchedulingService _daysOffSchedulingService;
		private readonly Func<IWorkShiftFinderResultHolder> _allResults;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly TeamBlockRetryRemoveShiftCategoryBackToLegalService _teamBlockRemoveShiftCategoryBackToLegalService;
		private readonly INightRestWhiteSpotSolverServiceFactory _nightRestWhiteSpotSolverServiceFactory;
		private readonly IUserTimeZone _userTimeZone;


		public RequiredScheduleHelper(ISchedulePeriodListShiftCategoryBackToLegalStateService shiftCategoryBackToLegalState, 
				IRuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak, 
				Func<ISchedulingResultStateHolder> resultStateHolder, 
				Func<IFixedStaffSchedulingService> fixedStaffSchedulingService, 
				IStudentSchedulingService studentSchedulingService, 
				Func<IOptimizationPreferences> optimizationPreferences, 
				IResourceCalculation resourceOptimizationHelper, 
				IGridlockManager gridlockManager, 
				IDaysOffSchedulingService daysOffSchedulingService, 
				Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, 
				IScheduleDayChangeCallback scheduleDayChangeCallback, 
				IScheduleDayEquator scheduleDayEquator, 
				IMatrixListFactory matrixListFactory,
				TeamBlockRetryRemoveShiftCategoryBackToLegalService teamBlockRemoveShiftCategoryBackToLegalService,
				INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory,
				IUserTimeZone userTimeZone)
		{
			_shiftCategoryBackToLegalState = shiftCategoryBackToLegalState;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_resultStateHolder = resultStateHolder;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_studentSchedulingService = studentSchedulingService;
			_optimizationPreferences = optimizationPreferences;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_gridlockManager = gridlockManager;
			_daysOffSchedulingService = daysOffSchedulingService;
			_allResults = workShiftFinderResultHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_scheduleDayEquator = scheduleDayEquator;
			_matrixListFactory = matrixListFactory;
			_teamBlockRemoveShiftCategoryBackToLegalService = teamBlockRemoveShiftCategoryBackToLegalService;
			_nightRestWhiteSpotSolverServiceFactory = nightRestWhiteSpotSolverServiceFactory;
			_userTimeZone = userTimeZone;
		}

		public void RemoveShiftCategoryBackToLegalState(
			IList<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
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

				if (schedulingOptions.UseBlock || schedulingOptions.UseTeam)
				{
					var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _resultStateHolder(), _userTimeZone);
					foreach (var matrix in matrixList)
					{
						backgroundWorker.ReportProgress(0, new TeleoptiProgressChangeMessage(Resources.TryingToResolveShiftCategoryLimitationsDotDotDot));
						_teamBlockRemoveShiftCategoryBackToLegalService.Execute(schedulingOptions, matrix, _resultStateHolder(), resourceCalculateDelayer, matrixList, optimizationPreferences, allMatrixes);
					}
				}
				else
				{
					_shiftCategoryBackToLegalState.Execute(matrixList, schedulingOptions, optimizationPreferences);
				}
			}
		}

		public void ScheduleSelectedPersonDays(IEnumerable<IScheduleDay> allSelectedSchedules, IList<IScheduleMatrixPro> matrixList,
			ISchedulingProgress backgroundWorker, ISchedulingOptions schedulingOptions)
		{
			if (matrixList == null) throw new ArgumentNullException("matrixList");

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;

			var selectedPeriod = allSelectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());
			//lock none selected days
			var matrixUnselectedDaysLocker = new MatrixUnselectedDaysLocker(matrixList, period);
			matrixUnselectedDaysLocker.Execute();
			var unlockedSchedules = (from scheduleMatrixPro in matrixList
									 from scheduleDayPro in scheduleMatrixPro.UnlockedDays
									 select scheduleDayPro.DaySchedulePart()).ToList();

			if (!unlockedSchedules.Any()) return;

			var selectedPersons = matrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).ToList();
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
				foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixListForSelection(_resultStateHolder().Schedules, allSelectedSchedules))
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
						var iterations = 0;
						while (nightRestWhiteSpotSolverService.Resolve(scheduleMatrixOriginalStateContainer.ScheduleMatrix, schedulingOptions, rollbackService) && iterations < 10)
						{
							iterations++;
						}
					}
				}

				if (schedulingOptions.RotationDaysOnly || schedulingOptions.PreferencesDaysOnly ||
					schedulingOptions.UsePreferencesMustHaveOnly || schedulingOptions.AvailabilityDaysOnly)
					schedulePartModifyAndRollbackServiceForContractDaysOff.Rollback();
			}
		}

		public void ScheduleSelectedStudents(IEnumerable<IScheduleDay> allSelectedSchedules, ISchedulingProgress backgroundWorker,
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