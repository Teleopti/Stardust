using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ExtendReduceTimeOptimizerTest
    {
        private IExtendReduceTimeOptimizer _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IScheduleResultDataExtractor _dataExtractor;
        private IExtendReduceTimeDecisionMaker _decisionMaker;
        private IScheduleMatrixLockableBitArrayConverter _scheduleMatrixLockableBitArrayConverter;
        private IScheduleService _scheduleService;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IDeleteSchedulePartService _deleteService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IResourceCalculateDaysDecider _decider;
        private IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;

        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IEffectiveRestriction _effectiveRestriction;
        private IOptimizationPreferences _optimizerPreferences;
        private ISchedulingOptions _schedulingOptions;
		private IResourceCalculateDelayer _resourceCalculateDelayer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _dataExtractor = _mocks.Stub<IScheduleResultDataExtractor>();
            _decisionMaker = _mocks.StrictMock<IExtendReduceTimeDecisionMaker>();
            _scheduleMatrixLockableBitArrayConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleService = _mocks.StrictMock<IScheduleService>();
            _rollbackService = _mocks.Stub<ISchedulePartModifyAndRollbackService>();
            _deleteService = _mocks.Stub<IDeleteSchedulePartService>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _decider = _mocks.StrictMock<IResourceCalculateDaysDecider>();
            _originalStateContainerForTagChange = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _optimizerPreferences = new OptimizationPreferences();
            _optimizationOverLimitDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
            _schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();

            _target = new ExtendReduceTimeOptimizer(
                _periodValueCalculator, 
                _dataExtractor, 
                _decisionMaker,
                _scheduleMatrixLockableBitArrayConverter, 
                _scheduleService,
                _optimizerPreferences, 
                _rollbackService,
                _deleteService, 
                _resourceOptimizationHelper,
                _effectiveRestrictionCreator, 
                _decider, 
                _originalStateContainerForTagChange, 
                _optimizationOverLimitDecider, 
                _schedulingOptionsCreator);

            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _effectiveRestriction = _mocks.Stub<IEffectiveRestriction>();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void ShouldReturnTrueIfDateToLengthenIsSuccessful()
        {
            ExtendReduceTimeDecisionMakerResult decisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            decisionMakerResult.DayToLengthen = new DateOnly(2011, 1, 1);
            decisionMakerResult.DayToShorten = new DateOnly(2011, 1, 2);
            
            using (_mocks.Record())
            {
                commonMocks(decisionMakerResult);

                Expect.Call(_optimizationOverLimitDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 1)))
                    .Return(true).Repeat.Any();
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsTrue(result);
        }

        [Test]
        public void ShouldReturnTrueIfDateToShortenIsSuccessful()
        {
            ExtendReduceTimeDecisionMakerResult decisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            decisionMakerResult.DayToLengthen = new DateOnly(2011, 1, 1);
            decisionMakerResult.DayToShorten = new DateOnly(2011, 1, 2);

            using (_mocks.Record())
            {
                commonMocks(decisionMakerResult);

                Expect.Call(_optimizationOverLimitDecider.OverLimit()).Return(new List<DateOnly>()).Repeat.Twice();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 2)))
                    .Return(false).Repeat.Any();
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState[new DateOnly(2011, 1, 2)])
                    .Return(_scheduleDay2).Repeat.Any();
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsTrue(result);
        }

        [Test] 
        public void ShouldReturnFalseIfNoDayIsSuccessful()
        {
            ExtendReduceTimeDecisionMakerResult decisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            decisionMakerResult.DayToLengthen = new DateOnly(2011, 1, 1);
            decisionMakerResult.DayToShorten = new DateOnly(2011, 1, 2);

            using (_mocks.Record())
            {
                commonMocks(decisionMakerResult);

                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>());
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldRollbackIfPeriodValueIsIncreasing()
        {
            ExtendReduceTimeDecisionMakerResult decisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            decisionMakerResult.DayToLengthen = new DateOnly(2011, 1, 1);
            decisionMakerResult.DayToShorten = new DateOnly(2011, 1, 2);

            using (_mocks.Record())
            {
                commonMocks(decisionMakerResult);
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>()).Repeat.Twice();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(100);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 2)))
                    .Return(false).Repeat.Any();
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState[new DateOnly(2011, 1, 2)])
                    .Return(_scheduleDay2).Repeat.Any();
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnFalseIfNoDaysFound()
        {
            ExtendReduceTimeDecisionMakerResult decisionMakerResult = new ExtendReduceTimeDecisionMakerResult();

            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>());
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_decisionMaker.Execute(_scheduleMatrixLockableBitArrayConverter, _dataExtractor))
                    .Return(decisionMakerResult);
            }

            bool result;

            using (_mocks.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldExposeOwnerPerson()
        {
            IPerson person = PersonFactory.CreatePerson();
            using (_mocks.Record())
            {
                Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix).Return(_matrix);
                Expect.Call(_matrix.Person).Return(person);

            }

            using (_mocks.Playback())
            {
                Assert.AreEqual(person, _target.Owner);
            }
            
        }

        private void commonMocks(ExtendReduceTimeDecisionMakerResult decisionMakerResult)
        {
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                .Return(_schedulingOptions);
            Expect.Call(_decisionMaker.Execute(_scheduleMatrixLockableBitArrayConverter, _dataExtractor))
                .Return(decisionMakerResult);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                .Return(40);
            Expect.Call(_scheduleMatrixLockableBitArrayConverter.SourceMatrix)
                .Return(_matrix).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(decisionMakerResult.DayToLengthen.Value))
                .Return(_scheduleDayPro1).Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(decisionMakerResult.DayToShorten.Value))
                .Return(_scheduleDayPro2).Repeat.Any();
            Expect.Call(_scheduleDayPro1.DaySchedulePart())
                .Return(_scheduleDay1).Repeat.Any();
            Expect.Call(_scheduleDayPro2.DaySchedulePart())
                .Return(_scheduleDay2).Repeat.Any();
            Expect.Call(_scheduleDay1.Clone())
                .Return(_scheduleDay1).Repeat.Any();
            Expect.Call(_scheduleDay2.Clone())
                .Return(_scheduleDay2).Repeat.Any();
            Expect.Call(_decider.DecideDates(null, null)).IgnoreArguments()
                .Return(new List<DateOnly> { new DateOnly(2011, 1, 1) }).Repeat.Any();
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), true, true)).Repeat.Any();
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 2), true, true)).Repeat.Any();
            Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1))).Repeat.AtLeastOnce();
            Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(2011, 1, 2, 2011, 1, 2))).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).IgnoreArguments()
                .Return(_effectiveRestriction);
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay2, _schedulingOptions)).IgnoreArguments()
                .Return(_effectiveRestriction);
        }
    }
}