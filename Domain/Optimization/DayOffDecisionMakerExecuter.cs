using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffDecisionMakerExecuter : IDayOffDecisionMakerExecuter
    {
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
        private readonly IDayOffTemplate _dayOffTemplate;
        private readonly IScheduleService _scheduleService;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly IPeriodValueCalculator _periodValueCalculator;
        private readonly IWorkShiftBackToLegalStateServicePro _workTimeBackToLegalStateService;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IResourceCalculateDaysDecider _decider;
        private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private readonly IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IOptimizationOverLimitDecider _overLimitDecider;
        private readonly INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private readonly ISchedulingOptionsSynchronizer _schedulingOptionsSynchronizer;

        public DayOffDecisionMakerExecuter(
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService,
            IDayOffTemplate dayOffTemplate,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            IPeriodValueCalculator periodValueCalculator,
            IWorkShiftBackToLegalStateServicePro workTimeBackToLegalStateService,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IResourceCalculateDaysDecider decider,
            IDayOffOptimizerValidator dayOffOptimizerValidator,
            IDayOffOptimizerConflictHandler dayOffOptimizerConflictHandler,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IOptimizationOverLimitDecider overLimitDecider,
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService, 
            ISchedulingOptionsSynchronizer schedulingOptionsSynchronizer
            )
        {
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
            _dayOffTemplate = dayOffTemplate;
            _scheduleService = scheduleService;
            _optimizerPreferences = optimizerPreferences;
            _periodValueCalculator = periodValueCalculator;
            _workTimeBackToLegalStateService = workTimeBackToLegalStateService;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _resourceOptimizationHelper = resourceOptimizationHelper;
            _decider = decider;
            _dayOffOptimizerValidator = dayOffOptimizerValidator;
            _dayOffOptimizerConflictHandler = dayOffOptimizerConflictHandler;
            _originalStateContainer = originalStateContainer;
            _overLimitDecider = overLimitDecider;
            _nightRestWhiteSpotSolverService = nightRestWhiteSpotSolverService;
            _schedulingOptionsSynchronizer = schedulingOptionsSynchronizer;
        }

        public bool Execute(ILockableBitArray workingBitArray, ILockableBitArray originalBitArray, IScheduleMatrixPro currentScheduleMatrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer, bool doReschedule, bool handleDayOffConflict)
        {
            if (workingBitArray == null)
                throw new ArgumentNullException("workingBitArray");

            ILogWriter logWriter = new LogWriter<DayOffDecisionMakerExecuter>();

            if (movesOverMaxDaysLimit())
                return false;

            ISchedulingOptions schedulingOptions = _scheduleService.SchedulingOptions;
            IDaysOffPreferences daysOffPreferences = _optimizerPreferences.DaysOff;

            _schedulingOptionsSynchronizer.SynchronizeSchedulingOption(_optimizerPreferences, schedulingOptions);

            var changesTracker = new LockableBitArrayChangesTracker();

            IList<DateOnly> movedDays = changesTracker.DayOffChanges(workingBitArray, originalBitArray, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);

            writeToLogMovedDays(movedDays, logWriter);

            double oldValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);

            var workingBitArrayBeforeBackToLegalState = (ILockableBitArray)workingBitArray.Clone();

            if (!removeIllegalDayOffs(workingBitArray))
            {
                writeToLogBackToLegalStateFailed(logWriter);
                return false;
            }

            movedDays = changesTracker.DayOffChanges(workingBitArray, workingBitArrayBeforeBackToLegalState, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);
            if (movedDays.Count > 0)
            {
                writeToLogDayOffBackToLegalStateRemovedDays(movedDays, logWriter);
            }

            if (doReschedule)
                _schedulePartModifyAndRollbackService.ClearModificationCollection();

            IList<DateOnly> removedIllegalWorkTimeDays = new List<DateOnly>();

            var result = 
                executeDayOffMovesInMatrix(workingBitArray, originalBitArray, currentScheduleMatrix, schedulingOptions, daysOffPreferences, handleDayOffConflict);
            IEnumerable<changedDay> movedDates = result.MovedDays;

            if (!result.Result)
            {
                if (doReschedule)
                    rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            resourceCalculateMovedDays(movedDates);

            removedIllegalWorkTimeDays = removeIllegalWorkTimeDays(currentScheduleMatrix);
            if (removedIllegalWorkTimeDays.Count > 0)
            {
                writeToLogWorkShiftBackToLegalStateRemovedDays(logWriter, removedIllegalWorkTimeDays);
            }

            if (!doReschedule)
                return true;

            if (!rescheduleWhiteSpots(movedDates, removedIllegalWorkTimeDays, schedulingOptions, currentScheduleMatrix, originalStateContainer))
            {
                writeToLogReschedulingFailed(logWriter);
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            if (movesOverMaxDaysLimit())
            {
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            double newValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
            if (newValue >= oldValue)
            {
                writeToLogValueNotBetter(logWriter);
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            return true;
        }

        private bool movesOverMaxDaysLimit()
        {
            return _overLimitDecider.OverLimit();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogValueNotBetter(ILogWriter logWriter)
        {
            logWriter.LogInfo("Move did not result in a better value " + _periodValueCalculator);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogReschedulingFailed(ILogWriter logWriter)
        {
            logWriter.LogInfo("Rescheduling failed and a roll back was performed");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogWorkShiftBackToLegalStateRemovedDays(ILogWriter logWriter, IEnumerable<DateOnly> removedIllegalWorkTimeDays)
        {
            string loginfo = ("Work Shift back to legal state service removed the following days: " +
                              createCommaSeparatedString(removedIllegalWorkTimeDays));
            logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogDayOffBackToLegalStateRemovedDays(IEnumerable<DateOnly> movedDays, ILogWriter logWriter)
        {
            string loginfo = ("Day off back to legal state service removed the following days: " +
                              createCommaSeparatedString(movedDays));
            logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogBackToLegalStateFailed(ILogWriter logWriter)
        {
            const string loginfo = "Days off back to legal state failed";
            logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private static void writeToLogMovedDays(IEnumerable<DateOnly> movedDays, ILogWriter logWriter)
        {
            string loginfo = ("Day Off executer will work with the following days: " + createCommaSeparatedString(movedDays));
            logWriter.LogInfo(loginfo);
        }

        private bool removeIllegalDayOffs(ILockableBitArray workingBitArray)
        {
            //get back to legal state, if needed
            return _smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100);
        }

        private dayOffOptimizerMoveDaysResult executeDayOffMovesInMatrix(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro matrix,
            ISchedulingOptions schedulingOptions,
            IDaysOffPreferences daysOffPreferences, 
            bool handleConflict)
        {
            IList<changedDay> movedDays = new List<changedDay>();
            bool result = true;

            //ok so what have changed
            int bitArrayToMatrixOffset = 0;
            if (!daysOffPreferences.ConsiderWeekBefore)
                bitArrayToMatrixOffset = 7;

            for (int i = 0; i < workingBitArray.Count; i++)
            {
                {
                    if (workingBitArray[i] && !originalBitArray[i])
                    {
                        IScheduleDayPro scheduleDayPro =
                        matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset];
                        IScheduleDay part = scheduleDayPro.DaySchedulePart();
                        var changed = new changedDay
                                          {
                                              DateChanged = scheduleDayPro.Day,
                                              PrevoiousSchedule = (IScheduleDay)part.Clone()
                                          };
                        part.DeleteMainShift(part);
                        part.CreateAndAddDayOff(_dayOffTemplate);

                        removeDayOffFromMatrix(workingBitArray, originalBitArray, matrix, daysOffPreferences, movedDays);
                        _schedulePartModifyAndRollbackService.Modify(part);

                        changed.CurrentSchedule = scheduleDayPro.DaySchedulePart();
                        movedDays.Add(changed);

                        if (handleConflict && !_dayOffOptimizerValidator.Validate(part.DateOnlyAsPeriod.DateOnly, matrix))
                        {
                            //if day off rule validation fails, try to reschedule day before and day after
                            result = _dayOffOptimizerConflictHandler.HandleConflict(schedulingOptions, part.DateOnlyAsPeriod.DateOnly);
                            if (!result)
                                break;
                        }
                    }
                }
            }

            var moveResult = new dayOffOptimizerMoveDaysResult { Result = result, MovedDays = movedDays };
            return moveResult;
        }

        private void removeDayOffFromMatrix(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro matrix,
            IDaysOffPreferences daysOffPreferences,
            IList<changedDay> movedDays)
        {
            int bitArrayToMatrixOffset = 0;
            if (!daysOffPreferences.ConsiderWeekBefore)
                bitArrayToMatrixOffset = 7;

            for (int i = 0; i < workingBitArray.Count; i++)
            {
                if (!workingBitArray[i] && originalBitArray[i] && !workingBitArray.IsLocked(i, true))
                {
                    IScheduleDayPro scheduleDayPro =
                        matrix.OuterWeeksPeriodDays[i + bitArrayToMatrixOffset];
                    IScheduleDay part = scheduleDayPro.DaySchedulePart();
                    var changed = new changedDay
                    {
                        DateChanged = scheduleDayPro.Day,
                        PrevoiousSchedule = (IScheduleDay)part.Clone()
                    };
                    part.DeleteDayOff();
                    _schedulePartModifyAndRollbackService.Modify(part);
                    changed.CurrentSchedule = scheduleDayPro.DaySchedulePart();
                    movedDays.Add(changed);
                    // lock the day
                    workingBitArray.Lock(i, true);

                    break;
                }
            }
        }

        private void rollbackMovedDays(IEnumerable<changedDay> movedDates, IEnumerable<DateOnly> removedIllegalWorkTimeDays, IScheduleMatrixPro matrix)
        {
            IDictionary<DateOnly, changedDay> dic = new Dictionary<DateOnly, changedDay>();
            foreach (changedDay changedDay in movedDates)
            {
                changedDay.PrevoiousSchedule = (IScheduleDay)matrix.GetScheduleDayByKey(changedDay.DateChanged).DaySchedulePart().Clone();
                dic.Add(changedDay.DateChanged, changedDay);
            }
            foreach (DateOnly dateOnly in removedIllegalWorkTimeDays)
            {
                if (dic.ContainsKey(dateOnly)) continue;
                var changedDay = new changedDay { DateChanged = dateOnly };
                changedDay.PrevoiousSchedule =
                    (IScheduleDay)matrix.GetScheduleDayByKey(changedDay.DateChanged).DaySchedulePart().Clone();
                dic.Add(changedDay.DateChanged, changedDay);
            }
            _schedulePartModifyAndRollbackService.Rollback();
            foreach (KeyValuePair<DateOnly, changedDay> keyValuePair in dic)
            {
                changedDay changedDay = keyValuePair.Value;
                changedDay.CurrentSchedule = matrix.GetScheduleDayByKey(changedDay.DateChanged).DaySchedulePart();
            }

            resourceCalculateMovedDays(dic.Values);

        }

        private bool rescheduleWhiteSpots(
            IEnumerable<changedDay> movedDates, 
            IEnumerable<DateOnly> removedIllegalWorkTimeDays, 
            ISchedulingOptions schedulingOptions,
            IScheduleMatrixPro matrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            var toSchedule = movedDates.Select(changedDay => changedDay.DateChanged).ToList();
            toSchedule.AddRange(removedIllegalWorkTimeDays);
            toSchedule.Sort();
            foreach (DateOnly dateOnly in toSchedule)
            {

                IScheduleDay schedulePart = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();


                // reviewed and fixed version
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
                    schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, true, originalShiftCategory);
                    if(!schedulingResult)
                        schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, true, effectiveRestriction);
                }
                else
                    schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, true, effectiveRestriction);

                if (!schedulingResult)
                {
                    int iterations = 0;
                    while (_nightRestWhiteSpotSolverService.Resolve(matrix) && iterations < 10)
                    {
                        iterations++;
                    }

                    if (_originalStateContainer.IsFullyScheduled())
                        return true;

                    IList<DateOnly> toResourceCalculate = _schedulePartModifyAndRollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
                    _schedulePartModifyAndRollbackService.Rollback();
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

        private static string createCommaSeparatedString(IEnumerable<DateOnly> days)
        {
            var stringBuilder = new StringBuilder();
            foreach (DateOnly day in days)
            {
                stringBuilder.Append(day.ToShortDateString(CultureInfo.CurrentCulture) + ",");
            }
            string result = stringBuilder.ToString();
            if (result.Length > 0)
                result = result.Substring(0, result.Length - 1);
            return result;
        }

        private class changedDay
        {
            public DateOnly DateChanged { get; set; }
            public IScheduleDay PrevoiousSchedule { get; set; }
            public IScheduleDay CurrentSchedule { get; set; }
        }

        private class dayOffOptimizerMoveDaysResult
        {
            public IList<changedDay> MovedDays { get; set; }
            public bool Result { get; set; }
        }
    }

}
