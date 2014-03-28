using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Secret;
using Teleopti.Interfaces.Domain;

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
	    private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
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
		private readonly IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;

		public ExtendReduceDaysOffOptimizer(
			IPeriodValueCalculator periodValueCalculator,
			IScheduleResultDataExtractor personalSkillsDataExtractor,
			IExtendReduceDaysOffDecisionMaker decisionMaker,
			IScheduleMatrixLockableBitArrayConverter matrixConverter,
			IScheduleService scheduleServiceForFlexibleAgents,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
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
			IOptimizationOverLimitByRestrictionDecider optimizationOverLimitDecider,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter
			)
		{
			_periodValueCalculator = periodValueCalculator;
			_personalSkillsDataExtractor = personalSkillsDataExtractor;
			_decisionMaker = decisionMaker;
			_matrixConverter = matrixConverter;
			_scheduleServiceForFlexibleAgents = scheduleServiceForFlexibleAgents;
			_optimizerPreferences = optimizerPreferences;
			_rollbackService = rollbackService;
		    _resourceCalculateDelayer = resourceCalculateDelayer;
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
			_optimizationOverLimitDecider = optimizationOverLimitDecider;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
		}

		public bool Execute()
		{
			if (restrictionsOverMax().Count > 0 || daysOverMax())
				return false;

			_rollbackService.ClearModificationCollection();

			var schedulePeriod = _matrixConverter.SourceMatrix.SchedulePeriod;
			int targetDaysoff;
			IList<IScheduleDay> dayOffDays;
			if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysoff, out dayOffDays))
				return false;

			bool success = false;

			ExtendReduceTimeDecisionMakerResult daysToBeRescheduled =
				_decisionMaker.Execute(_matrixConverter, _personalSkillsDataExtractor, _validatorList);

			if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
				return false;

			var oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
			var sourceMatrix = _matrixConverter.SourceMatrix;
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);

			if (daysToBeRescheduled.DayToLengthen.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(schedulePeriod))
			{

				var dayToLengthen = daysToBeRescheduled.DayToLengthen.Value;
				var schedulePart = sourceMatrix.GetScheduleDayByKey(dayToLengthen).DaySchedulePart();
				var changedDayOff = new changedDay {PreviousSchedule = schedulePart, DateChanged = dayToLengthen};


				schedulePart.DeleteDayOff();
				_rollbackService.Modify(schedulePart);

				changedDayOff.CurrentSchedule = sourceMatrix.GetScheduleDayByKey(dayToLengthen).DaySchedulePart();

				IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(sourceMatrix, schedulingOptions, _rollbackService);  //resource calculation is done automaticaly

				if (rescheduleWhiteSpots(new[] { changedDayOff }, illegalDays, sourceMatrix, _originalStateContainerForTagChange, schedulingOptions))
					success = true;
				// bugfix for infinie loop 19889. We need to lock the day for avoiding infinitive loop
				sourceMatrix.LockPeriod(new DateOnlyPeriod(daysToBeRescheduled.DayToLengthen.Value, daysToBeRescheduled.DayToLengthen.Value));
			}

			if (daysToBeRescheduled.DayToShorten.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(schedulePeriod))
			{
				DateOnly dayToShorten = daysToBeRescheduled.DayToShorten.Value;
				if (addDayOff(dayToShorten, true, schedulingOptions, _rollbackService))
					success = true;
				// bugfix for infinie loop 19889. We need to lock the day for avoiding infinitive loop
				sourceMatrix.LockPeriod(new DateOnlyPeriod(daysToBeRescheduled.DayToShorten.Value, daysToBeRescheduled.DayToShorten.Value));
			}

			if (success)
			{
				if (daysOverMax())
				{
					rollbackAndResourceCalculate();
					return false;
				}

				IList<DateOnly> daysToLock = restrictionsOverMax();
				if (daysToLock.Count > 0)
				{
					foreach (var dateOnly in daysToLock)
					{
						sourceMatrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
					}
					rollbackAndResourceCalculate();
					return true;
				}

				var currentPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (currentPeriodValue > oldPeriodValue)
				{
					rollbackAndResourceCalculate();
					return false;
				}
			}


			return success;
		}

		private void rollbackAndResourceCalculate()
		{
			IList<DateOnly> toResourceCalculate =
				_rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
			_rollbackService.Rollback();

			foreach (DateOnly dateOnly1 in toResourceCalculate)
			{
			    var currentScheduleDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly1).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly1, currentScheduleDay.ProjectionService().CreateProjection().Period());
			}
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
				schedulingOptions.MainShiftOptimizeActivitySpecification = null;

				var originalMainShift = originalStateContainer.OldPeriodDaysState[dateOnly].GetEditorShift();
				if (originalMainShift != null)
				{
					originalShiftCategory = originalMainShift.ShiftCategory;
					_mainShiftOptimizeActivitySpecificationSetter.SetSpecification(schedulingOptions, _optimizerPreferences,
					                                                               originalMainShift, dateOnly);
				}

				var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
				
				bool schedulingResult;
				if (effectiveRestriction.ShiftCategory == null && originalShiftCategory != null)
				{
                	var possibleShiftOption = new PossibleStartEndCategory {ShiftCategory = originalShiftCategory};
                    schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, possibleShiftOption, _rollbackService);
					if (!schedulingResult)
						schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService);
				}
				else
					schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService);

				if (!schedulingResult)
				{
					int iterations = 0;
					while (_nightRestWhiteSpotSolverService.Resolve(matrix, schedulingOptions, _rollbackService) && iterations < 10)
					{
						iterations++;
					}

					if (originalStateContainer.IsFullyScheduled())
						return true;

					IList<DateOnly> toResourceCalculate = _rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
					_rollbackService.Rollback();
					foreach (DateOnly dateOnly1 in toResourceCalculate)
					{
                        var currentScheduleDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly1).DaySchedulePart();
                        _resourceCalculateDelayer.CalculateIfNeeded(dateOnly1, currentScheduleDay.ProjectionService().CreateProjection().Period());
					}
					return false;
				}
			}

			return true;
		}


		private bool addDayOff(DateOnly dateOnly, bool handleConflict, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var matrix = _matrixConverter.SourceMatrix;
			var currentPart = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			var changed = new changedDay {DateChanged = dateOnly, PreviousSchedule = currentPart};

			currentPart.DeleteMainShift(currentPart);
			currentPart.CreateAndAddDayOff(_dayOffTemplate);
			_rollbackService.Modify(currentPart);

			changed.CurrentSchedule = matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			resourceCalculateMovedDays(new[] { changed });

			IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(_matrixConverter.SourceMatrix, schedulingOptions, rollbackService);  //resource calculation is done automaticaly

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
				IList<DateOnly> days = _decider.DecideDates(changed.CurrentSchedule, changed.PreviousSchedule);
				foreach (var dateOnly in days)
				{
				    _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null);
				}
			}
		}

		private IEnumerable<DateOnly> removeIllegalWorkTimeDays(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			_workTimeBackToLegalStateService.Execute(matrix, schedulingOptions, rollbackService);
			var removedIllegalDates = _workTimeBackToLegalStateService.RemovedDays;
			//resource calculate removed days
			foreach (DateOnly dateOnly in removedIllegalDates)
			{
                var currentScheduleDay = _matrixConverter.SourceMatrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, currentScheduleDay.ProjectionService().CreateProjection().Period());
			}

			return removedIllegalDates;
		}


		private IList<DateOnly> restrictionsOverMax()
		{
			return _optimizationOverLimitDecider.OverLimit(); //maybe send in matrix to get the days locked
		}

		private bool daysOverMax()
		{
			return _optimizationOverLimitDecider.MoveMaxDaysOverLimit();
		}

		private class changedDay
		{
			public DateOnly DateChanged { get; set; }
			public IScheduleDay PreviousSchedule { get; set; }
			public IScheduleDay CurrentSchedule { get; set; }
		}
	}
}