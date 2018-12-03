using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

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
        private readonly ExtendReduceTimeDecisionMaker _decisionMaker;
        private readonly IScheduleService _scheduleServiceForFlexibleAgents;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IDeleteSchedulePartService _deleteService;
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly ScheduleChangesAffectedDates _decider;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
	    private readonly OptimizationLimits _optimizationLimits;
        private readonly SchedulingOptions _schedulingOptions;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
	    private readonly IScheduleMatrixPro _matrix;

	    public ExtendReduceTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            ExtendReduceTimeDecisionMaker decisionMaker,
            IScheduleService scheduleServiceForFlexibleAgents,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDeleteSchedulePartService deleteService,
            IResourceCalculateDelayer resourceCalculateDelayer,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            ScheduleChangesAffectedDates decider,
            IScheduleMatrixOriginalStateContainer originalStateContainerForTagChange,
 			OptimizationLimits optimizationLimits,
            SchedulingOptions schedulingOptions,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IScheduleMatrixPro matrix)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _scheduleServiceForFlexibleAgents = scheduleServiceForFlexibleAgents;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _deleteService = deleteService;
    	    _resourceCalculateDelayer = resourceCalculateDelayer;
    	    _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _decider = decider;
            _originalStateContainerForTagChange = originalStateContainerForTagChange;
    		_optimizationLimits = optimizationLimits;
            _schedulingOptions = schedulingOptions;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
    		_matrix = matrix;
        }

        public bool Execute()
        {
	        var lastOverLimitCount = _optimizationLimits.OverLimitsCounts(_matrix);

            if (daysOverMax())
                return false;

            bool sucess = false;

            ExtendReduceTimeDecisionMakerResult daysToBeRescheduled = _decisionMaker.Execute(_matrix, _personalSkillsDataExtractor);

            if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
                return false;

            if(daysToBeRescheduled.DayToLengthen.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToLengthen.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Long, _schedulingOptions, dateOnly, _matrix, lastOverLimitCount))
                    sucess = true;
            }

            if (daysToBeRescheduled.DayToShorten.HasValue)
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToShorten.Value;

                if (rescheduleAndCheckPeriodValue(WorkShiftLengthHintOption.Short, _schedulingOptions, dateOnly, _matrix, lastOverLimitCount))
                    sucess = true;
            }

            return sucess;
        }

        public IPerson Owner => _matrix.Person;

	    private bool rescheduleAndCheckPeriodValue(
            WorkShiftLengthHintOption lenghtHint, 
            SchedulingOptions schedulingOptions,
            DateOnly dateOnly,
            IScheduleMatrixPro matrix, OverLimitResults lastOverLimitResults)
        {
            double oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            _rollbackService.ClearModificationCollection();

            IScheduleDay scheduleDayBefore =
                (IScheduleDay)matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            deleteDay(dateOnly);
            IScheduleDay scheduleDayAfter =
               (IScheduleDay)matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();

            var daysToRecalculate = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);
            foreach (var dateToRecalculate in daysToRecalculate)
            {
                _resourceCalculateDelayer.CalculateIfNeeded(dateToRecalculate, null, false);
            }

            matrix.LockDay(dateOnly);
            if (!tryScheduleDay(dateOnly, schedulingOptions, lenghtHint))
            {
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
                return false;
            }

            if(daysOverMax())
            {
                rollbackAndResourceCalculate(dateOnly);
                return false;
            }

	        if (_optimizationLimits.HasOverLimitExceeded(lastOverLimitResults, _matrix))
	        {
				rollbackAndResourceCalculate(dateOnly);
				return true;   
	        }

			var minWorkTimePerWeekOk = _optimizationLimits.ValidateMinWorkTimePerWeek(_matrix);

			if (!minWorkTimePerWeekOk)
			{
				rollbackAndResourceCalculate(dateOnly);
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
            IScheduleDay scheduleDayBefore = (IScheduleDay) _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            _rollbackService.Rollback();
            IScheduleDay scheduleDayAfter = (IScheduleDay) _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().Clone();
            var days = _decider.DecideDates(scheduleDayAfter, scheduleDayBefore);

            foreach (var date in days)
            {
                _resourceCalculateDelayer.CalculateIfNeeded(date, null, false);
            }
        }

        private void deleteDay(DateOnly dateOnly)
        {
            var deleteOption = new DeleteOption { Default = true };
            var scheduleDayPro = _matrix.GetScheduleDayByKey(dateOnly);
            var scheduleDay = scheduleDayPro.DaySchedulePart();

	        _deleteService.Delete(new List<IScheduleDay> {scheduleDay}, deleteOption, _rollbackService, new NoSchedulingProgress());
        }

        private bool tryScheduleDay(DateOnly day, SchedulingOptions schedulingOptions, WorkShiftLengthHintOption workShiftLengthHintOption)
        {
            IScheduleDayPro scheduleDay = _matrix.GetScheduleDayByKey(day);
            schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;
            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay.DaySchedulePart(), schedulingOptions);

	        var originalShift = _originalStateContainerForTagChange.OldPeriodDaysState[day].GetEditorShift();
			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizerPreferences, originalShift, day);

			if (!_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService))
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

        private bool daysOverMax()
        {
	        return _optimizationLimits.MoveMaxDaysOverLimit();
        }
    }
}