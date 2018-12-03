using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

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
        private readonly MoveTimeDecisionMaker2 _decisionMaker;
        private readonly IScheduleService _scheduleService;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
    	private readonly DeleteAndResourceCalculateService _deleteAndResourceCalculateService;
    	private readonly IResourceCalculation _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
	    private readonly OptimizationLimits _optimizationLimits;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
	    private readonly IScheduleMatrixPro _matrix;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IUserTimeZone _userTimeZone;

	    public MoveTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
			MoveTimeDecisionMaker2 decisionMaker,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            DeleteAndResourceCalculateService deleteAndResourceCalculateService,
            IResourceCalculation resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
			OptimizationLimits optimizationLimits,
            ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IScheduleMatrixPro matrix,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _scheduleService = scheduleService;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
    		_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
    		_resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _workShiftOriginalStateContainer = workShiftOriginalStateContainer;
		    _optimizationLimits = optimizationLimits;

            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		    _matrix = matrix;
		    _schedulingResultStateHolder = schedulingResultStateHolder;
	        _userTimeZone = userTimeZone;
        }

		public bool Execute()
        {
            var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();
			var lastOverLimitCount = _optimizationLimits.OverLimitsCounts(_matrix);
            if (daysOverMax())
                return false;

            double oldPeriodValue = calculatePeriodValue();

            // Step: get days to move
            IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(_matrix, _personalSkillsDataExtractor);

            if (daysToBeMoved.Count == 0)
                return false;

            _rollbackService.ClearModificationCollection();

            IScheduleDayPro firstDay = _matrix.GetScheduleDayByKey(daysToBeMoved[0]);
            DateOnly firstDayDate = daysToBeMoved[0];
            IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();
			var originalFirstScheduleDay = (IScheduleDay) firstScheduleDay.Clone();
            var firstDayEffectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(firstScheduleDay, schedulingOptions);
			TimeSpan firstDayContractTime = firstDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

            IScheduleDayPro secondDay = _matrix.GetScheduleDayByKey(daysToBeMoved[1]);
            DateOnly secondDayDate = daysToBeMoved[1];
            IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			var originalSecondScheduleDay = (IScheduleDay)secondScheduleDay.Clone();
            var secondDayEffectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(secondScheduleDay, schedulingOptions);
			TimeSpan secondDayContractTime = secondDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

            if (firstDayDate == secondDayDate)
                return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(secondDayDate);
				return true;
			}

            //delete schedule on the two days
            IList<IScheduleDay> listToDelete = new List<IScheduleDay> { firstDay.DaySchedulePart(), secondDay.DaySchedulePart() };
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, _rollbackService, schedulingOptions.ConsiderShortBreaks, false);
            var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder, _userTimeZone);

            if (!tryScheduleFirstDay(firstDayDate, schedulingOptions, firstDayEffectiveRestriction, firstDayContractTime))
            {
                safeCalculateDate(firstDayDate, originalFirstScheduleDay, resourceCalculateDelayer);
                safeCalculateDate(secondDayDate, originalSecondScheduleDay, resourceCalculateDelayer);
                return true;
            }

            // Back to legal state
            // leave it for now

            if (!tryScheduleSecondDay(secondDayDate, schedulingOptions, secondDayEffectiveRestriction, secondDayContractTime))
            {
                safeCalculateDate(firstDayDate, originalFirstScheduleDay, resourceCalculateDelayer);
                safeCalculateDate(secondDayDate, originalSecondScheduleDay, resourceCalculateDelayer);
                return true;
            }

            // Step: Check that there are no white spots

            double newPeriodValue = calculatePeriodValue();
            bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
            {
                rollbackLockAndCalculate(firstDayDate, secondDayDate, originalFirstScheduleDay, originalSecondScheduleDay, resourceCalculateDelayer);
				lockDays(firstDayDate, secondDayDate);
                return true;
            }

            if (daysOverMax())
            {
                rollbackLockAndCalculate(firstDayDate, secondDayDate, originalFirstScheduleDay, originalSecondScheduleDay, resourceCalculateDelayer);
				lockDays(firstDayDate, secondDayDate);
                return false;
            }

			if (_optimizationLimits.HasOverLimitExceeded(lastOverLimitCount, _matrix))
			{
				rollbackLockAndCalculate(firstDayDate, secondDayDate, originalFirstScheduleDay, originalSecondScheduleDay, resourceCalculateDelayer);
				lockDays(firstDayDate, secondDayDate);
				return true;	
			}

			var minWorkTimePerWeekOk = _optimizationLimits.ValidateMinWorkTimePerWeek(_matrix);

			if (!minWorkTimePerWeekOk)
			{
				rollbackLockAndCalculate(firstDayDate, secondDayDate, originalFirstScheduleDay, originalSecondScheduleDay, resourceCalculateDelayer);
				lockDays(firstDayDate, secondDayDate);
				return true;
			}
            
            return true;
        }

		private void rollbackLockAndCalculate(DateOnly firstDayDate, DateOnly secondDayDate, IScheduleDay originalFirstScheduleDay, IScheduleDay originalSecondScheduleDay,IResourceCalculateDelayer resourceCalculateDelayer)
		{
			_rollbackService.Rollback();
            safeCalculateDate(firstDayDate, originalFirstScheduleDay, resourceCalculateDelayer);
            safeCalculateDate(secondDayDate, originalSecondScheduleDay, resourceCalculateDelayer);
		}

        private void safeCalculateDate(DateOnly dayDate, IScheduleDay originalScheduleDay, IResourceCalculateDelayer resourceCalculateDelayer)
        {
            resourceCalculateDelayer.CalculateIfNeeded(dayDate, originalScheduleDay.ProjectionService().CreateProjection().Period(), false);
        }

		private void lockDays(DateOnly firstDayDate, DateOnly secondDayDate)
		{
			lockDay(firstDayDate);
			lockDay(secondDayDate);
		}

        private bool daysOverMax()
        {
	        return _optimizationLimits.MoveMaxDaysOverLimit();
        }

        public IPerson ContainerOwner
        {
            get { return _matrix.Person; }
        }

	    public IScheduleMatrixPro Matrix
	    {
			get
			{
				return _matrix;
			}
	    }

	    private double calculatePeriodValue()
        {
            return _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
        }

		private bool tryScheduleSecondDay(DateOnly secondDate, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, TimeSpan originalLength)
        {
            return tryScheduleDay(secondDate, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.AverageWorkTime, originalLength);
        }

		private bool tryScheduleFirstDay(DateOnly firstDate, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, TimeSpan originalLength)
        {
            return tryScheduleDay(firstDate, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.Long, originalLength);
        }

        private bool tryScheduleDay(DateOnly day, SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption, TimeSpan originalLength )
        {
			IScheduleDayPro scheduleDay = _matrix.GetScheduleDayByKey(day);
            schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder, _userTimeZone);

        	var dic = _workShiftOriginalStateContainer.OldPeriodDaysState;
        	IScheduleDay originalScheduleDay = dic[day];
			var originalShift = originalScheduleDay.GetEditorShift();
			_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizerPreferences, originalShift, day);

        	bool success = _scheduleService.SchedulePersonOnDay(scheduleDay.DaySchedulePart(), schedulingOptions,
        	                                                    effectiveRestriction, resourceCalculateDelayer, _rollbackService);

			if (success && scheduleDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime() == originalLength)
				success = false;
			if (!success)
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
            _matrix.LockDay(day);
        }

    }
}
