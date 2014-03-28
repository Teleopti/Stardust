using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Secret;
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
        private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
        private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private readonly IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private readonly INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
    	private readonly IDayOffOptimizerPreMoveResultPredictor _dayOffOptimizerPreMoveResultPredictor;
    	private readonly ILogWriter _logWriter;

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
            IResourceCalculateDaysDecider resourceCalculateDaysDecider,
            IDayOffOptimizerValidator dayOffOptimizerValidator,
            IDayOffOptimizerConflictHandler dayOffOptimizerConflictHandler,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider,
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService, 
            ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IDayOffOptimizerPreMoveResultPredictor dayOffOptimizerPreMoveResultPredictor
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
            _resourceCalculateDaysDecider = resourceCalculateDaysDecider;
            _dayOffOptimizerValidator = dayOffOptimizerValidator;
            _dayOffOptimizerConflictHandler = dayOffOptimizerConflictHandler;
            _originalStateContainer = originalStateContainer;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _nightRestWhiteSpotSolverService = nightRestWhiteSpotSolverService;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
        	_dayOffOptimizerPreMoveResultPredictor = dayOffOptimizerPreMoveResultPredictor;

        	_logWriter = new LogWriter<DayOffDecisionMakerExecuter>();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool Execute(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro currentScheduleMatrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer, 
            bool doReschedule, 
            bool handleDayOffConflict,
			bool goBackToLegalState)
        {
            if (workingBitArray == null)
                throw new ArgumentNullException("workingBitArray");

            if (restrictionsOverMax().Count > 0 || daysOverMax())
                return false;

            ISchedulingOptions schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
        	schedulingOptions.UseCustomTargetTime = _originalStateContainer.OriginalWorkTime();
            IDaysOffPreferences daysOffPreferences = _optimizerPreferences.DaysOff;
            var changesTracker = new LockableBitArrayChangesTracker();
			IList<DateOnly> movedDays = changesTracker.DayOffChanges(workingBitArray, originalBitArray, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);
			double oldValue;
			if(!_optimizerPreferences.Advanced.UseTweakedValues && doReschedule)
			{
				oldValue = _dayOffOptimizerPreMoveResultPredictor.CurrentValue(currentScheduleMatrix);
				double predictedNewValue = _dayOffOptimizerPreMoveResultPredictor.PredictedValue(currentScheduleMatrix, workingBitArray,
				                                                                          originalBitArray, daysOffPreferences);
				if (predictedNewValue >= oldValue)
				{
					writeToLogValueNotBetter();
					foreach (var movedDay in movedDays)
					{
						currentScheduleMatrix.LockPeriod(new DateOnlyPeriod(movedDay, movedDay));
					}
					return true;
				}	
			}
			else
			{
				oldValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
			}

            

            writeToLogMovedDays(movedDays);

            var workingBitArrayBeforeBackToLegalState = (ILockableBitArray)workingBitArray.Clone();

			if(goBackToLegalState)
			{
				if (!removeIllegalDayOffs(workingBitArray))
				{
					writeToLogBackToLegalStateFailed();
					return false;
				}
			}
           

            movedDays = changesTracker.DayOffChanges(workingBitArray, workingBitArrayBeforeBackToLegalState, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);
            if (movedDays.Count > 0)
            {
                writeToLogDayOffBackToLegalStateRemovedDays(movedDays);
				foreach (var day in movedDays)
				{
					currentScheduleMatrix.LockPeriod(new DateOnlyPeriod(day, day));
				}
				return true;
            }

            if (doReschedule)
                _schedulePartModifyAndRollbackService.ClearModificationCollection();

            var result = 
                executeDayOffMovesInMatrix(workingBitArray, originalBitArray, currentScheduleMatrix, schedulingOptions, daysOffPreferences, handleDayOffConflict);
            IEnumerable<changedDay> movedDates = result.MovedDays;

			IList<DateOnly> removedIllegalWorkTimeDays = new List<DateOnly>();

            if (!result.Result)
            {
                if (doReschedule)
                    rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

			if (goBackToLegalState)
			{
				removedIllegalWorkTimeDays = removeIllegalWorkTimeDays(currentScheduleMatrix, schedulingOptions, _schedulePartModifyAndRollbackService);
				if (removedIllegalWorkTimeDays == null)
					return false;

				if (removedIllegalWorkTimeDays.Count > 0)
					writeToLogWorkShiftBackToLegalStateRemovedDays(removedIllegalWorkTimeDays);
			}

        	if (!doReschedule)
                return true;

            if (!rescheduleWhiteSpots(movedDates, removedIllegalWorkTimeDays, schedulingOptions, currentScheduleMatrix, originalStateContainer))
            {
                writeToLogReschedulingFailed();
                return false;
            }

            if(daysOverMax())
            {
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            IList<DateOnly> daysToLock = restrictionsOverMax();
            if (daysToLock.Count > 0)
            {
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                foreach (var day in daysToLock)
                {
                    //should only lock the days breaking restrictions, so we must return those days from restrictionsOverMax
                    currentScheduleMatrix.LockPeriod(new DateOnlyPeriod(day, day));
                }
                return true;
            }

			double newValue;
			if(!_optimizerPreferences.Advanced.UseTweakedValues)
				newValue = _dayOffOptimizerPreMoveResultPredictor.CurrentValue(currentScheduleMatrix);
			else
			{
				newValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
			}
			if (newValue >= oldValue)
            {
                writeToLogValueNotBetter();
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }

            return true;
        }

        private IList<DateOnly> restrictionsOverMax()
        {
            return _optimizationOverLimitDecider.OverLimit(); //maybe send in matrix to get the days locked
        }

        private bool daysOverMax()
        {
            return _optimizationOverLimitDecider.MoveMaxDaysOverLimit();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogValueNotBetter()
        {
            _logWriter.LogInfo("Move did not result in a better value " + _periodValueCalculator);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogReschedulingFailed()
        {
            _logWriter.LogInfo("Rescheduling failed and a roll back was performed");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogWorkShiftBackToLegalStateRemovedDays(IEnumerable<DateOnly> removedIllegalWorkTimeDays)
        {
            string loginfo = ("Work Shift back to legal state service removed the following days: " +
                              createCommaSeparatedString(removedIllegalWorkTimeDays));
            _logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogDayOffBackToLegalStateRemovedDays(IEnumerable<DateOnly> movedDays)
        {
            string loginfo = ("Day off back to legal state service removed the following days: " +
                              createCommaSeparatedString(movedDays));
            _logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogBackToLegalStateFailed()
        {
            const string loginfo = "Days off back to legal state failed";
            _logWriter.LogInfo(loginfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogMovedDays(IEnumerable<DateOnly> movedDays)
        {
            string loginfo = ("Day Off executer will work with the following days: " + createCommaSeparatedString(movedDays));
            _logWriter.LogInfo(loginfo);
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
                    	IPersonAssignment assignment = part.PersonAssignment();
											if (assignment == null || assignment.ShiftCategory == null)
							return new dayOffOptimizerMoveDaysResult { Result = false, MovedDays = movedDays };
                        var changed = new changedDay
                                          {
                                              DateChanged = scheduleDayPro.Day,
                                              PrevoiousSchedule = (IScheduleDay)part.Clone()
                                          };
                        part.DeleteMainShift(part);
                        part.CreateAndAddDayOff(_dayOffTemplate);

                        removeDayOffFromMatrix(workingBitArray, originalBitArray, matrix, daysOffPreferences, movedDays);
                        _schedulePartModifyAndRollbackService.Modify(part);
						_resourceOptimizationHelper.ResourceCalculateDate(changed.DateChanged, true, schedulingOptions.ConsiderShortBreaks);

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
            	schedulingOptions.MainShiftOptimizeActivitySpecification = null;

	            var originalMainShift = originalScheduleDay.GetEditorShift();
	            if (originalMainShift != null)
	            {
					originalShiftCategory = originalMainShift.ShiftCategory;
		            _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
		                                                                           originalMainShift, dateOnly);
	            }

	            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);

                bool schedulingResult;
                if (effectiveRestriction.ShiftCategory == null && originalShiftCategory != null)
                {
					var possibleShiftOption = new PossibleStartEndCategory { ShiftCategory = originalShiftCategory };
					schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, possibleShiftOption, _schedulePartModifyAndRollbackService);
                    if(!schedulingResult)
						schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, null, _schedulePartModifyAndRollbackService);
                }
                else
					schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, null, _schedulePartModifyAndRollbackService);

                if (!schedulingResult)
                {
                    int iterations = 0;
                    while (_nightRestWhiteSpotSolverService.Resolve(matrix, schedulingOptions, _schedulePartModifyAndRollbackService) && iterations < 10)
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
                IList<DateOnly> days = _resourceCalculateDaysDecider.DecideDates(changed.CurrentSchedule, changed.PrevoiousSchedule);
                foreach (var dateOnly in days)
                {
                    bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                }
            }
        }

        private IList<DateOnly> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
			if (!_workTimeBackToLegalStateService.Execute(matrix, schedulingOptions, rollbackService))
				return null;

            IList<DateOnly> removedIllegalDates = _workTimeBackToLegalStateService.RemovedDays;
        	//resource calculate removed days
            foreach (DateOnly dateOnly in removedIllegalDates)
            {
				bool considerShortBreaks = schedulingOptions.ConsiderShortBreaks;
                _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, considerShortBreaks);
                if (!removedIllegalDates.Contains(dateOnly.AddDays(1)))
                {
                    _resourceOptimizationHelper.ResourceCalculateDate(dateOnly.AddDays(1), true, considerShortBreaks);
                }
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
