﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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
    	private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
    	private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
        private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private readonly IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
        private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
    	private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

    	public MoveTimeOptimizer(
            IPeriodValueCalculator periodValueCalculator,
            IScheduleResultDataExtractor personalSkillsDataExtractor,
            IMoveTimeDecisionMaker decisionMaker,
            IScheduleMatrixLockableBitArrayConverter matrixConverter,
            IScheduleService scheduleService,
            IOptimizationPreferences optimizerPreferences,
            ISchedulePartModifyAndRollbackService rollbackService,
            IDeleteAndResourceCalculateService deleteAndResourceCalculateService,
            IResourceOptimizationHelper resourceOptimizationHelper,
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
            IScheduleMatrixOriginalStateContainer workShiftOriginalStateContainer,
            IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider, 
            ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter)
        {
            _periodValueCalculator = periodValueCalculator;
            _personalSkillsDataExtractor = personalSkillsDataExtractor;
            _decisionMaker = decisionMaker;
            _matrixConverter = matrixConverter;
            _scheduleService = scheduleService;
            _optimizerPreferences = optimizerPreferences;
            _rollbackService = rollbackService;
    		_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
    		_resourceOptimizationHelper = resourceOptimizationHelper;
            _effectiveRestrictionCreator = effectiveRestrictionCreator;
            _workShiftOriginalStateContainer = workShiftOriginalStateContainer;
            _optimizationOverLimitDecider = optimizationOverLimitDecider;
            _schedulingOptionsCreator = schedulingOptionsCreator;
        	_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public bool Execute()
        {

            var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);
			schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

            if (restrictionsOverMax().Count > 0 || daysOverMax())
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
			//IScheduleDay originalFirstScheduleDay = (IScheduleDay) firstScheduleDay.Clone();
            var firstDayEffectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(firstScheduleDay, schedulingOptions);
			TimeSpan firstDayContractTime = firstDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

            IScheduleDayPro secondDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(daysToBeMoved[1]);
            DateOnly secondDayDate = daysToBeMoved[1];
            IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			//IScheduleDay originalSecondScheduleDay = (IScheduleDay)secondScheduleDay.Clone();
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
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, _rollbackService);


            if (!tryScheduleFirstDay(firstDayDate, schedulingOptions, firstDayEffectiveRestriction, firstDayContractTime))
            {
				safeCalculateDate(firstDayDate);
				safeCalculateDate(secondDayDate);
                return true;
            }

            // Back to legal state
            // leave it for now

            if (!tryScheduleSecondDay(secondDayDate, schedulingOptions, secondDayEffectiveRestriction, secondDayContractTime))
            {
				safeCalculateDate(firstDayDate);
				safeCalculateDate(secondDayDate);
                return true;
            }

            // Step: Check that there are no white spots

            double newPeriodValue = calculatePeriodValue();
            bool isPeriodWorse = newPeriodValue > oldPeriodValue;
            if (isPeriodWorse)
            {
            	rollbackLockAndCalculate(firstDayDate, secondDayDate);
                return true;
            }

            if (daysOverMax())
            {
				rollbackLockAndCalculate(firstDayDate, secondDayDate);
                return false;
            }

            if (restrictionsOverMax().Count > 0)
            {
				rollbackLockAndCalculate(firstDayDate, secondDayDate);
                return true;
            }

            //lockDays(firstDayDate, secondDayDate);

            return true;
        }

		private void rollbackLockAndCalculate(DateOnly firstDayDate, DateOnly secondDayDate)
		{
			_rollbackService.Rollback();
			lockDays(firstDayDate, secondDayDate);
			safeCalculateDate(firstDayDate);
			safeCalculateDate(secondDayDate);
		}

		private void safeCalculateDate(DateOnly dayDate)
		{
			_resourceOptimizationHelper.ResourceCalculateDate(dayDate, true, true);
			_resourceOptimizationHelper.ResourceCalculateDate(dayDate.AddDays(1), true, true);
		}

		//private void calculateDate(DateOnly dayDate, IScheduleDay originalScheduleDay)
		//{
		//    IScheduleDay currentFirstScheduleDay =
		//        _matrixConverter.SourceMatrix.GetScheduleDayByKey(dayDate).DaySchedulePart();
		//    IList<DateOnly> dates = _decider.DecideDates(currentFirstScheduleDay, originalScheduleDay);
		//    foreach (var dateOnly in dates)
		//    {
		//        _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true,
		//                                                      new List<IScheduleDay> { currentFirstScheduleDay },
		//                                                      new List<IScheduleDay> { originalScheduleDay });
		//    }
		//}

        private void lockDays(DateOnly firstDayDate, DateOnly secondDayDate)
        {
            lockDay(firstDayDate);
            lockDay(secondDayDate);
        }

        private IList<DateOnly> restrictionsOverMax()
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

        private double calculatePeriodValue()
        {
            return _periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
        }

		private bool tryScheduleSecondDay(DateOnly secondDate, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, TimeSpan originalLength)
        {
            return tryScheduleDay(secondDate, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.Short, originalLength);
        }

		private bool tryScheduleFirstDay(DateOnly firstDate, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, TimeSpan originalLength)
        {
            return tryScheduleDay(firstDate, schedulingOptions, effectiveRestriction, WorkShiftLengthHintOption.Long, originalLength);
        }

        private bool tryScheduleDay(DateOnly day, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, WorkShiftLengthHintOption workShiftLengthHintOption, TimeSpan originalLength )
        {
			IScheduleDayPro scheduleDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(day);
            schedulingOptions.WorkShiftLengthHintOption = workShiftLengthHintOption;
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																		schedulingOptions.ConsiderShortBreaks);

        	var dic = _workShiftOriginalStateContainer.OldPeriodDaysState;
        	IScheduleDay originalScheduleDay = dic[day];
        	IPersonAssignment personAssignment = originalScheduleDay.AssignmentHighZOrder();
			IMainShift originalShift = personAssignment.MainShift;
			_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences, originalShift, day);

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
            _matrixConverter.SourceMatrix.LockPeriod(new DateOnlyPeriod(day, day));
        }

    }
}
