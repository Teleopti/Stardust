using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private readonly ExtendReduceDaysOffDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleServiceForFlexibleAgents;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
	    private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ScheduleChangesAffectedDates _decider;
		private readonly IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
		private readonly IWorkShiftBackToLegalStateServicePro _workTimeBackToLegalStateService;
		private readonly INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
		private readonly IList<IDayOffLegalStateValidator> _validatorList;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IDayOffTemplate _dayOffTemplate;
		private readonly IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
		private readonly DayOffOptimizerValidator _dayOffOptimizerValidator;
		private readonly OptimizationLimits _optimizationLimits;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private readonly IScheduleMatrixPro _matrix;
		private readonly IScheduleDictionary _scheduleDictionary;

		public ExtendReduceDaysOffOptimizer(
			IPeriodValueCalculator periodValueCalculator,
			IScheduleResultDataExtractor personalSkillsDataExtractor,
			ExtendReduceDaysOffDecisionMaker decisionMaker,
			IScheduleService scheduleServiceForFlexibleAgents,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ScheduleChangesAffectedDates decider,
			IScheduleMatrixOriginalStateContainer originalStateContainerForTagChange,
			IWorkShiftBackToLegalStateServicePro workTimeBackToLegalStateService,
			INightRestWhiteSpotSolverService nightRestWhiteSpotSolverService,
			IList<IDayOffLegalStateValidator> validatorList,
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IDayOffTemplate dayOffTemplate,
			IDayOffOptimizerConflictHandler dayOffOptimizerConflictHandler,
			DayOffOptimizerValidator dayOffOptimizerValidator,
			OptimizationLimits optimizationLimits,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter,
			IScheduleMatrixPro matrix, 
			IScheduleDictionary scheduleDictionary
			)
		{
			_periodValueCalculator = periodValueCalculator;
			_personalSkillsDataExtractor = personalSkillsDataExtractor;
			_decisionMaker = decisionMaker;
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
			_optimizationLimits = optimizationLimits;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_mainShiftOptimizeActivitySpecificationSetter = mainShiftOptimizeActivitySpecificationSetter;
			_matrix = matrix;
			_scheduleDictionary = scheduleDictionary;
		}

		public bool Execute()
		{
			var lastOverLimitCount = _optimizationLimits.OverLimitsCounts(_matrix);
			if (daysOverMax())
				return false;

			_rollbackService.ClearModificationCollection();

			var schedulePeriod = _matrix.SchedulePeriod;
			if (!_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_scheduleDictionary, schedulePeriod, out _, out _))
				return false;

			var success = false;

			var daysToBeRescheduled = _decisionMaker.Execute(_matrix, _personalSkillsDataExtractor, _validatorList);
			if (!daysToBeRescheduled.DayToLengthen.HasValue && !daysToBeRescheduled.DayToShorten.HasValue)
				return false;

			var oldPeriodValue = _periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization);
			
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences);

			if (daysToBeRescheduled.DayToLengthen.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_scheduleDictionary, schedulePeriod))
			{
				var dayToLengthen = daysToBeRescheduled.DayToLengthen.Value;
				var schedulePart = _matrix.GetScheduleDayByKey(dayToLengthen).DaySchedulePart();
				var changedDayOff = new changedDay {PreviousSchedule = schedulePart, DateChanged = dayToLengthen};

				schedulePart.DeleteDayOff();
				_rollbackService.Modify(schedulePart);

				changedDayOff.CurrentSchedule = _matrix.GetScheduleDayByKey(dayToLengthen).DaySchedulePart();

				IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(schedulingOptions, _rollbackService);  //resource calculation is done automaticaly

				if (rescheduleWhiteSpots(new[] { changedDayOff }, illegalDays, _originalStateContainerForTagChange, schedulingOptions))
					success = true;
				// bugfix for infinie loop 19889. We need to lock the day for avoiding infinitive loop
				_matrix.LockDay(daysToBeRescheduled.DayToLengthen.Value);
			}

			if (daysToBeRescheduled.DayToShorten.HasValue && !_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_scheduleDictionary, schedulePeriod))
			{
				var dayToShorten = daysToBeRescheduled.DayToShorten.Value;
				if (addDayOff(dayToShorten, true, schedulingOptions, _rollbackService))
					success = true;
				// bugfix for infinie loop 19889. We need to lock the day for avoiding infinitive loop
				_matrix.LockDay(daysToBeRescheduled.DayToShorten.Value);
			}

			if (success)
			{
				if (daysOverMax())
				{
					rollbackAndResourceCalculate();
					return false;
				}

				if (_optimizationLimits.HasOverLimitExceeded(lastOverLimitCount, _matrix))
				{
					rollbackAndResourceCalculate();
					return true;	
				}

				var minWorkTimePerWeekOk = _optimizationLimits.ValidateMinWorkTimePerWeek(_matrix);

				if (!minWorkTimePerWeekOk)
				{
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

		public IPerson Owner { get { return _matrix.Person; } }

		private void rollbackAndResourceCalculate()
		{
			IList<DateOnly> toResourceCalculate =
				_rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
			_rollbackService.Rollback();

			foreach (DateOnly dateOnly1 in toResourceCalculate)
			{
				var currentScheduleDay = _matrix.GetScheduleDayByKey(dateOnly1).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly1, currentScheduleDay.ProjectionService().CreateProjection().Period(), false);
			}
		}

		private bool rescheduleWhiteSpots(
			IEnumerable<changedDay> movedDates,
			IEnumerable<DateOnly> removedIllegalWorkTimeDays,
			IScheduleMatrixOriginalStateContainer originalStateContainer,
			SchedulingOptions schedulingOptions)
		{
			var toSchedule = movedDates.Select(changedDay => changedDay.DateChanged).ToList();
			toSchedule.AddRange(removedIllegalWorkTimeDays);
			toSchedule.Sort();
			foreach (DateOnly dateOnly in toSchedule)
			{
				IScheduleDay schedulePart = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();

				IShiftCategory originalShiftCategory = null;
				schedulingOptions.MainShiftOptimizeActivitySpecification = null;

				var originalMainShift = originalStateContainer.OldPeriodDaysState[dateOnly].GetEditorShift();
				if (originalMainShift != null)
				{
					originalShiftCategory = originalMainShift.ShiftCategory;
					_mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(schedulingOptions, _optimizerPreferences,
					                                                               originalMainShift, dateOnly);
				}

				var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(schedulePart, schedulingOptions);
				
				bool schedulingResult;
				if (effectiveRestriction.ShiftCategory == null && originalShiftCategory != null)
				{
                    schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService);
					if (!schedulingResult)
						schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService);
				}
				else
					schedulingResult = _scheduleServiceForFlexibleAgents.SchedulePersonOnDay(schedulePart, schedulingOptions, effectiveRestriction, _resourceCalculateDelayer, _rollbackService);

				if (!schedulingResult)
				{
					if (_nightRestWhiteSpotSolverService.Resolve(_matrix, schedulingOptions, _rollbackService))
						return true;

					IList<DateOnly> toResourceCalculate = _rollbackService.ModificationCollection.Select(scheduleDay => scheduleDay.DateOnlyAsPeriod.DateOnly).ToList();
					_rollbackService.Rollback();
					foreach (DateOnly dateOnly1 in toResourceCalculate)
					{
                        var currentScheduleDay = _matrix.GetScheduleDayByKey(dateOnly1).DaySchedulePart();
                        _resourceCalculateDelayer.CalculateIfNeeded(dateOnly1, currentScheduleDay.ProjectionService().CreateProjection().Period(), false);
					}
					return false;
				}
			}

			return true;
		}


		private bool addDayOff(DateOnly dateOnly, bool handleConflict, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var currentPart = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			var changed = new changedDay {DateChanged = dateOnly, PreviousSchedule = currentPart};

			currentPart.DeleteMainShift();
			currentPart.CreateAndAddDayOff(_dayOffTemplate);
			_rollbackService.Modify(currentPart);

			changed.CurrentSchedule = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
			resourceCalculateMovedDays(new[] { changed });

			IEnumerable<DateOnly> illegalDays = removeIllegalWorkTimeDays(schedulingOptions, rollbackService);  //resource calculation is done automaticaly

			if (!rescheduleWhiteSpots(new List<changedDay>(), illegalDays, _originalStateContainerForTagChange, schedulingOptions))
				return false;

			if (handleConflict && !_dayOffOptimizerValidator.Validate(currentPart.DateOnlyAsPeriod.DateOnly, _matrix))
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
				var days = _decider.DecideDates(changed.CurrentSchedule, changed.PreviousSchedule);
				foreach (var dateOnly in days)
				{
				    _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
				}
			}
		}

		private IEnumerable<DateOnly> removeIllegalWorkTimeDays(SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService)
		{
			_workTimeBackToLegalStateService.Execute(_matrix, schedulingOptions, rollbackService);
			var removedIllegalDates = _workTimeBackToLegalStateService.RemovedDays;
			//resource calculate removed days
			foreach (DateOnly dateOnly in removedIllegalDates)
			{
                var currentScheduleDay = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
                _resourceCalculateDelayer.CalculateIfNeeded(dateOnly, currentScheduleDay.ProjectionService().CreateProjection().Period(), false);
			}

			return removedIllegalDates;
		}

		private bool daysOverMax()
		{
			return _optimizationLimits.MoveMaxDaysOverLimit();
		}

		private class changedDay
		{
			public DateOnly DateChanged { get; set; }
			public IScheduleDay PreviousSchedule { get; set; }
			public IScheduleDay CurrentSchedule { get; set; }
		}
	}
}