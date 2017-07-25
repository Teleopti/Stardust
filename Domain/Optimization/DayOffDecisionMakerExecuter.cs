using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
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
        private readonly IResourceCalculation _resourceOptimizationHelper;
        private readonly ScheduleChangesAffectedDates _resourceCalculateDaysDecider;
        private readonly IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private readonly IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
	    private readonly IOptimizationLimits _optimizationLimits;
        private readonly INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
    	private readonly DayOffOptimizerPreMoveResultPredictor _dayOffOptimizerPreMoveResultPredictor;
	    private readonly IDaysOffPreferences _daysOffPreferences;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IUserTimeZone _userTimeZone;
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
            IResourceCalculation resourceOptimizationHelper,
            ScheduleChangesAffectedDates resourceCalculateDaysDecider,
            IDayOffOptimizerValidator dayOffOptimizerValidator,
            IDayOffOptimizerConflictHandler dayOffOptimizerConflictHandler,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IOptimizationLimits optimizationLimits,
            INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService, 
            ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			DayOffOptimizerPreMoveResultPredictor dayOffOptimizerPreMoveResultPredictor,
			IDaysOffPreferences daysOffPreferences,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone
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
            _optimizationLimits = optimizationLimits;
            _nightRestWhiteSpotSolverService = nightRestWhiteSpotSolverService;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
        	_dayOffOptimizerPreMoveResultPredictor = dayOffOptimizerPreMoveResultPredictor;
	        _daysOffPreferences = daysOffPreferences;
	        _schedulingResultStateHolder = schedulingResultStateHolder;
	        _userTimeZone = userTimeZone;

	        _logWriter = new LogWriter<DayOffDecisionMakerExecuter>();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool Execute(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro currentScheduleMatrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            if (workingBitArray == null)
                throw new ArgumentNullException(nameof(workingBitArray));

			var lastOverLimitCount = _optimizationLimits.OverLimitsCounts(currentScheduleMatrix);
            if (daysOverMax())
                return false;

            var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
        	schedulingOptions.UseCustomTargetTime = _originalStateContainer.OriginalWorkTime();

            var daysOffPreferences = _daysOffPreferences;

            var changesTracker = new LockableBitArrayChangesTracker();
			IList<DateOnly> movedDays = changesTracker.DayOffChanges(workingBitArray, originalBitArray, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);
			double oldValue;
			if(!_optimizerPreferences.Advanced.UseTweakedValues)
			{
				var predictorResult = _dayOffOptimizerPreMoveResultPredictor.IsPredictedBetterThanCurrent(currentScheduleMatrix,
					workingBitArray, originalBitArray, daysOffPreferences);
				oldValue = predictorResult.CurrentValue;

				if (!predictorResult.IsBetter)
				{
					writeToLogValueNotBetter();
					foreach (var movedDay in movedDays)
					{
						currentScheduleMatrix.LockDay(movedDay);
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


				if (!removeIllegalDayOffs(workingBitArray, _daysOffPreferences))
				{
					writeToLogBackToLegalStateFailed();
					return false;
				}
           
            movedDays = changesTracker.DayOffChanges(workingBitArray, workingBitArrayBeforeBackToLegalState, currentScheduleMatrix, daysOffPreferences.ConsiderWeekBefore);
            if (movedDays.Count > 0)
            {
                writeToLogDayOffBackToLegalStateRemovedDays(movedDays);
				foreach (var day in movedDays)
				{
					currentScheduleMatrix.LockDay(day);
				}
				return true;
            }

                _schedulePartModifyAndRollbackService.ClearModificationCollection();

            var result = executeDayOffMovesInMatrix(workingBitArray, originalBitArray, currentScheduleMatrix, schedulingOptions, daysOffPreferences);
            IEnumerable<changedDay> movedDates = result.MovedDays;

			IList<DateOnly> removedIllegalWorkTimeDays = new List<DateOnly>();

            if (!result.Result)
            {
                rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
                return false;
            }


				removedIllegalWorkTimeDays = removeIllegalWorkTimeDays(currentScheduleMatrix, schedulingOptions, _schedulePartModifyAndRollbackService);
				if (removedIllegalWorkTimeDays == null)
				{
					rollbackMovedDays(movedDates, new List<DateOnly>(), currentScheduleMatrix);
					return false;
				}
				
				if (removedIllegalWorkTimeDays.Count > 0)
					writeToLogWorkShiftBackToLegalStateRemovedDays(removedIllegalWorkTimeDays);

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


			var overLimitIncreased = _optimizationLimits.HasOverLimitExceeded(lastOverLimitCount, currentScheduleMatrix);

			if (overLimitIncreased)
			{
				rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
				var lastDay = movedDates.Last().DateChanged;
				currentScheduleMatrix.LockDay(lastDay);
				return true;
			}

			var minWorkTimePerWeekOk = _optimizationLimits.ValidateMinWorkTimePerWeek(currentScheduleMatrix);

			if (!minWorkTimePerWeekOk)
			{
				rollbackMovedDays(movedDates, removedIllegalWorkTimeDays, currentScheduleMatrix);
				foreach (var movedDate in movedDates)
				{
					var previousScheduleDay = movedDate.PrevoiousSchedule = (IScheduleDay)currentScheduleMatrix.GetScheduleDayByKey(movedDate.DateChanged).DaySchedulePart().Clone();
					if (previousScheduleDay.SignificantPart() != SchedulePartView.DayOff) currentScheduleMatrix.LockDay(movedDate.DateChanged);
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

        private bool daysOverMax()
        {
	        return _optimizationLimits.MoveMaxDaysOverLimit();
            //return _optimizationOverLimitDecider.MoveMaxDaysOverLimit();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogValueNotBetter()
        {
            _logWriter.LogInfo(()=>$"Move did not result in a better value {_periodValueCalculator}");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogReschedulingFailed()
        {
            _logWriter.LogInfo(()=>$"Rescheduling failed and a roll back was performed");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogWorkShiftBackToLegalStateRemovedDays(IEnumerable<DateOnly> removedIllegalWorkTimeDays)
        {
            _logWriter.LogInfo(()=>$"Work Shift back to legal state service removed the following days: { string.Join(",", removedIllegalWorkTimeDays.Select(d => d.ToShortDateString(CultureInfo.CurrentCulture))) }");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogDayOffBackToLegalStateRemovedDays(IEnumerable<DateOnly> movedDays)
        {
	        _logWriter.LogInfo(()=>
		        $"Day off back to legal state service removed the following days: {string.Join(",", movedDays.Select(d => d.ToShortDateString(CultureInfo.CurrentCulture)))}");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogBackToLegalStateFailed()
        {
            _logWriter.LogInfo(()=>$"Days off back to legal state failed");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.ILogWriter.LogInfo(System.String)")]
        private void writeToLogMovedDays(IEnumerable<DateOnly> movedDays)
        {
            _logWriter.LogInfo(()=>$"Day Off executer will work with the following days: { string.Join(", ", movedDays.Select(d => d.ToShortDateString(CultureInfo.CurrentCulture))) }");
        }

        private bool removeIllegalDayOffs(ILockableBitArray workingBitArray, IDaysOffPreferences daysOffPreferences)
        {
            //get back to legal state, if needed
            return _smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray, daysOffPreferences, 25), 100, new List<string>());
        }

        private dayOffOptimizerMoveDaysResult executeDayOffMovesInMatrix(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro matrix,
            SchedulingOptions schedulingOptions,
            IDaysOffPreferences daysOffPreferences)
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
						if (assignment?.ShiftCategory == null)
							return new dayOffOptimizerMoveDaysResult { Result = false, MovedDays = movedDays };

                        var changed = new changedDay
                                          {
                                              DateChanged = scheduleDayPro.Day,
                                              PrevoiousSchedule = (IScheduleDay)part.Clone()
                                          };
                        part.DeleteMainShift();
                        part.CreateAndAddDayOff(_dayOffTemplate);

                        removeDayOffFromMatrix(workingBitArray, originalBitArray, matrix, daysOffPreferences, movedDays);
                        _schedulePartModifyAndRollbackService.Modify(part);
	                    var resCalcData =_schedulingResultStateHolder.ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks,false);
						_resourceOptimizationHelper.ResourceCalculate(changed.DateChanged, resCalcData);

                        changed.CurrentSchedule = scheduleDayPro.DaySchedulePart();
                        movedDays.Add(changed);

                        if (!_dayOffOptimizerValidator.Validate(part.DateOnlyAsPeriod.DateOnly, matrix))
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
            SchedulingOptions schedulingOptions,
            IScheduleMatrixPro matrix, 
            IScheduleMatrixOriginalStateContainer originalStateContainer)
        {
            var toSchedule = movedDates.Select(changedDay => changedDay.DateChanged).ToList();
            toSchedule.AddRange(removedIllegalWorkTimeDays);
            toSchedule.Sort();
            foreach (DateOnly dateOnly in toSchedule)
            {

                IScheduleDay schedulePart = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();

                IScheduleDay originalScheduleDay = originalStateContainer.OldPeriodDaysState[dateOnly];
            	schedulingOptions.MainShiftOptimizeActivitySpecification = null;

	            var originalMainShift = originalScheduleDay.GetEditorShift();
	            if (originalMainShift != null)
	            {
		            _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(
						schedulingOptions, _optimizerPreferences, originalMainShift, dateOnly);
	            }

	            var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder, _userTimeZone);

	            bool schedulingResult = _scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, resourceCalculateDelayer,  _schedulePartModifyAndRollbackService);

                if (!schedulingResult)
                {
	                if (_nightRestWhiteSpotSolverService.Resolve(matrix, schedulingOptions, _schedulePartModifyAndRollbackService))
		                return true;

					IList<DateOnly> toResourceCalculate = _schedulePartModifyAndRollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
					_schedulePartModifyAndRollbackService.Rollback();
					var resCalcData = _schedulingResultStateHolder.ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks, false);
					foreach (DateOnly dateOnly1 in toResourceCalculate)
					{
						_resourceOptimizationHelper.ResourceCalculate(dateOnly1, resCalcData);
						_resourceOptimizationHelper.ResourceCalculate(dateOnly1.AddDays(1), resCalcData);
					}
                    return false;
                }
            }

            return true;
        }
        
        private void resourceCalculateMovedDays(IEnumerable<changedDay> changedDays)
        {
			bool considerShortBreaks = _optimizerPreferences.Rescheduling.ConsiderShortBreaks;
			var resCalcData = _schedulingResultStateHolder.ToResourceOptimizationData(considerShortBreaks, false);
			foreach (changedDay changed in changedDays)
            {
                var days = _resourceCalculateDaysDecider.DecideDates(changed.CurrentSchedule, changed.PrevoiousSchedule);
                foreach (var dateOnly in days)
                {
                    _resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
                }
            }
        }

        private IList<DateOnly> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
        {
			if (!_workTimeBackToLegalStateService.Execute(matrix, schedulingOptions, rollbackService))
				return null;

			bool considerShortBreaks = schedulingOptions.ConsiderShortBreaks;
			var resCalcData = _schedulingResultStateHolder.ToResourceOptimizationData(considerShortBreaks, false);
			IList<DateOnly> removedIllegalDates = _workTimeBackToLegalStateService.RemovedDays;
        	//resource calculate removed days
            foreach (DateOnly dateOnly in removedIllegalDates)
            {
                _resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
                if (!removedIllegalDates.Contains(dateOnly.AddDays(1)))
                {
                    _resourceOptimizationHelper.ResourceCalculate(dateOnly.AddDays(1), resCalcData);
                }
            }

            return removedIllegalDates;
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
