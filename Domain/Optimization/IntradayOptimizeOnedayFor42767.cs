using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizeOnedayFor42767 : IIntradayOptimizeOneday
	{
		private readonly IScheduleService _scheduleService;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IOptimizationLimits _optimizationLimits;
		private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IScheduleMatrixPro _matrix;
		private readonly IIntradayOptimizeOneDayCallback _intradayOptimizeOneDayCallback;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public IntradayOptimizeOnedayFor42767(IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculation resourceOptimizationHelper,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IOptimizationLimits optimizationLimits,
			IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
			IScheduleMatrixPro matrix,
			IIntradayOptimizeOneDayCallback intradayOptimizeOneDayCallback,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone,
			IScheduleDayEquator scheduleDayEquator)
		{
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
			_matrix = matrix;
			_intradayOptimizeOneDayCallback = intradayOptimizeOneDayCallback;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_userTimeZone = userTimeZone;
			_scheduleDayEquator = scheduleDayEquator;
		}

		public bool Execute(DateOnly dateOnly)
		{
			_intradayOptimizeOneDayCallback.Optimizing(_matrix.Person, dateOnly);

			if (daysOverMax())
				return false;

			var lastOverLimitCounts = _optimizationLimits.OverLimitsCounts(_matrix);

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

			var originalShift = _workShiftOriginalStateContainer.OldPeriodDaysState[dateOnly].GetEditorShift();
			if (originalShift == null)
				return false;

			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizerPreferences, originalShift, dateOnly);
			var undoResCalcChanges = new UndoRedoContainer();
			foreach (var skillDay in _schedulingResultStateHolder.SkillDaysOnDateOnly(new[] { dateOnly.AddDays(-1), dateOnly, dateOnly.AddDays(1) }))
			{
				skillDay.SkillStaffPeriodCollection.ForEach(x => undoResCalcChanges.SaveState(x.Payload));
			}

			_rollbackService.ClearModificationCollection();

			var scheduleDayPro = _matrix.GetScheduleDayByKey(dateOnly);
			var scheduleDay = scheduleDayPro.DaySchedulePart();
			//var currentShift = scheduleDay.GetEditorShift();

			var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(scheduleDay, _rollbackService, schedulingOptions.ConsiderShortBreaks, false);

			if (!tryScheduleDay(dateOnly, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.AverageWorkTime))
			{
				undoResCalcChanges.UndoAll();
				return true;
			}

			if (_optimizationLimits.HasOverLimitExceeded(lastOverLimitCounts, _matrix) || daysOverMax())
			{
				_rollbackService.Rollback();
				undoResCalcChanges.UndoAll();
				lockDay(dateOnly);
				return false;
			}

			// Always lock days we moved
			lockDay(dateOnly);

			return true;
		}

		private void lockDay(DateOnly day)
		{
			_matrix.LockPeriod(new DateOnlyPeriod(day, day));
		}

		private bool tryScheduleDay(DateOnly day, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption)
		{
			IScheduleDayPro scheduleDay = _matrix.FullWeeksPeriodDictionary[day];
			schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder, _userTimeZone);

			if (!_scheduleService.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, effectiveRestriction, resourceCalculateDelayer, _rollbackService))
			{
				_rollbackService.Rollback();
				lockDay(day);
				return false;
			}

			//var newShift = scheduleDay.DaySchedulePart().ReFetch().GetEditorShift();
			//if (_scheduleDayEquator.MainShiftEquals(currentShift, newShift))
			//{
			//	_rollbackService.Rollback();
			//	lockDay(day);
			//	return false;
			//}

			if (!_workShiftOriginalStateContainer.WorkShiftChanged(day))
			{
				//should return false and lock?
				_rollbackService.Modify(_workShiftOriginalStateContainer.OldPeriodDaysState[day], new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
				lockDay(day);
				return false;
			}

			return true;
		}

		private bool daysOverMax()
		{
			return _optimizationLimits.MoveMaxDaysOverLimit();
		}
	}
}
