using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffDecisionMakerExecuterTest
    {
        private DayOffDecisionMakerExecuter _target;
        private MockRepository _mocks;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IPeriodValueCalculator _periodValueCalculator;
        private IWorkShiftBackToLegalStateServicePro _workShiftBackToLegalStateService;
        private ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private IDayOffTemplate _dayOffTemplate;
        private IOptimizationPreferences _optimizerPreferences;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IScheduleService _scheduleService;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IScheduleDayPro _scheduleDayPro;
        private IResourceCalculateDaysDecider _decider;
        private IEffectiveRestriction _effectiveRestriction;
        private IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IResourceCalculateDelayer _resourceCalculateDelayer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _workShiftBackToLegalStateService = _mocks.StrictMock<IWorkShiftBackToLegalStateServicePro>();
            _smartDayOffBackToLegalStateService = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _dayOffTemplate = new DayOffTemplate(new Description("Test"));
            _scheduleService = _mocks.StrictMock<IScheduleService>();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _decider = _mocks.StrictMock<IResourceCalculateDaysDecider>();
            _optimizerPreferences = new OptimizationPreferences();
            _optimizerPreferences.DaysOff.ConsiderWeekBefore = false;
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _effectiveRestrictionCreator = _mocks.DynamicMock<IEffectiveRestrictionCreator>();
            _dayOffOptimizerConflictHandler = _mocks.StrictMock<IDayOffOptimizerConflictHandler>();
            _dayOffOptimizerValidator = _mocks.StrictMock<IDayOffOptimizerValidator>();
            _optimizationOverLimitDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
            _nightRestWhiteSpotSolverService = _mocks.StrictMock<INightRestWhiteSpotSolverService>();
            _schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
        }


        [Test]
        public void VerifyExecuteReturnsTrue()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null)
                                                       {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null)
                                                      {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayAfterMove.Set(1, true);
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> outerWeekList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2,
                                                           scheduleDay3,
                                                           scheduleDay4,
                                                           scheduleDay5,
                                                           scheduleDay6,
                                                           scheduleDay7,
                                                           scheduleDay8,
                                                           scheduleDay9
                                                       };
            var part = _mocks.StrictMock<IScheduleDay>();
            var dateOnlyPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var dateOnly = new DateOnly();

            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(schedulingOptions);
				Expect.Call(_originalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.Once();
                 _rollbackService.ClearModificationCollection();
                Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
                    .Return(new ReadOnlyCollection<IScheduleDayPro>(outerWeekList)).Repeat.Times(4);
                Expect.Call(scheduleDay8.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(scheduleDay9.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(part.Clone()).Return(part).Repeat.Twice();
                part.DeleteMainShift(part);
                part.CreateAndAddDayOff(_dayOffTemplate);
                part.DeleteDayOff();
                _rollbackService.Modify(part);
                LastCall.Repeat.Twice();
                Expect.Call(_decider.DecideDates(part, part))
                    .Return(new List<DateOnly>{ DateOnly.MinValue}).Repeat.Twice();
                Expect.Call(scheduleDay8.Day)
                    .Return(new DateOnly(2010, 1, 1)).Repeat.Twice();
                Expect.Call(scheduleDay9.Day)
                    .Return(new DateOnly(2010, 1, 2)).Repeat.Twice();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true)).Repeat.Twice();
                Expect.Call(_workShiftBackToLegalStateService.Execute(_scheduleMatrix, schedulingOptions))
                    .Return(true).Repeat.Once();
                Expect.Call(_workShiftBackToLegalStateService.RemovedDays)
                    .Return(new List<DateOnly>());
                Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(bitArrayAfterMove)).IgnoreArguments().Return(null).Repeat.Once();
                Expect.Call(_smartDayOffBackToLegalStateService.Execute(null, 25)).IgnoreArguments()
                    .Return(true).Repeat.Once();
                Expect.Call(_scheduleMatrix.Person)
                    .Return(new Person()).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly())).IgnoreArguments()
                    .Return(_scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(part).Repeat.AtLeastOnce();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null))
                    .Return(_effectiveRestriction).IgnoreArguments();
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, schedulingOptions, true, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true).Repeat.Twice();
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(5).Repeat.Once();
                Expect.Call(part.DateOnlyAsPeriod)
                    .Return(dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(dateOnlyPeriod.DateOnly)
                    .Return(dateOnly);
                Expect.Call(_dayOffOptimizerValidator.Validate(dateOnly, _scheduleMatrix)).Return(true);
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>())
                    .Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                SetExpectationsForSettingOriginalShiftCategory();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute(bitArrayAfterMove, bitArrayBeforeMove, _scheduleMatrix, _originalStateContainer, true, true);
            }

            Assert.IsTrue(result);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRollbackAndReturnFalseIfRescheduleFails()
		{
			ISchedulingOptions schedulingOptions = new SchedulingOptions();

			ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
			bitArrayBeforeMove.Set(0, true);
			ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
			bitArrayAfterMove.Set(1, true);
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> outerWeekList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2,
                                                           scheduleDay3,
                                                           scheduleDay4,
                                                           scheduleDay5,
                                                           scheduleDay6,
                                                           scheduleDay7,
                                                           scheduleDay8,
                                                           scheduleDay9
                                                       };
			var part = _mocks.StrictMock<IScheduleDay>();
			var dateOnlyPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnly = new DateOnly();

			using (_mocks.Record())
			{
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
					.Return(schedulingOptions);
				Expect.Call(_originalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
					.Return(10).Repeat.Once();
				_rollbackService.ClearModificationCollection();
				Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>(outerWeekList)).Repeat.Times(4);
				Expect.Call(scheduleDay8.DaySchedulePart())
					.Return(part).Repeat.Twice();
				Expect.Call(scheduleDay9.DaySchedulePart())
					.Return(part).Repeat.Twice();
				Expect.Call(part.Clone()).Return(part).Repeat.Twice();
				part.DeleteMainShift(part);
				part.CreateAndAddDayOff(_dayOffTemplate);
				part.DeleteDayOff();
				_rollbackService.Modify(part);
				LastCall.Repeat.Twice();
				Expect.Call(_decider.DecideDates(part, part))
					.Return(new List<DateOnly> { DateOnly.MinValue }).Repeat.Twice();
				Expect.Call(scheduleDay8.Day)
					.Return(new DateOnly(2010, 1, 1)).Repeat.Twice();
				Expect.Call(scheduleDay9.Day)
					.Return(new DateOnly(2010, 1, 2)).Repeat.Twice();
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true)).Repeat.Twice();
				Expect.Call(_workShiftBackToLegalStateService.Execute(_scheduleMatrix, schedulingOptions))
					.Return(true).Repeat.Once();
				Expect.Call(_workShiftBackToLegalStateService.RemovedDays)
					.Return(new List<DateOnly>());
				Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(bitArrayAfterMove)).IgnoreArguments().Return(null).Repeat.Once();
				Expect.Call(_smartDayOffBackToLegalStateService.Execute(null, 25)).IgnoreArguments()
					.Return(true).Repeat.Once();
				Expect.Call(_scheduleMatrix.Person)
					.Return(new Person()).Repeat.Any();
				Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly())).IgnoreArguments()
					.Return(_scheduleDayPro).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(part).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null))
					.Return(_effectiveRestriction).IgnoreArguments();
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, schedulingOptions, true, _resourceCalculateDelayer, null)).
					IgnoreArguments()
					.Return(false);
				//Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
				//    .Return(5).Repeat.Once();
				Expect.Call(part.DateOnlyAsPeriod)
					.Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(dateOnlyPeriod.DateOnly)
					.Return(dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_dayOffOptimizerValidator.Validate(dateOnly, _scheduleMatrix)).Return(true);
				Expect.Call(_optimizationOverLimitDecider.OverLimit())
					.Return(new List<DateOnly>())
					.Repeat.AtLeastOnce();
				Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
					.Return(false).Repeat.AtLeastOnce();
				SetExpectationsForSettingOriginalShiftCategory();
				Expect.Call(_nightRestWhiteSpotSolverService.Resolve(_scheduleMatrix, schedulingOptions)).Return(false).Repeat.Times
					(1);
				Expect.Call(_originalStateContainer.IsFullyScheduled()).Return(false);
				Expect.Call(_rollbackService.ModificationCollection).Return(
					new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> {part}));
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly().AddDays(1), true, true));

				//rollback 2 moved days
				Expect.Call(part.Clone()).Return(part);
				Expect.Call(part.Clone()).Return(part);
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_decider.DecideDates(part, part))
					.Return(new List<DateOnly> { new DateOnly() }).Repeat.Twice();
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, true)).Repeat.Twice();
			}

			bool result;

			using (_mocks.Playback())
			{
				_target = createTarget();
				result = _target.Execute(bitArrayAfterMove, bitArrayBeforeMove, _scheduleMatrix, _originalStateContainer, true, true);
			}

			Assert.IsFalse(result);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnFalseWhenBreakingDayOffRule()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> outerWeekList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2,
                                                           scheduleDay3,
                                                           scheduleDay4,
                                                           scheduleDay5,
                                                           scheduleDay6,
                                                           scheduleDay7,
                                                           scheduleDay8,
                                                           scheduleDay9
                                                       };
            var part = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(schedulingOptions);
            	Expect.Call(_originalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.Once();
                _rollbackService.ClearModificationCollection();
                Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
                    .Return(new ReadOnlyCollection<IScheduleDayPro>(outerWeekList)).Repeat.Times(4);
                Expect.Call(scheduleDay8.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(scheduleDay9.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(part.Clone())
                    .Return(part).Repeat.AtLeastOnce();
                part.DeleteMainShift(part);
                part.CreateAndAddDayOff(_dayOffTemplate);
                part.DeleteDayOff();
                Expect.Call(()=>_rollbackService.Modify(part)).Repeat.Twice();
                Expect.Call(_decider.DecideDates(part, part))
                    .Return(new List<DateOnly> { DateOnly.MinValue }).Repeat.Twice();
                Expect.Call(scheduleDay8.Day)
                    .Return(new DateOnly(2010, 1, 1)).Repeat.Twice();
                Expect.Call(scheduleDay9.Day)
                    .Return(new DateOnly(2010, 1, 2)).Repeat.Twice();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true)).Repeat.Twice();
                expectsBreakingDayOffRule(part, bitArrayAfterMove);
                Expect.Call(_dayOffOptimizerConflictHandler.HandleConflict(schedulingOptions, new DateOnly()))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>());
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute(bitArrayAfterMove, bitArrayBeforeMove, _scheduleMatrix, _originalStateContainer, true, true);
            }

            Assert.IsFalse(result);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void ShouldReturnTrueWhenBreakingDayOffRuleButHandleConflict()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> outerWeekList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2,
                                                           scheduleDay3,
                                                           scheduleDay4,
                                                           scheduleDay5,
                                                           scheduleDay6,
                                                           scheduleDay7,
                                                           scheduleDay8,
                                                           scheduleDay9
                                                       };
            var part = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(schedulingOptions);
				Expect.Call(_originalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.Once();
                _rollbackService.ClearModificationCollection();
                Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
                    .Return(new ReadOnlyCollection<IScheduleDayPro>(outerWeekList)).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay8.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay9.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(part.Clone()).Return(part).Repeat.AtLeastOnce();
                part.DeleteMainShift(part);
                part.CreateAndAddDayOff(_dayOffTemplate);
                part.DeleteDayOff();
                Expect.Call(() => _rollbackService.Modify(part)).Repeat.Twice();
                Expect.Call(_decider.DecideDates(part, part))
                    .Return(new List<DateOnly> { DateOnly.MinValue }).Repeat.Twice();
                Expect.Call(scheduleDay8.Day)
                    .Return(new DateOnly(2010, 1, 1)).Repeat.Twice();
                Expect.Call(scheduleDay9.Day)
                    .Return(new DateOnly(2010, 1, 2)).Repeat.Twice();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true)).Repeat.Twice();
                Expect.Call(_workShiftBackToLegalStateService.Execute(_scheduleMatrix, schedulingOptions))
                    .Return(true).Repeat.Once();
                Expect.Call(_workShiftBackToLegalStateService.RemovedDays)
                    .Return(new List<DateOnly>());
                expectsBreakingDayOffRule(part, bitArrayAfterMove);
                Expect.Call(_dayOffOptimizerConflictHandler.HandleConflict(schedulingOptions, new DateOnly()))
                    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, schedulingOptions, true, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true).Repeat.Twice();
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(5).Repeat.AtLeastOnce();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null)).IgnoreArguments()
                    .Return(_effectiveRestriction).Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                    .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                SetExpectationsForSettingOriginalShiftCategory();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute(bitArrayAfterMove, bitArrayBeforeMove, _scheduleMatrix, _originalStateContainer, true, true);
            }

            Assert.IsTrue(result);
        }

        private void SetExpectationsForSettingOriginalShiftCategory()
        {

            var personAssignment = _mocks.StrictMock<IPersonAssignment>();
            var mainShift = _mocks.StrictMock<IMainShift>();
            var shiftCategory = _mocks.StrictMock<IShiftCategory>();
             var originalSchedulePart = _mocks.StrictMock<IScheduleDay>();

             Expect.Call(_effectiveRestriction.ShiftCategory).Return(shiftCategory).Repeat.Any();

            Expect.Call(originalSchedulePart.AssignmentHighZOrder()).Return(personAssignment).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.MainShift).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(mainShift.ShiftCategory).Return(shiftCategory).Repeat.AtLeastOnce();

            IDictionary<DateOnly, IScheduleDay> originalScheuduleDays = new Dictionary<DateOnly, IScheduleDay>();
            originalScheuduleDays.Add(new DateOnly(2010, 1, 1), originalSchedulePart);
            originalScheuduleDays.Add(new DateOnly(2010, 1, 2), originalSchedulePart);
            originalScheuduleDays.Add(DateOnly.Today, originalSchedulePart);

            Expect.Call(_originalStateContainer.OldPeriodDaysState).Return(originalScheuduleDays).Repeat.AtLeastOnce();
        }


        private void expectsBreakingDayOffRule(IScheduleDay part, ILockableBitArray bitArrayAfterMove)
        {
            var dateOnlyPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var dateOnly = new DateOnly();

            Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(bitArrayAfterMove)).IgnoreArguments().Return(null).Repeat.Once();
            Expect.Call(_smartDayOffBackToLegalStateService.Execute(null, 25)).IgnoreArguments().Return(true).Repeat.Once();
            Expect.Call(_scheduleMatrix.Person).Return(new Person()).Repeat.Any();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(_scheduleDayPro).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(part).Repeat.AtLeastOnce();
            Expect.Call(part.DateOnlyAsPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
            Expect.Call(dateOnlyPeriod.DateOnly).Return(dateOnly).Repeat.Twice();
            Expect.Call(_dayOffOptimizerValidator.Validate(dateOnly, _scheduleMatrix)).Return(false);
        }

        [Test]
        public void VerifyExecuteWithBackToLegalState()
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null)
                                                       {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null)
                                                      {PeriodArea = new MinMax<int>(0, 1)};
            bitArrayAfterMove.Set(1, true);
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay3 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay4 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay5 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay6 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay7 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay8 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay9 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> outerWeekList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2,
                                                           scheduleDay3,
                                                           scheduleDay4,
                                                           scheduleDay5,
                                                           scheduleDay6,
                                                           scheduleDay7,
                                                           scheduleDay8,
                                                           scheduleDay9
                                                       };
            var part = _mocks.StrictMock<IScheduleDay>();
            var dateOnlyPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var dateOnly = new DateOnly();
            
            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(schedulingOptions);
				Expect.Call(_originalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.Once();
                _rollbackService.ClearModificationCollection();
                Expect.Call(_scheduleMatrix.OuterWeeksPeriodDays)
                    .Return(new ReadOnlyCollection<IScheduleDayPro>(outerWeekList)).Repeat.Times(4);
                Expect.Call(scheduleDay8.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(scheduleDay9.DaySchedulePart())
                    .Return(part).Repeat.Twice();
                Expect.Call(part.Clone())
                    .Return(part).Repeat.Twice();
                part.DeleteMainShift(part);
                part.CreateAndAddDayOff(_dayOffTemplate);
                part.DeleteDayOff();
                _rollbackService.Modify(part);
                LastCall.Repeat.Twice();
                Expect.Call(_decider.DecideDates(part, part))
                    .Return(new List<DateOnly> { DateOnly.MinValue }).Repeat.Twice();
                Expect.Call(scheduleDay8.Day)
                    .Return(new DateOnly(2010, 1, 1)).Repeat.Twice();
                Expect.Call(scheduleDay9.Day)
                    .Return(new DateOnly(2010, 1, 2)).Repeat.Twice();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true)).Repeat.Twice();
                Expect.Call(_scheduleMatrix.Person)
                    .Return(new Person()).Repeat.Any();
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(null, null))
                    .Return(_effectiveRestriction).Repeat.AtLeastOnce().IgnoreArguments();
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, schedulingOptions, true, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true).Repeat.Twice();
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(5).Repeat.Once();
                Expect.Call(_workShiftBackToLegalStateService.Execute(_scheduleMatrix, schedulingOptions))
                    .Return(true);
                Expect.Call(_workShiftBackToLegalStateService.RemovedDays)
                    .Return(new List<DateOnly> { DateOnly.Today });
                Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(bitArrayAfterMove))
                    .Return(null).Repeat.Once();
                Expect.Call(_smartDayOffBackToLegalStateService.Execute(null, 25)).IgnoreArguments()
                    .Return(true).Repeat.Once();
				Expect.Call(_scheduleService.SchedulePersonOnDay(null, schedulingOptions, true, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true).Repeat.Once();
                _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.Today, true, true);
                LastCall.IgnoreArguments().Repeat.Twice();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(new DateOnly())).IgnoreArguments()
                    .Return(_scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(part.DateOnlyAsPeriod)
                    .Return(dateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(dateOnlyPeriod.DateOnly)
                    .Return(dateOnly);
                Expect.Call(_dayOffOptimizerValidator.Validate(dateOnly, _scheduleMatrix))
                    .Return(true);
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>())
                    .Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                SetExpectationsForSettingOriginalShiftCategory();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute(bitArrayAfterMove, bitArrayBeforeMove, _scheduleMatrix, _originalStateContainer, true, true);
            }

            Assert.IsTrue(result);
        }

        private DayOffDecisionMakerExecuter createTarget()
        {
			var mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

            return new DayOffDecisionMakerExecuter(
                                      _rollbackService,
                                      _smartDayOffBackToLegalStateService,
                                      _dayOffTemplate,
									  _scheduleService,
                                      _optimizerPreferences,
                                      _periodValueCalculator,
                                      _workShiftBackToLegalStateService,
                                      _effectiveRestrictionCreator, 
                                      _resourceOptimizationHelper,
                                      _decider,
                                      _dayOffOptimizerValidator,
                                      _dayOffOptimizerConflictHandler,
                                      _originalStateContainer, 
                                      _optimizationOverLimitDecider,
                                      _nightRestWhiteSpotSolverService,
                                      _schedulingOptionsCreator,
									  mainShiftOptimizeActivitySpecificationSetter
                                      );
        }
    }


}
