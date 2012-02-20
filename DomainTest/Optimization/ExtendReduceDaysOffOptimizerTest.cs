using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ExtendReduceDaysOffOptimizerTest
    {
        private MockRepository _mocks;
        private IExtendReduceDaysOffOptimizer _target;
        private IPeriodValueCalculator _personalSkillPeriodValueCalculator;
        private IScheduleResultDataExtractor _personalScheduleResultDataExtractor;
        private IExtendReduceDaysOffDecisionMaker _decisionMaker;
        private IScheduleMatrixLockableBitArrayConverter _matrixConverter;
        private IScheduleService _scheduleServiceForFlexibleAgents;
        private IOptimizationPreferences _optimizerPreferences;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IResourceCalculateDaysDecider _decider;
        private IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private IWorkShiftBackToLegalStateServicePro _workTimeBackToLegalStateService;
        private INightRestWhiteSpotSolverService _nightRestWhiteSpotSolverService;
        private IList<IDayOffLegalStateValidator> _validatorList;
        private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
        private IDayOffTemplate _dayOffTemplate;
        private IDayOffOptimizerConflictHandler _dayOffOptimizerConflictHandler;
        private IDayOffOptimizerValidator _dayOffOptimizerValidator;
        private IScheduleMatrixPro _matrix;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private ExtendReduceTimeDecisionMakerResult _extendReduceTimeDecisionMakerResult;
        private IPersonAssignment _personAssignment;
        private IEffectiveRestriction _effectiveRestriction;
        private DateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private ISchedulingOptionsSyncronizer _schedulingOptionsSyncronizer;
        private ISchedulingOptions _schedulingOptions;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personalSkillPeriodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _personalScheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _decisionMaker = _mocks.StrictMock<IExtendReduceDaysOffDecisionMaker>();
            _matrixConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleServiceForFlexibleAgents = _mocks.StrictMock<IScheduleService>();
            _optimizerPreferences = new OptimizationPreferences();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _decider = _mocks.StrictMock<IResourceCalculateDaysDecider>();
            _originalStateContainerForTagChange = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _workTimeBackToLegalStateService = _mocks.StrictMock<IWorkShiftBackToLegalStateServicePro>();
            _nightRestWhiteSpotSolverService = _mocks.StrictMock<INightRestWhiteSpotSolverService>();
            _validatorList = _mocks.StrictMock<IList<IDayOffLegalStateValidator>>();
            _dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
            _dayOffTemplate = _mocks.StrictMock<IDayOffTemplate>();
            _dayOffOptimizerConflictHandler = _mocks.StrictMock<IDayOffOptimizerConflictHandler>();
            _dayOffOptimizerValidator = _mocks.StrictMock<IDayOffOptimizerValidator>();
            _schedulingOptionsSyncronizer = _mocks.StrictMock<ISchedulingOptionsSyncronizer>();
            _schedulingOptions = new SchedulingOptions();

            _target = new ExtendReduceDaysOffOptimizer(_personalSkillPeriodValueCalculator,
                                                       _personalScheduleResultDataExtractor, _decisionMaker,
                                                       _matrixConverter, _scheduleServiceForFlexibleAgents,
                                                       _optimizerPreferences, _rollbackService,
                                                       _resourceOptimizationHelper, _effectiveRestrictionCreator,
                                                       _decider, _originalStateContainerForTagChange,
                                                       _workTimeBackToLegalStateService,
                                                       _nightRestWhiteSpotSolverService, _validatorList,
                                                       _dayOffsInPeriodCalculator, _dayOffTemplate,
                                                       _dayOffOptimizerConflictHandler, _dayOffOptimizerValidator, 
                                                       _schedulingOptionsSyncronizer);
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _extendReduceTimeDecisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            DateTime mainShiftStart = new DateTime(2012, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod mainShiftPeriod = new DateTimePeriod(mainShiftStart, mainShiftStart.AddHours(8));
            _personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(),
                                                                                      mainShiftPeriod);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldSuccess()
        {
            _extendReduceTimeDecisionMakerResult.DayToLengthen = DateOnly.MaxValue;
            _extendReduceTimeDecisionMakerResult.DayToShorten = DateOnly.MinValue;
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                             new WorkTimeLimitation(), null, null, null,
                                                             new List<IActivityRestriction>());
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, new CccTimeZoneInfo(TimeZoneInfo.Utc));

            using (_mocks.Record())
            {
                commonMocks();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly>{DateOnly.MinValue});
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(
                    new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
                Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, true,
                                                                                  _personAssignment.MainShift.
                                                                                      ShiftCategory)).Return(true).Repeat.AtLeastOnce();

                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_dayOffTemplate));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_decider.DecideDates(_scheduleDay, _scheduleDay)).Return(new List<DateOnly>
                                                                                         {DateOnly.MinValue});
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dayOffOptimizerValidator.Validate(_extendReduceTimeDecisionMakerResult.DayToShorten.Value,
                                                               _matrix)).Return(true);

                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(1);
                Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.Execute();
            }

            Assert.IsTrue(ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldRollbackAndFailIfNotPeriodValueIsBetter()
        {
            _extendReduceTimeDecisionMakerResult.DayToLengthen = DateOnly.MaxValue;
            _extendReduceTimeDecisionMakerResult.DayToShorten = DateOnly.MinValue;
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                             new WorkTimeLimitation(), null, null, null,
                                                             new List<IActivityRestriction>());
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, new CccTimeZoneInfo(TimeZoneInfo.Utc));

            using (_mocks.Record())
            {
                commonMocks();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
                    .Return(_effectiveRestriction).Repeat.Times(2);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(
                    new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
                 Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, true,
                                                                                  _personAssignment.MainShift.
                                                                                      ShiftCategory)).Return(true).Repeat.AtLeastOnce();

                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_schedulePeriod)).Return(true);

                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(100);
                Expect.Call(_rollbackService.ModificationCollection).Return(
                    new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _scheduleDay }));
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_dateOnlyAsDateTimePeriod.DateOnly, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_dateOnlyAsDateTimePeriod.DateOnly.AddDays(1), true, true));
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.Execute();
            }

            Assert.IsFalse(ret);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldFailIfRescheduleWhiteSpotsFails()
        {
            _extendReduceTimeDecisionMakerResult.DayToLengthen = DateOnly.MaxValue;
            _extendReduceTimeDecisionMakerResult.DayToShorten = DateOnly.MinValue;
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                             new WorkTimeLimitation(), null, null, null,
                                                             new List<IActivityRestriction>());
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, new CccTimeZoneInfo(TimeZoneInfo.Utc));

            using (_mocks.Record())
            {
                commonMocks();
                int x;
                int y;
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, 0);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(
                    new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
                Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, true,
                                                                                  _personAssignment.MainShift.
                                                                                      ShiftCategory)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, true,
                                                                                  _effectiveRestriction)).Return(false).Repeat.AtLeastOnce();
                Expect.Call(_nightRestWhiteSpotSolverService.Resolve(_matrix)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_originalStateContainerForTagChange.IsFullyScheduled()).Return(false);
                Expect.Call(_rollbackService.ModificationCollection).Return(
                    new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay>{ _scheduleDay }));
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(
                    () =>
                    _resourceOptimizationHelper.ResourceCalculateDate(_dateOnlyAsDateTimePeriod.DateOnly, true, true));
                Expect.Call(
                    () =>
                    _resourceOptimizationHelper.ResourceCalculateDate(_dateOnlyAsDateTimePeriod.DateOnly.AddDays(1), true, true));
                Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue)));

                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_schedulePeriod)).Return(true);

            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.Execute();
            }

            Assert.IsFalse(ret);
        }

        [Test]
        public void VerifyOwner()
        {
            IPerson person = PersonFactory.CreatePerson();
            using (_mocks.Record())
            {
                Expect.Call(_matrixConverter.SourceMatrix).Return(_matrix);
                Expect.Call(_matrix.Person).Return(person);
            }
            using (_mocks.Playback())
            {
                Assert.AreSame(person, _target.Owner);
            }
        }

        private void commonMocks()
        {
            Expect.Call(() => _rollbackService.ClearModificationCollection());
            Expect.Call(_matrixConverter.SourceMatrix).Return(_matrix).Repeat.Any();
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_scheduleServiceForFlexibleAgents.SchedulingOptions)
                .Return(_schedulingOptions).Repeat.AtLeastOnce();
            Expect.Call(() => _schedulingOptionsSyncronizer.SyncronizeSchedulingOption(_optimizerPreferences, _schedulingOptions)).Repeat.AtLeastOnce();
            Expect.Call(_matrix.GetScheduleDayByKey(_extendReduceTimeDecisionMakerResult.DayToLengthen.Value))
                .Return(_scheduleDayPro).Repeat.Any();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay)
                .Repeat.Any();
            Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment)
                .Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_extendReduceTimeDecisionMakerResult.DayToShorten.Value))
                .Return(_scheduleDayPro).Repeat.Any();
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
                .Return(_effectiveRestriction);
        }
    }
}