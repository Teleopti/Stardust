using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MoveTimeOptimizerTest
    {
        private MoveTimeOptimizer _target;
        private MockRepository _mockRepository;
        private IPeriodValueCalculator _periodValueCalculator;
        private IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private IMoveTimeDecisionMaker _decisionMaker;
        private IScheduleMatrixLockableBitArrayConverter _bitArrayConverter;
        private IScheduleService _scheduleService;
        private IOptimizationPreferences _optimizerPreferences;
        private IScheduleMatrixPro _scheduleMatrix;
        private DateOnly _mostUnderStaffDate;
        private DateOnly _mostOverStaffDate;
        private IScheduleDayPro _mostUnderStaffDay;
        private IScheduleDayPro _mostOverStaffDay;
        private IScheduleDay _mostUnderStaffSchedulePart;
        private IScheduleDay _mostOverStaffSchedulePart;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private ISchedulingOptions _schedulingOptions;
        private IEffectiveRestriction _effectiveRestriction;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
    	private IProjectionService _projectionService;
    	private IVisualLayerCollection _visualLayerCollection;
		private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private IPersonAssignment _personAssignment;
		private IMainShift _mainShift;
		private IDictionary<DateOnly, IScheduleDay> _originalDays;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
			_schedulingOptions = new SchedulingOptions{ConsiderShortBreaks = true};
            _mockRepository = new MockRepository();
			_resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
			_resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true, true);
            _periodValueCalculator = _mockRepository.StrictMock<IPeriodValueCalculator>();
            _personalSkillsDataExtractor = _mockRepository.StrictMock<IScheduleResultDataExtractor>();
            _decisionMaker = _mockRepository.StrictMock<IMoveTimeDecisionMaker>();
            _bitArrayConverter = _mockRepository.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleService = _mockRepository.StrictMock<IScheduleService>();
            _optimizerPreferences = new OptimizationPreferences();
            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _mostOverStaffDate = new DateOnly(2010, 01, 08);
            _mostUnderStaffDate = new DateOnly(2010, 01, 09);
            _mostOverStaffDay = _mockRepository.StrictMock<IScheduleDayPro>();
            _mostUnderStaffDay = _mockRepository.StrictMock<IScheduleDayPro>();
            _mostOverStaffSchedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _mostUnderStaffSchedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _rollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
			_deleteAndResourceCalculateService = _mockRepository.StrictMock<IDeleteAndResourceCalculateService>();
            _resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
            _effectiveRestrictionCreator = _mockRepository.StrictMock<IEffectiveRestrictionCreator>();
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            _workShiftOriginalStateContainer = _mockRepository.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _optimizationOverLimitDecider = _mockRepository.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
            _schedulingOptionsCreator = _mockRepository.StrictMock<ISchedulingOptionsCreator>();
        	_projectionService = _mockRepository.StrictMock<IProjectionService>();
        	_visualLayerCollection = _mockRepository.StrictMock<IVisualLayerCollection>();
			_mainShiftOptimizeActivitySpecificationSetter =
		    _mockRepository.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
		    _projectionService = _mockRepository.StrictMock<IProjectionService>();
		    _personAssignment = _mockRepository.StrictMock<IPersonAssignment>();
		    _mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
		    _originalDays = new Dictionary<DateOnly, IScheduleDay>{{_mostOverStaffDate, _mostOverStaffSchedulePart},{_mostUnderStaffDate, _mostUnderStaffSchedulePart}};

        	_target = new MoveTimeOptimizer(
        		_periodValueCalculator,
        		_personalSkillsDataExtractor,
        		_decisionMaker,
        		_bitArrayConverter,
        		_scheduleService,
        		_optimizerPreferences,
        		_rollbackService,
				_deleteAndResourceCalculateService,
        		_resourceOptimizationHelper,
        		_effectiveRestrictionCreator,
        		_workShiftOriginalStateContainer,
        		_optimizationOverLimitDecider,
        		_schedulingOptionsCreator,
        		_mainShiftOptimizeActivitySpecificationSetter
        		);
        }

		[Test]
		public void ShouldExposeOwnerAndMatrix()
		{
			var person = PersonFactory.CreatePerson();
			using (_mockRepository.Record())
			{
				Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Twice();
				Expect.Call(_scheduleMatrix.Person).Return(person);
			}
			using (_mockRepository.Playback())
			{
				Assert.AreSame(person, _target.ContainerOwner);
				Assert.AreSame(_scheduleMatrix, _target.Matrix);
			}
		}

        [Test]
        public void VerifyExecuteWithBetterPeriodValue()
        {
            using (_mockRepository.Record())
            {
                setupForScheduling();
				tryScheduleFirstDate(true, false);
				tryScheduleSecondDate(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1);
                // do not lock days
				//_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
				//_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyExecuteWithDecisionMakerFailure()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);

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
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                .Return(_schedulingOptions);
        	Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
			Expect.Call(() => _schedulingOptions.UseCustomTargetTime = new TimeSpan());
            Expect.Call(_optimizationOverLimitDecider.OverLimit())
                .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
            Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .IgnoreArguments()
                .Return(new List<DateOnly>());
        }

        [Test]
        public void VerifyExecuteWithWorsePeriodValue()
        {
            using (_mockRepository.Record())
            {
                setupForScheduling();
				tryScheduleFirstDate(true, false);
				tryScheduleSecondDate(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(3);
            	Expect.Call(() => _rollbackService.Rollback());
				resourceCalculation();
                Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Twice();
                Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.Twice();
                // lock days
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyExecuteWithSamePeriodValue()
        {
            using (_mockRepository.Record())
            {
                setupForScheduling();
				tryScheduleFirstDate(true, false);
				tryScheduleSecondDate(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);
                //do not lock days
				//_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
				//_scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyExecuteSecondDayCannotScheduled()
        {

            using (_mockRepository.Record())
            {
				setupForScheduling();
				tryScheduleFirstDate(true, false);
				tryScheduleSecondDate(false);
                resourceCalculation();
                // lock days
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

		[Test]
		public void VerifyExecuteFirstDayCannotScheduled()
		{
			using (_mockRepository.Record())
			{
				setupForScheduling();
				tryScheduleFirstDate(false, false);
                resourceCalculation();

                // lock days
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
			}

			using (_mockRepository.Playback())
			{
				bool result = _target.Execute();
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldRollbackIfSameContractTime()
		{
		    using (_mockRepository.Record())
			{
				setupForScheduling();
				tryScheduleFirstDate(true, true);
                resourceCalculation();
                // lock days
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
			}

			using (_mockRepository.Playback())
			{
				bool result = _target.Execute();
				Assert.IsTrue(result);
			}
		}

		private void setupForScheduling()
		{
			Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
					.Return(_schedulingOptions);
		    Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
			Expect.Call(() => _schedulingOptions.UseCustomTargetTime = new TimeSpan());
			Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
				 .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
			Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
				.Return(false).Repeat.AtLeastOnce();
			Expect.Call(_bitArrayConverter.SourceMatrix)
				.Return(_scheduleMatrix).Repeat.AtLeastOnce();
			Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
				.Return(2);
			Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).
				Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });

			_rollbackService.ClearModificationCollection();

			//first date
			Expect.Call(_mostUnderStaffDay.DaySchedulePart())
				.Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
				.Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
            Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
            Expect.Call(_mostUnderStaffSchedulePart.Clone())
                .Return(_mostUnderStaffSchedulePart);
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions)).IgnoreArguments()
				.Return(_effectiveRestriction);

			//second Date
			Expect.Call(_mostOverStaffDay.DaySchedulePart())
				.Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
				.Return(_mostOverStaffDay).Repeat.AtLeastOnce();
            Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
            Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue.Add(TimeSpan.FromHours(-1)));
            Expect.Call(_mostOverStaffSchedulePart.Clone())
                .Return(_mostOverStaffSchedulePart);
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions)).IgnoreArguments()
				.Return(_effectiveRestriction);

			//delete
			Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(null, null, true)).IgnoreArguments()
				.Return(null);
		}

		private void tryScheduleSecondDate(bool success)
		{
			Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime);
			Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays);
			Expect.Call(_mostOverStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
			Expect.Call(_personAssignment.MainShift).Return(_mainShift);
			Expect.Call(
				() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).
				IgnoreArguments();
			Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions,
			                                                 _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService))
				.IgnoreArguments()
				.Return(success);

			if (success)
			{
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
				Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(new DateOnly(2010, 1, 8))).Return(true);
			}

			if (!success)
            {
                Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
				// rollback
				_rollbackService.Rollback();
			}
		}

    	private void tryScheduleFirstDate(bool success, bool sameContractTime)
		{
			Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long);
			Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays);
			Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
			Expect.Call(_personAssignment.MainShift).Return(_mainShift);
			Expect.Call(
				() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments();
			Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, _effectiveRestriction, _resourceCalculateDelayer, null, _rollbackService)).IgnoreArguments()
				.Return(success);
			if(success)
			{
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			    if(sameContractTime)
				{
				    Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
                    Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                    Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                    Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                    Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
                    // rollback
                    _rollbackService.Rollback();
				}
				else
				{
					Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue.Add(TimeSpan.FromHours(1)));
					Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(new DateOnly(2010, 1, 9))).Return(true);
				}
			}
			if (!success)
			{
                Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
			    Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
				// rollback
				_rollbackService.Rollback();
			}
		}

		private void resourceCalculation()
		{
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true, new List<IScheduleDay>(), new List<IScheduleDay>())).IgnoreArguments();
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true, new List<IScheduleDay>(), new List<IScheduleDay>())).IgnoreArguments();
		}
    }
}
