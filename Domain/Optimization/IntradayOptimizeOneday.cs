using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizeOneday
	{
		private readonly IScheduleResultDailyValueCalculator _dailyValueCalculator;
		private readonly IScheduleService _scheduleService;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IOptimizationLimits _optimizationLimits;
		private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
		private readonly IScheduleMatrixPro _matrix;

		public IntradayOptimizeOneday(IScheduleResultDailyValueCalculator dailyValueCalculator,
			IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IOptimizationLimits optimizationLimits,
			IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IScheduleMatrixPro matrix)
		{
			_dailyValueCalculator = dailyValueCalculator;
			_scheduleService = scheduleService;
			_optimizerPreferences = optimizerPreferences;
			_rollbackService = rollbackService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_optimizationLimits = optimizationLimits;
			_workShiftOriginalStateContainer = workShiftOriginalStateContainer;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_resourceCalculateDelayer = resourceCalculateDelayer;
			_matrix = matrix;
		}

		public bool Execute(DateOnly dateToBeRemoved)
		{
			double? oldPeriodValue = calculatePeriodValue(dateToBeRemoved);

			var lastOverLimitCounts = _optimizationLimits.OverLimitsCounts(_matrix);

			if (!oldPeriodValue.HasValue)
				return false;

			ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

			var originalShift = _workShiftOriginalStateContainer.OldPeriodDaysState[dateToBeRemoved].GetEditorShift();
			if (originalShift == null) return false;

			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizerPreferences, originalShift, dateToBeRemoved);

			_rollbackService.ClearModificationCollection();

			IScheduleDayPro scheduleDayPro = _matrix.GetScheduleDayByKey(dateToBeRemoved);
			IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();

			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);

			//delete schedule
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(scheduleDay, _rollbackService, schedulingOptions.ConsiderShortBreaks, false);

			if (!tryScheduleDay(dateToBeRemoved, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.AverageWorkTime))
				return true;

			// Step: Check that there are no white spots

			double newValidatedPeriodValue = double.MaxValue;
			double? newPeriodValue = calculatePeriodValue(dateToBeRemoved);
			if (newPeriodValue.HasValue)
				newValidatedPeriodValue = newPeriodValue.Value;

			bool isPeriodWorse = newValidatedPeriodValue > oldPeriodValue;
			if (isPeriodWorse)
			{
				_rollbackService.Rollback();
				_resourceCalculateDelayer.CalculateIfNeeded(dateToBeRemoved, null, false);
				lockDay(dateToBeRemoved);
				return true;
			}

			if (_optimizationLimits.HasOverLimitExceeded(lastOverLimitCounts, _matrix) || daysOverMax())
			{
				_rollbackService.Rollback();
				_resourceCalculateDelayer.CalculateIfNeeded(dateToBeRemoved, null, false);
				lockDay(dateToBeRemoved);
				return false;
			}

			// Always lock days we moved
			lockDay(dateToBeRemoved);

			return true;
		}

		private double? calculatePeriodValue(DateOnly scheduleDay)
		{
			return _dailyValueCalculator.DayValue(scheduleDay);
		}

		private void lockDay(DateOnly day)
		{
			_matrix.LockPeriod(new DateOnlyPeriod(day, day));
		}

		private bool tryScheduleDay(DateOnly day, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption)
		{
			IScheduleDayPro scheduleDay = _matrix.FullWeeksPeriodDictionary[day];
			schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);

			if (!_scheduleService.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, effectiveRestriction, resourceCalculateDelayer, _rollbackService))
			{
				var days = _rollbackService.ModificationCollection.ToList();
				_rollbackService.Rollback();
				foreach (var schedDay in days)
				{
					var dateOnly = schedDay.DateOnlyAsPeriod.DateOnly;
					resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
				}
				lockDay(day);
				return false;
			}

			if (!_workShiftOriginalStateContainer.WorkShiftChanged(day))
			{
				_rollbackService.Modify(_workShiftOriginalStateContainer.OldPeriodDaysState[day], new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			}

			return true;
		}

		private bool daysOverMax()
		{
			return _optimizationLimits.MoveMaxDaysOverLimit();
		}
	}
}