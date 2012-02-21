using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using log4net;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Move time optimization container, which contatins a logic to try to do one move on one matrix
    /// - Checks for old and new period value.
    /// - Reschedule moved days.
    /// - Checks for white spots.
    /// - Does rollback for the moved days if move is not successful. 
    /// - Manages temporary locks to unsuccessfull days
    /// </summary>
    public class MoveTimeOptimizer : IMoveTimeOptimizer
    {
        private readonly IPeriodValueCalculator _periodValueCalculator;
        private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private readonly IMoveTimeDecisionMaker _decisionMaker;
        private readonly IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private readonly IScheduleService _scheduleService;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IDeleteSchedulePartService _deleteService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IResourceCalculateDaysDecider _decider;
        private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
        private IOptimizationOverLimitDecider _optimizationOverLimitDecider;
        private readonly ISchedulingOptionsSyncronizer _schedulingOptionsSyncronizer;
        private readonly ILogWriter _log;

        public MoveTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            IMoveTimeDecisionMaker decisionMaker,
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDeleteSchedulePartService deleteService,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IResourceCalculateDaysDecider decider,
            IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
            IOptimizationOverLimitDecider optimizationOverLimitDecider, 
            ISchedulingOptionsSyncronizer schedulingOptionsSyncronizer)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
            _scheduleService = scheduleService;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _deleteService = deleteService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _decider = decider;
            _workShiftOriginalStateContainer = workShiftOriginalStateContainer;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _schedulingOptionsSyncronizer = schedulingOptionsSyncronizer;

            _log = new LogWriter<MoveTimeOptimizer>();
        }

        public bool Execute()
        {
            _schedulingOptionsSyncronizer.SyncronizeSchedulingOption(_optimizerPreferences, _scheduleService.SchedulingOptions);

            if (MovedDaysOverMaxDaysLimit())
                return false;

            double oldPeriodValue = calculatePeriodValue();

            // Step: get days to move
            IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor);

            if (daysToBeMoved.Count == 0)
                return false;

            _rollbackService.ClearModificationCollection();

            IScheduleDayPro firstDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(daysToBeMoved[0]);
            DateOnly firstDayDate = daysToBeMoved[0];
            IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();

            var firstDayEffectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(firstScheduleDay, _scheduleService.SchedulingOptions);

            IScheduleDayPro secondDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(daysToBeMoved[1]);
            DateOnly secondDayDate = daysToBeMoved[1];
            IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
            var secondDayEffectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(secondScheduleDay, _scheduleService.SchedulingOptions);

            if (firstDayDate == secondDayDate)
                return false;

            //delete schedule on the two days
            IList<IScheduleDay> listToDelete = new List<IScheduleDay> { firstDay.DaySchedulePart(), secondDay.DaySchedulePart() };

            IDictionary<DateOnly, changedDay> dic = new Dictionary<DateOnly, changedDay>();
            changedDay changedDay = new changedDay();
            changedDay.DateChanged = firstDay.Day;
            changedDay.PrevoiousSchedule = (IScheduleDay)firstDay.DaySchedulePart().Clone();
            dic.Add(changedDay.DateChanged, changedDay);
            changedDay = new changedDay
                             {
                                 DateChanged = secondDay.Day,
                                 PrevoiousSchedule = (IScheduleDay)secondDay.DaySchedulePart().Clone()
                             };
            dic.Add(changedDay.DateChanged, changedDay);

            var deleteOption = new DeleteOption { Default = true };
            using (var bgWorker = new BackgroundWorker())
            {
                _deleteService.Delete(listToDelete, deleteOption, _rollbackService, bgWorker);
            }
            
            foreach (changedDay changed in dic.Values)
            {
                changed.CurrentSchedule = _matrixConverter.SourceMatrix.GetScheduleDayByKey(changed.DateChanged).DaySchedulePart();
            }

            resourceCalculateMovedDays(dic.Values);

            if (!tryScheduleFirstDay(firstDayDate, firstDayEffectiveRestriction))
            {

                resourceCalculateDays(firstDayDate, secondDayDate);
                return true;
            }


            // Back to legal state
            // leave it for now

            if (!tryScheduleSecondDay(secondDayDate, secondDayEffectiveRestriction))
            {
                resourceCalculateDays(firstDayDate, secondDayDate);
                return true;
            }

            // Step: Check that there are no white spots

            double newPeriodValue = calculatePeriodValue();

            dic = new Dictionary<DateOnly, changedDay>();
            changedDay changedToRollback = new changedDay
                                               {
                                                   PrevoiousSchedule = (IScheduleDay)firstScheduleDay.Clone(),
                                                   DateChanged = firstDayDate
                                               };
            dic.Add(changedToRollback.DateChanged, changedToRollback);
            changedToRollback = new changedDay
                                    {
                                        PrevoiousSchedule = (IScheduleDay)secondScheduleDay.Clone(),
                                        DateChanged = secondDayDate
                                    };
            dic.Add(changedToRollback.DateChanged, changedToRollback);

            bool isPeriodWorse = newPeriodValue > oldPeriodValue;
            if (isPeriodWorse)
            {
                rollbackAndLockDays(firstDayDate, secondDayDate, dic);
                return true;
            }
            if (MovedDaysOverMaxDaysLimit())
            {
                rollbackAndLockDays(firstDayDate, secondDayDate, dic);
                return false;
            }

            lockDays(firstDayDate, secondDayDate);

            return true;
        }

        private void rollbackAndLockDays(DateOnly firstDayDate, DateOnly secondDayDate, IDictionary<DateOnly, changedDay> dic)
        {
            doRollback(firstDayDate, secondDayDate, dic);
            
            // Always lock days we moved
            lockDays(firstDayDate, secondDayDate);
        }

        private void lockDays(DateOnly firstDayDate, DateOnly secondDayDate)
        {
            lockDay(firstDayDate);
            lockDay(secondDayDate);
        }

        private void doRollback(DateOnly firstDayDate, DateOnly secondDayDate, IDictionary<DateOnly, changedDay> dic)
        {
            _rollbackService.Rollback();
            dic[firstDayDate].CurrentSchedule = _matrixConverter.SourceMatrix.GetScheduleDayByKey(firstDayDate).DaySchedulePart();
            dic[secondDayDate].CurrentSchedule = _matrixConverter.SourceMatrix.GetScheduleDayByKey(secondDayDate).DaySchedulePart();
            resourceCalculateMovedDays(dic.Values);
        }

        private void resourceCalculateDays(DateOnly firstDayDate, DateOnly secondDayDate)
        {
            bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
            _resourceOptimizationHelper.ResourceCalculateDate(firstDayDate, true, considerShortBreaks);
            _resourceOptimizationHelper.ResourceCalculateDate(firstDayDate.AddDays(1), true, considerShortBreaks);
            _resourceOptimizationHelper.ResourceCalculateDate(secondDayDate, true, considerShortBreaks);
            _resourceOptimizationHelper.ResourceCalculateDate(secondDayDate.AddDays(1), true, considerShortBreaks);
        }

        private void resourceCalculateMovedDays(IEnumerable<changedDay> changedDays)
        {
            bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
            foreach (changedDay changed in changedDays)
            {
                IList<DateOnly> days = _decider.DecideDates(changed.CurrentSchedule, changed.PrevoiousSchedule);
                foreach (var dateOnly in days)
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                }
            }
        }

        public bool MovedDaysOverMaxDaysLimit()
        {
            return _optimizationOverLimitDecider.OverLimit(_log);
        }

        public IPerson ContainerOwner
        {
            get { return _matrixConverter.SourceMatrix.Person; }
        }

        private double calculatePeriodValue()
        {
            return _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
        }

        private bool tryScheduleSecondDay(DateOnly secondDate, IEffectiveRestriction effectiveRestriction)
        {
            return tryScheduleDay(secondDate, effectiveRestriction, WorkShiftLengthHintOption.Short);
        }

        private bool tryScheduleFirstDay(DateOnly firstDate, IEffectiveRestriction effectiveRestriction)
        {
            return tryScheduleDay(firstDate, effectiveRestriction, WorkShiftLengthHintOption.Long);
        }

        private bool tryScheduleDay(DateOnly day, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption)
        {
            IScheduleDayPro scheduleDay = _matrixConverter.SourceMatrix.FullWeeksPeriodDictionary[day];
            _scheduleService.SchedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;

            if (!_scheduleService.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), false, effectiveRestriction))
            {
                _rollbackService.Rollback();
                lockDay(day);
                return false;
            }

            if (!_workShiftOriginalStateContainer.WorkShiftChanged(day))
            {
                _rollbackService.Modify(_workShiftOriginalStateContainer.OldPeriodDaysState[day], new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            }

            return true;
        }

        private void lockDay(DateOnly day)
        {
            _matrixConverter.SourceMatrix.LockPeriod(new DateOnlyPeriod(day, day));
        }

        private class changedDay
        {
            public DateOnly DateChanged { get; set; }
            public IScheduleDay PrevoiousSchedule { get; set; }
            public IScheduleDay CurrentSchedule { get; set; }
        }
    }
}
