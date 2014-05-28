using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
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
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IResourceCalculateDaysDecider _decider;
        private IScheduleMatrixOriginalStateContainer _originalStateContainerForTagChange;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private ISchedulingOptions _schedulingOptions;

        private IScheduleMatrixPro _matrix;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDayPro _scheduleDayPro2;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IEffectiveRestriction _effectiveRestriction;
        private IOptimizationPreferences _optimizerPreferences;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
    	private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
    	private IDictionary<DateOnly, IScheduleDay> _dic;
	    private OverLimitResults _overLimtitesResult;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
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
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_decider = _mocks.StrictMock<IResourceCalculateDaysDecider>();
			_originalStateContainerForTagChange = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_optimizerPreferences = new OptimizationPreferences();
			_optimizationOverLimitDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			_schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_mainShiftOptimizeActivitySpecificationSetter =
				_mocks.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_target = new ExtendReduceTimeOptimizer(
				_periodValueCalculator,
				_dataExtractor,
				_decisionMaker,
				_scheduleMatrixLockableBitArrayConverter,
				_scheduleService,
				_optimizerPreferences,
				_rollbackService,
				_deleteService,
                _resourceCalculateDelayer,
				_effectiveRestrictionCreator,
				_decider,
				_originalStateContainerForTagChange,
				_optimizationOverLimitDecider,
				_schedulingOptions,
				_mainShiftOptimizeActivitySpecificationSetter);

			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_dic = new Dictionary<DateOnly, IScheduleDay>();
			_dic.Add(new DateOnly(2011, 1, 1), _scheduleDay1);
			_dic.Add(new DateOnly(2011, 1, 2), _scheduleDay2);
			_effectiveRestriction = _mocks.Stub<IEffectiveRestriction>();
		    _overLimtitesResult = new OverLimitResults(0, 0, 0, 0, 0);
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

                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(false);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 1)))
                    .Return(true).Repeat.Any();
	            Expect.Call(_optimizationOverLimitDecider.HasOverLimitIncreased(_overLimtitesResult)).Return(false);
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

                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 2)))
                    .Return(false).Repeat.Any();
				Expect.Call(_optimizationOverLimitDecider.HasOverLimitIncreased(_overLimtitesResult)).Return(false);
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

                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
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

                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_rollbackService.ModificationCollection).Return(new List<IScheduleDay>()).Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay1, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_scheduleDay2, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(30);
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
                    .Return(100);
                Expect.Call(_originalStateContainerForTagChange.WorkShiftChanged(new DateOnly(2011, 1, 2)))
                    .Return(false).Repeat.Any();
				Expect.Call(_optimizationOverLimitDecider.HasOverLimitIncreased(_overLimtitesResult)).Return(false);
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
				Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimtitesResult);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void commonMocks(ExtendReduceTimeDecisionMakerResult decisionMakerResult)
		{
			Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimtitesResult);
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
            Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).PropertyBehavior().Return(WorkShiftLengthHintOption.Long).Repeat.Any();
            Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 1, 1), null)).Return(true).Repeat.Any();
            Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(2011, 1, 2), null)).Return(true).Repeat.Any();
            Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1))).Repeat.AtLeastOnce();
            Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(2011, 1, 2, 2011, 1, 2))).Repeat.AtLeastOnce();
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).IgnoreArguments()
                .Return(_effectiveRestriction);
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay2, _schedulingOptions)).IgnoreArguments()
                .Return(_effectiveRestriction);
			Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(_dic).Repeat.Any();
			Expect.Call(_scheduleDay1.GetEditorShift()).Return(EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers()).
				Repeat.Any();
			Expect.Call(_scheduleDay2.GetEditorShift()).Return(EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers()).
				Repeat.Any();
			Expect.Call(
				() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue))
				.IgnoreArguments().Repeat.Twice();
        }
    }
}