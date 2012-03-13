using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IExtendReduceTimeOptimizer
    {
        bool Execute();
        IPerson Owner { get; }
    }

    public class ExtendReduceTimeOptimizer : IExtendReduceTimeOptimizer
    {
        private readonly IPeriodValueCalculator _periodValueCalculator;
        private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private readonly IExtendReduceTimeDecisionMaker _decisionMaker;
        private readonly IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private readonly IScheduleService _scheduleServiceForFlexibleAgents;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IDeleteSchedulePartService _deleteService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IResourceCalculateDaysDecider _decider;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private readonly IOptimizationOverLimitDecider _optimizationOverLimitDecider;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;

        public ExtendReduceTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            IExtendReduceTimeDecisionMaker decisionMaker,
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
            IScheduleService scheduleServiceForFlexibleAgents,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDeleteSchedulePartService deleteService,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IResourceCalculateDaysDecider decider,
            IScheduleMatrixOriginalStateContainer originalStateContainerForTagChange, 
            IOptimizationOverLimitDecider optimizationOverLimitDecider, 
            ISchedulingOptionsCreator schedulingOptionsCreator)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
            _scheduleServiceForFlexibleAgents = scheduleServiceForFlexibleAgents;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _deleteService = deleteService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _decider = decider;
            _originalStateContainerForTagChange = originalStateContainerForTagChange;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        }

        public bool Execute()
        {
            if (movedDaysOverOrAtMaxDaysLimit())
                return false;

            bool sucess = false;

            ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);

            ExtendReduceTimeDecisionMakerResult daysToBeRescheduled = _decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor);

            if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
                return false;

            bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
            if(daysToBeRescheduled.DayToLengthen.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToLengthen.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Long, schedulingOptions, dateOnly, considerShortBreaks))
                    sucess = true;
            }

            if (daysToBeRescheduled.DayToShorten.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToShorten.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Short, schedulingOptions,  dateOnly, considerShortBreaks))
                    sucess = true;
            }

            

            return sucess;
        }

        public IPerson Owner
        {
            get { return _matrixConverter.SourceMatrix.Person; }
        }

        private bool rescheduleAndCheckPeriodValue(
            WorkShiftLengthHintOption lenghtHint, 
            ISchedulingOptions schedulingOptions,
            DateOnly dateOnly,
            bool considerShortBreaks)
        {
            double oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            _rollbackService.ClearModificationCollection();

            IScheduleDay scheduleDayBefore =
                (IScheduleDay)_matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            deleteDay(dateOnly);
            IScheduleDay scheduleDayAfter =
               (IScheduleDay)_matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();

            IList<DateOnly> daysToRecalculate = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);
            foreach (var dateToRecalculate in daysToRecalculate)
            {
                _resourceOptimizationHelper.ResourceCalculateDate(dateToRecalculate, true, considerShortBreaks);
            }

            _matrixConverter.SourceMatrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
            if (!tryScheduleDay(dateOnly, schedulingOptions, lenghtHint))
            {
                _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                return false;
            }

            double newPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            if (newPeriodValue > oldPeriodValue)
            {
                scheduleDayBefore =
                (IScheduleDay)_matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
                _rollbackService.Rollback();
                scheduleDayAfter =
               (IScheduleDay)_matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
                IList<DateOnly> days = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);
                foreach (var date in days)
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(date, true, considerShortBreaks);

                }
                return false;
            }

            return true;
        }

        private void deleteDay(DateOnly dateOnly)
        {
            var deleteOption = new DeleteOption { Default = true };
            var scheduleDayPro = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly);
            var scheduleDay = scheduleDayPro.DaySchedulePart();

            using (var bgWorker = new BackgroundWorker())
            {
                _deleteService.Delete(new List<IScheduleDay> { scheduleDay }, deleteOption, _rollbackService, bgWorker);
            }
        }

        private bool tryScheduleDay(DateOnly day, ISchedulingOptions schedulingOptions, WorkShiftLengthHintOption workShiftLengthHintOption)
        {
            IScheduleDayPro scheduleDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(day);
            schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay.DaySchedulePart(), schedulingOptions);

            if (!_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, false, effectiveRestriction))
            {
                _rollbackService.Rollback();
                return false;
            }

            if (!_originalStateContainerForTagChange.WorkShiftChanged(day))
            {
                _rollbackService.Modify(_originalStateContainerForTagChange.OldPeriodDaysState[day], new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
            }

            return true;
        }

        private bool movedDaysOverOrAtMaxDaysLimit()
        {
            return _optimizationOverLimitDecider.OverLimit();
        }
    }
}