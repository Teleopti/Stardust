using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Intraday optimization container, which contatins a logic to try to do one move on one matrix
    /// - Checks for old and new period value.
    /// - Reschedule moved days.
    /// - Checks for white spots.
    /// - Does rollback for the moved days if move is not successful. 
    /// - Manages temporary locks to unsuccessfull days
    /// </summary>
    public class IntradayOptimizer2 : IIntradayOptimizer2
    {

        private readonly IScheduleResultDailyValueCalculator _dailyValueCalculator;
        private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private readonly IIntradayDecisionMaker _decisionMaker;
        private readonly IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private readonly IScheduleService _scheduleService;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
        private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;

        public IntradayOptimizer2(
            IScheduleResultDailyValueCalculator dailyValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor, 
            IIntradayDecisionMaker decisionMaker, 
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
			IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences, 
            ISchedulePartModifyAndRollbackService rollbackService,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider, 
            IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer, 
            ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
            IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
            IResourceCalculateDelayer resourceCalculateDelayer)
        {
            _dailyValueCalculator = dailyValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
			_scheduleService = scheduleService;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _workShiftOriginalStateContainer = workShiftOriginalStateContainer;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
    	    _deleteAndResourceCalculateService = deleteAndResourceCalculateService;
            _resourceCalculateDelayer = resourceCalculateDelayer;
        }

        public bool Execute()
        {
            if (RestrictionsOverMax().Count > 0 || daysOverMax())
                return false;

            // Step: get day to move
            DateOnly? dayToBeMoved = _decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor);
            if (!dayToBeMoved.HasValue)
                return false;

        	DateOnly dateToBeRemoved = dayToBeMoved.Value;
            double? oldPeriodValue = calculatePeriodValue(dateToBeRemoved);
            if (!oldPeriodValue.HasValue)
                return false;

            ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

			var originalShift = _workShiftOriginalStateContainer.OldPeriodDaysState[dateToBeRemoved].GetEditorShift();
			if (originalShift == null) return false;

			_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences, originalShift, dateToBeRemoved);

            _rollbackService.ClearModificationCollection();

			IScheduleDayPro scheduleDayPro = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateToBeRemoved);
        	IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();

            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay, schedulingOptions);
            
            //delete schedule
			IList<IScheduleDay> listToDelete = new List<IScheduleDay> { scheduleDay };    
            var changed = new changedDay
                              {
                                  PrevoiousSchedule = (IScheduleDay) scheduleDay.Clone()
                              };
            
            _deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, _rollbackService, schedulingOptions.ConsiderShortBreaks);

            changed.CurrentSchedule = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateToBeRemoved).DaySchedulePart();
            
            if (!tryScheduleDay(dateToBeRemoved, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.AverageWorkTime)) 
                return true;

            // Step: Check that there are no white spots

            double newValidatedPeriodValue = double.MaxValue;
			double? newPeriodValue = calculatePeriodValue(dateToBeRemoved);
            if (newPeriodValue.HasValue)
                newValidatedPeriodValue = newPeriodValue.Value;

            changed = new changedDay();
            changed.PrevoiousSchedule = (IScheduleDay)_matrixConverter.SourceMatrix.GetScheduleDayByKey(dateToBeRemoved).DaySchedulePart().Clone();

            bool isPeriodWorse = newValidatedPeriodValue > oldPeriodValue;
            if (isPeriodWorse)
            {
                _rollbackService.Rollback();
                changed.CurrentSchedule =
                    _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateToBeRemoved).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateToBeRemoved, null);
                lockDay(dateToBeRemoved);
                return true;
            }

            if (RestrictionsOverMax().Count > 0 || daysOverMax())
            {
                _rollbackService.Rollback();
                changed.CurrentSchedule =
                    _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateToBeRemoved).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateToBeRemoved, null);
                lockDay(dateToBeRemoved);
                return false;
            }

            // Always lock days we moved
			lockDay(dateToBeRemoved);

            return true;
        }

        public IList<DateOnly> RestrictionsOverMax()
        {
            return _optimizationOverLimitDecider.OverLimit();
        }

        private bool daysOverMax()
        {
            return _optimizationOverLimitDecider.MoveMaxDaysOverLimit();
        }

        public IPerson ContainerOwner
        {
            get { return _matrixConverter.SourceMatrix.Person; }
        }

        private double? calculatePeriodValue(DateOnly scheduleDay)
        {
            return _dailyValueCalculator.DayValue(scheduleDay);
        }

        private bool tryScheduleDay(DateOnly day, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption)
        {
            IScheduleDayPro scheduleDay = _matrixConverter.SourceMatrix.FullWeeksPeriodDictionary[day];
            schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);

			if (!_scheduleService.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, effectiveRestriction, resourceCalculateDelayer, null, _rollbackService))
            {
				var days = _rollbackService.ModificationCollection.ToList();
                _rollbackService.Rollback();
				foreach (var schedDay in days)
				{
					var dateOnly = schedDay.DateOnlyAsPeriod.DateOnly;
                    resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				}
                lockDay(day);
                return false;
            }

            if (!_workShiftOriginalStateContainer.WorkShiftChanged(day))
            {
                _rollbackService.Modify( _workShiftOriginalStateContainer.OldPeriodDaysState[day], new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            }

            return true;
        }

        private void lockDay(DateOnly day)
        {
            _matrixConverter.SourceMatrix.LockPeriod(new DateOnlyPeriod(day, day));
        }

        private class changedDay
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IScheduleDay PrevoiousSchedule { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IScheduleDay CurrentSchedule { get; set; }
        }
    }
}
