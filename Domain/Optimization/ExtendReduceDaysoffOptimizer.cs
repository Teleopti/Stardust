using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IExtendReduceDaysOffOptimizer
    {
        bool Execute();
        IPerson Owner { get; }
    }

    public class ExtendReduceDaysOffOptimizer : IExtendReduceDaysOffOptimizer
    {
        private readonly IPeriodValueCalculator _periodValueCalculator;
        private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private readonly IExtendReduceDaysOffDecisionMaker _decisionMaker;
        private readonly IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private readonly IScheduleService _scheduleServiceForFlexibleAgents;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IResourceCalculateDaysDecider _decider;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private readonly IWorkShiftBackToLegalStateServicePro _workTimeBackToLegalStateService;
        private readonly INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
        private readonly IDayOffTemplate _dayOffTemplate;
        private readonly IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private readonly ISchedulingOptionsSynchronizer _schedulingOptionsSynchronizer;

        public ExtendReduceDaysOffOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            IExtendReduceDaysOffDecisionMaker decisionMaker,
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
            IScheduleService scheduleServiceForFlexibleAgents,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IResourceCalculateDaysDecider decider,
            IScheduleMatrixOriginalStateContainer originalStateContainerForTagChange,
            IWorkShiftBackToLegalStateServicePro workTimeBackToLegalStateService,
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService,
            IList<IDayOffLegalStateValidator> validatorList,
            IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
            IDayOffTemplate dayOffTemplate,
            IDayOffOptimizerConflictHandler dayOffOptimizerConflictHandler,
            IDayOffOptimizerValidator dayOffOptimizerValidator, 
            ISchedulingOptionsSynchronizer schedulingOptionsSynchronizer)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
            _scheduleServiceForFlexibleAgents = scheduleServiceForFlexibleAgents;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _decider = decider;
            _originalStateContainerForTagChange = originalStateContainerForTagChange;
            _workTimeBackToLegalStateService = workTimeBackToLegalStateService;
            _nightRestWhiteSpotSolverService = nightRestWhiteSpotSolverService;
            _validatorList = validatorList;
            _dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
            _dayOffTemplate = dayOffTemplate;
            _dayOffOptimizerConflictHandler = dayOffOptimizerConflictHandler;
            _dayOffOptimizerValidator = dayOffOptimizerValidator;
            _schedulingOptionsSynchronizer = schedulingOptionsSynchronizer;
        }

        public bool Execute()
        {
            _rollbackService.ClearModificationCollection();

            var schedulePeriod = _matrixConverter.SourceMatrix.SchedulePeriod;
            int targetDaysoff;
            int currentDaysoff;
            if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysoff, out currentDaysoff))
                return false;

            bool success = false;

            var schedulingOptions = new SchedulingOptions();
            var sourceMatrix = _matrixConverter.SourceMatrix;

            _schedulingOptionsSynchronizer.SynchronizeSchedulingOption(_optimizerPreferences, schedulingOptions);

            ExtendReduceTimeDecisionMakerResult daysToBeRescheduled =
                _decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor, _validatorList);

            if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
                return false;

            var oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);

            if (daysToBeRescheduled.DayToLengthen.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(schedulePeriod))
            {

                DateOnly dateOnly = daysToBeRescheduled.DayToLengthen.Value;

                changedDay changedDayOff = new changedDay();
                var currentPart = sourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
                
                changedDayOff.PrevoiousSchedule = currentPart;
                changedDayOff.DateChanged = dateOnly;
               
                currentPart.DeleteDayOff();
                _rollbackService.Modify(currentPart);

                changedDayOff.CurrentSchedule = sourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();

                IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(sourceMatrix);  //resource calculation is done automaticaly

                if (rescheduleWhiteSpots(new[] { changedDayOff }, illegalDays, sourceMatrix, _originalStateContainerForTagChange, schedulingOptions))
                    success = true;
                else
                    sourceMatrix.LockPeriod(new DateOnlyPeriod(daysToBeRescheduled.DayToLengthen.Value, daysToBeRescheduled.DayToLengthen.Value));
            }

            if (daysToBeRescheduled.DayToShorten.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(schedulePeriod))
            {
                DateOnly dateOnly = daysToBeRescheduled.DayToShorten.Value;
                if (addDayOff(dateOnly, true, schedulingOptions))
                    success = true;
                else
                    sourceMatrix.LockPeriod(new DateOnlyPeriod(daysToBeRescheduled.DayToShorten.Value, daysToBeRescheduled.DayToShorten.Value));
            }

            if(success)
            {
                // todo ***** change it to if period value better form
                var currentPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
                if (currentPeriodValue > oldPeriodValue)
                {
                    IList<DateOnly> toResourceCalculate = _rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
                    _rollbackService.Rollback();
                    foreach (DateOnly dateOnly1 in toResourceCalculate)
                    {
                        bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly1, true, considerShortBreaks);
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly1.AddDays(1), true, considerShortBreaks);
                    }
                    return false;
                }
            }
            
                
            return success;
        }

        public IPerson Owner
        {
            get { return _matrixConverter.SourceMatrix.Person; }
        }

        private bool rescheduleWhiteSpots(
            IEnumerable<changedDay> movedDates, 
            IEnumerable<DateOnly> removedIllegalWorkTimeDays, 
            IScheduleMatrixPro matrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            ISchedulingOptions schedulingOptions)
        {
            var toSchedule = movedDates.Select(changedDay => changedDay.DateChanged).ToList();
            toSchedule.AddRange(removedIllegalWorkTimeDays);
            toSchedule.Sort();
            foreach (DateOnly dateOnly in toSchedule)
            {

                IScheduleDay schedulePart = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();

                IShiftCategory originalShiftCategory = null;
                IScheduleDay originalScheduleDay = originalStateContainer.OldPeriodDaysState[dateOnly];
                IPersonAssignment originalPersonAssignment = originalScheduleDay.AssignmentHighZOrder();
                if (originalPersonAssignment != null)
                {
                    IMainShift originalMainShift = originalPersonAssignment.MainShift;
                    if (originalMainShift != null)
                        originalShiftCategory = originalMainShift.ShiftCategory;
                }

                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);

                bool schedulingResult;
                if (effectiveRestriction.ShiftCategory == null && originalShiftCategory != null)
                {
                    schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, true, originalShiftCategory);
                    if (!schedulingResult)
                        schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, true, effectiveRestriction);
                }
                else
                    schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, true, effectiveRestriction);

                if (!schedulingResult)
                {
                    int iterations = 0;
                    while (_nightRestWhiteSpotSolverService.Resolve(matrix, schedulingOptions) && iterations < 10)
                    {
                        iterations++;
                    }

                    if (originalStateContainer.IsFullyScheduled())
                        return true;

                    IList<DateOnly> toResourceCalculate = _rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
                    _rollbackService.Rollback();
                    foreach (DateOnly dateOnly1 in toResourceCalculate)
                    {
                        bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly1, true, considerShortBreaks);
                        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly1.AddDays(1), true, considerShortBreaks);
                    }
                    return false;
                }
            }

            return true;
        }


        private bool addDayOff(DateOnly dateOnly, bool handleConflict, ISchedulingOptions schedulingOptions)
        {
            var matrix = _matrixConverter.SourceMatrix;
            var currentPart = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
            changedDay changed = new changedDay();
            changed.DateChanged = dateOnly;
            changed.PrevoiousSchedule = currentPart;

            currentPart.DeleteMainShift(currentPart);
            currentPart.CreateAndAddDayOff(_dayOffTemplate);
            _rollbackService.Modify(currentPart);

            changed.CurrentSchedule = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
            resourceCalculateMovedDays(new[] { changed });

            IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(_matrixConverter.SourceMatrix);  //resource calculation is done automaticaly

            if (!rescheduleWhiteSpots(new List<changedDay>(), illegalDays, _matrixConverter.SourceMatrix, _originalStateContainerForTagChange, schedulingOptions))
                return false;

            if (handleConflict && !_dayOffOptimizerValidator.Validate(currentPart.DateOnlyAsPeriod.DateOnly, matrix))
            {
                //if day off rule validation fails, try to reschedule day before and day after
                return _dayOffOptimizerConflictHandler.HandleConflict(schedulingOptions, currentPart.DateOnlyAsPeriod.DateOnly);
            }

            return true;
        }

        private void resourceCalculateMovedDays(IEnumerable<changedDay> changedDays)
        {
            foreach (changedDay changed in changedDays)
            {
                IList<DateOnly> days = _decider.DecideDates(changed.CurrentSchedule, changed.PrevoiousSchedule);
                foreach (var dateOnly in days)
                {
                    bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                }
            }
        }

        private IList<DateOnly> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix)
        {
            _workTimeBackToLegalStateService.Execute(matrix);
            IList<DateOnly> removedIllegalDates = _workTimeBackToLegalStateService.RemovedDays;
            //resource calculate removed days
            foreach (DateOnly dateOnly in removedIllegalDates)
            {
                bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
                _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                _resourceOptimizationHelper.ResourceCalculateDate(dateOnly.AddDays(1), true, considerShortBreaks);
            }

            return removedIllegalDates;
        }

        private class changedDay
        {
            public DateOnly DateChanged { get; set; }
            public IScheduleDay PrevoiousSchedule { get; set; }
            public IScheduleDay CurrentSchedule { get; set; }
        }
    }
}