using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.DayOffPlanning;
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
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IResourceCalculateDaysDecider _decider;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

    	public ExtendReduceTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            IExtendReduceTimeDecisionMaker decisionMaker,
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
            IScheduleService scheduleServiceForFlexibleAgents,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDeleteSchedulePartService deleteService,
            IResourceCalculateDelayer resourceCalculateDelayer,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IResourceCalculateDaysDecider decider,
            IScheduleMatrixOriginalStateContainer originalStateContainerForTagChange,
            IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider, 
            ISchedulingOptions schedulingOptions,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
            _scheduleServiceForFlexibleAgents = scheduleServiceForFlexibleAgents;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _deleteService = deleteService;
    	    _resourceCalculateDelayer = resourceCalculateDelayer;
    	    _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _decider = decider;
            _originalStateContainerForTagChange = originalStateContainerForTagChange;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _schedulingOptions = schedulingOptions;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
        }

        public bool Execute()
        {
            if (restrictionsOverMax().Count > 0 || daysOverMax())
                return false;

            bool sucess = false;

            ExtendReduceTimeDecisionMakerResult daysToBeRescheduled = _decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor);

            if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
                return false;

            if(daysToBeRescheduled.DayToLengthen.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToLengthen.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Long, _schedulingOptions, dateOnly, _matrixConverter.SourceMatrix))
                    sucess = true;
            }

            if (daysToBeRescheduled.DayToShorten.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToShorten.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Short, _schedulingOptions, dateOnly, _matrixConverter.SourceMatrix))
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
            IScheduleMatrixPro matrix)
        {
            double oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            _rollbackService.ClearModificationCollection();

            IScheduleDay scheduleDayBefore =
                (IScheduleDay)matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            deleteDay(dateOnly);
            IScheduleDay scheduleDayAfter =
               (IScheduleDay)matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();

            IList<DateOnly> daysToRecalculate = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);
            foreach (var dateToRecalculate in daysToRecalculate)
            {
                _resourceCalculateDelayer.CalculateIfNeeded(dateToRecalculate, null);
            }

            matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
            if (!tryScheduleDay(dateOnly, schedulingOptions, lenghtHint))
            {
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
                return false;
            }

            if(daysOverMax())
            {
                rollbackAndResourceCalculate(dateOnly);
                return false;
            }

            IList<DateOnly> daysToLock = restrictionsOverMax();
            if (daysToLock.Count > 0)
            {
                rollbackAndResourceCalculate(dateOnly);

                foreach (var date in daysToLock)
                {
                    matrix.LockPeriod(new DateOnlyPeriod(date, date));
                }
                return true;
            }

            double newPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            if (newPeriodValue > oldPeriodValue)
            {
                rollbackAndResourceCalculate(dateOnly);
                return false;
            }

            return true;
        }

        private void rollbackAndResourceCalculate(DateOnly dateOnly)
        {
            IScheduleDay scheduleDayBefore = (IScheduleDay) _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            _rollbackService.Rollback();
            IScheduleDay scheduleDayAfter = (IScheduleDay) _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            IList<DateOnly> days = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);

            foreach (var date in days)
            {
                _resourceCalculateDelayer.CalculateIfNeeded(date, null);
            }
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

	        var originalShift = _originalStateContainerForTagChange.OldPeriodDaysState[day].GetEditorShift();
			_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences, originalShift, day);

			if (!_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService))
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

        private IList<DateOnly> restrictionsOverMax()
        {
            return _optimizationOverLimitDecider.OverLimit();
        }

        private bool daysOverMax()
        {
            return _optimizationOverLimitDecider.MoveMaxDaysOverLimit();
        }
    }
}