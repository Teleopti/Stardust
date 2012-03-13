using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class IntradayOptimizerTest
    {
        private IntradayOptimizer2 _target;
        private MockRepository _mockRepository;
        private IScheduleResultDailyValueCalculator _dailyValueCalculator;
        private IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private IIntradayDecisionMaker _decisionMaker;
        private IScheduleMatrixLockableBitArrayConverter _bitArrayConverter;
        private IScheduleService _scheduleService;
        private IOptimizationPreferences _optimizerPreferences;
        private IScheduleMatrixPro _scheduleMatrix;
        private DateOnly _removedDate;
        private IScheduleDayPro _removedScheduleDay;
        private IScheduleDay _removedSchedulePart;
        private IDictionary<DateOnly, IScheduleDayPro> _fullWeeksPeriodDictionary;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IDeleteSchedulePartService _deleteService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private ISchedulingOptions _schedulingOptions;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IEffectiveRestriction _effectiveRestriction;
        private IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
        private IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
        private IOptimizationOverLimitDecider _optimizationOverLimitDecider;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
        private IReschedulingPreferences _reschedulingPreferences;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _dailyValueCalculator = _mockRepository.StrictMock<IScheduleResultDailyValueCalculator>();
            _personalSkillsDataExtractor = _mockRepository.StrictMock<IScheduleResultDataExtractor>();
            _decisionMaker = _mockRepository.StrictMock<IIntradayDecisionMaker>();
            _bitArrayConverter = _mockRepository.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleService = _mockRepository.StrictMock<IScheduleService>();
            _optimizerPreferences = _mockRepository.StrictMock<IOptimizationPreferences>();
            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _removedDate = new DateOnly(2010, 01, 09);
            _removedScheduleDay = _mockRepository.StrictMock<IScheduleDayPro>();
            _removedSchedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _fullWeeksPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro> { { _removedDate, _removedScheduleDay } };
            _rollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
            _deleteService = _mockRepository.StrictMock<IDeleteSchedulePartService>();
            _resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
            _schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();
            _effectiveRestrictionCreator = _mockRepository.StrictMock<IEffectiveRestrictionCreator>();
            _effectiveRestriction = _mockRepository.StrictMock<IEffectiveRestriction>();
            _resourceCalculateDaysDecider = _mockRepository.StrictMock<IResourceCalculateDaysDecider>();
            _workShiftOriginalStateContainer = _mockRepository.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _optimizationOverLimitDecider = _mockRepository.StrictMock<IOptimizationOverLimitDecider>();
            _schedulingOptionsCreator = _mockRepository.StrictMock<ISchedulingOptionsCreator>();
            _reschedulingPreferences = _mockRepository.StrictMock<IReschedulingPreferences>();


            _target = new IntradayOptimizer2(
                _dailyValueCalculator,
                _personalSkillsDataExtractor,
                _decisionMaker,
                _bitArrayConverter,
                _scheduleService,
                _optimizerPreferences,
                _rollbackService,
                _deleteService,
                _resourceOptimizationHelper,
                _effectiveRestrictionCreator,
                _resourceCalculateDaysDecider,
                _optimizationOverLimitDecider,
                _workShiftOriginalStateContainer,
                _schedulingOptionsCreator
                );
        }

        [Test]
        public void ShouldLockDayAndNoRollbackWhenExecuteWithBetterPeriodValue()
        {
            using (_mockRepository.Record())
            {
                expectationsForSuccessfullExecute();
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void expectationsForSuccessfullExecute()
        {
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                .Return(_schedulingOptions);
            Expect.Call(_optimizationOverLimitDecider.OverLimit())
                 .Return(false).Repeat.AtLeastOnce();
            Expect.Call(_optimizerPreferences.Rescheduling)
                .Return(_reschedulingPreferences).Repeat.AtLeastOnce();
            Expect.Call(_reschedulingPreferences.ConsiderShortBreaks)
                .Return(true).Repeat.AtLeastOnce();
            Expect.Call(_bitArrayConverter.SourceMatrix)
                .Return(_scheduleMatrix).Repeat.Any();

            makePeriodBetter();

            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .Return(_removedDate);
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                .Return(_fullWeeksPeriodDictionary).Repeat.Any();
            Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free).Repeat.Once();
            Expect.Call(_removedScheduleDay.DaySchedulePart())
                .Return(_removedSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_removedDate))
                .Return(_removedScheduleDay).Repeat.AtLeastOnce();
            Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                .Return(null);
            Expect.Call(_resourceCalculateDaysDecider.DecideDates(_removedSchedulePart, _removedSchedulePart))
                .Return(new List<DateOnly> { _removedDate });
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_removedSchedulePart, _schedulingOptions))
                .Return(_effectiveRestriction);
            Expect.Call(_scheduleService.SchedulePersonOnDay(_removedSchedulePart, _schedulingOptions, false, _effectiveRestriction))
                .Return(true);
            Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_removedDate])
                .Return(_removedSchedulePart);
            Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(new DateOnly(2010, 1, 9)))
                .Return(false);
            Expect.Call(() => _rollbackService.Modify(_removedSchedulePart, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();
            Expect.Call(_removedSchedulePart.Clone())
                .Return(_removedSchedulePart).Repeat.AtLeastOnce();
            // lock day
            _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_removedDate, _removedDate));
            // NO rollback
            _rollbackService.ClearModificationCollection();
        }

        private void makePeriodBetter()
        {
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(2);
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(1);
        }

        [Test]
        public void VerifyExecuteWithDecisionMakerFailure()
        {
            using (_mockRepository.Record())
            {
                makeDecisionMakerFailure();
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsFalse(result);
            }
        }

        private void makeDecisionMakerFailure()
        {
            Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                 .Return(false).Repeat.AtLeastOnce();
            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .IgnoreArguments()
                .Return(null);
        }

        [Test]
        public void VerifyExecuteWithWorsePeriodValue()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                     .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_optimizerPreferences.Rescheduling)
                    .Return(_reschedulingPreferences).Repeat.AtLeastOnce();
                Expect.Call(_reschedulingPreferences.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix).Repeat.AtLeastOnce();

                makePeriodWorse();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .Return(_removedDate);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free).Repeat.Once();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_removedDate))
                    .Return(_removedScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.Once();
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                    .Return(null);
                Expect.Call(_removedSchedulePart.Clone())
                    .Return(_removedSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_removedSchedulePart, _removedSchedulePart))
                    .Return(new List<DateOnly> { _removedDate });
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_removedSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_removedSchedulePart, _schedulingOptions, false, _effectiveRestriction))
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_removedDate]).Return(_removedSchedulePart);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(new DateOnly(2010, 1, 9)))
                    .Return(false);
                Expect.Call(() => _rollbackService.Modify(_removedSchedulePart, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();
                Expect.Call(() =>_rollbackService.ClearModificationCollection());
                Expect.Call(() =>_rollbackService.Rollback());
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_removedSchedulePart, _removedSchedulePart))
                    .Return(new List<DateOnly> { _removedDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
                Expect.Call(() =>_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_removedDate, _removedDate)));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        private void makePeriodWorse()
        {
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(1);
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(2);
        }

        [Test]
        public void ShouldReturnFalseButNoRollbackWhenExecuteWithSamePeriodValue()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                     .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_optimizerPreferences.Rescheduling)
                    .Return(_reschedulingPreferences).Repeat.AtLeastOnce();
                Expect.Call(_reschedulingPreferences.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix).Repeat.AtLeastOnce();

                makePeriodSame();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .IgnoreArguments()
                    .Return(_removedDate);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free).Repeat.Once();
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.Once();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_removedDate))
                    .Return(_removedScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_removedSchedulePart.Clone())
                    .Return(_removedSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                    .Return(null);
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_removedSchedulePart, _removedSchedulePart))
                    .Return(new List<DateOnly> { _removedDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_removedSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_removedSchedulePart, _schedulingOptions, false, _effectiveRestriction))
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_removedDate]).Return(_removedSchedulePart);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(new DateOnly(2010, 1, 9)))
                    .Return(false);
                Expect.Call(() => _rollbackService.Modify(_removedSchedulePart, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();
                // NO rollback
                _rollbackService.ClearModificationCollection();
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_removedDate, _removedDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        private void makePeriodSame()
        {
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(2);
            Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments().Return(2);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyExecuteDayCannotScheduled()
        {
            var period = _mockRepository.StrictMock<IDateOnlyAsDateTimePeriod>();

            using (_mockRepository.Record())
            {

                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix).Repeat.AtLeastOnce();
                Expect.Call(_dailyValueCalculator.DayValue(new DateOnly())).IgnoreArguments()
                    .Return(2);
                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).IgnoreArguments()
                    .Return(_removedDate);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.AtLeastOnce();
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                     .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_reschedulingPreferences.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Free).Repeat.Once();
                Expect.Call(_optimizerPreferences.Rescheduling)
                    .Return(_reschedulingPreferences).Repeat.AtLeastOnce();
                Expect.Call(_schedulingOptions.ConsiderShortBreaks)
                    .Return(true).Repeat.Twice();
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.Once();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_removedDate))
                    .Return(_removedScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_removedScheduleDay.DaySchedulePart())
                    .Return(_removedSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_removedSchedulePart.Clone())
                    .Return(_removedSchedulePart);
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                    .Return(null);
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_removedSchedulePart, _removedSchedulePart)).
                    Return(new List<DateOnly> { _removedDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_removedSchedulePart, _schedulingOptions)).IgnoreArguments()
                    .Return(_effectiveRestriction);
                Expect.Call(_scheduleService.SchedulePersonOnDay(_removedSchedulePart, _schedulingOptions, false, _effectiveRestriction)).IgnoreArguments()
                    .Return(false);

                // rollback
                _rollbackService.ClearModificationCollection();
                Expect.Call(_rollbackService.ModificationCollection).Return(
                    new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _removedSchedulePart }));
                _rollbackService.Rollback();
                Expect.Call(_removedSchedulePart.DateOnlyAsPeriod).Return(period);
                Expect.Call(period.DateOnly).Return(_removedDate);
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_removedDate.AddDays(1), true, true));
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_removedDate, _removedDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyContainerOwner()
        {
            IPerson person = PersonFactory.CreatePerson();
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix);
                Expect.Call(_scheduleMatrix.Person).Return(person);
            }

            IPerson owner;
            using (_mockRepository.Playback())
            {
                owner = _target.ContainerOwner;
            }

            Assert.AreSame(person, owner);
        }

        [Test]
        public void ShouldReturnFalseIfPeriodValueIsNull()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                     .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .Return(_removedDate);
                Expect.Call(_dailyValueCalculator.DayValue(_removedDate))
                    .Return(null);
            }

            bool result;
            using (_mockRepository.Playback())
            {
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }


        [Test]
        public void VerifyIsMovedDaysUnderLimit()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                     .Return(false).Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
                Assert.IsFalse(_target.MovedDaysOverMaxDaysLimit());
            }
        }

        [Test]
        public void VerifyIsMovedDaysOverLimit()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
                     .Return(true).Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
                Assert.IsTrue(_target.MovedDaysOverMaxDaysLimit());
            }
        }
    }
}
