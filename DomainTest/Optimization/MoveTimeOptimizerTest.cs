﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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
        private IDictionary<DateOnly, IScheduleDayPro> _fullWeeksPeriodDictionary;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IDeleteSchedulePartService _deleteService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private ISchedulingOptions _schedulingOptions;
        private IEffectiveRestriction _effectiveRestriction;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
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
			_schedulingOptions = new SchedulingOptions();
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
            _fullWeeksPeriodDictionary = new Dictionary<DateOnly, IScheduleDayPro>
                                             {
                                                 {_mostUnderStaffDate, _mostUnderStaffDay},
                                                 {_mostOverStaffDate, _mostOverStaffDay}
                                             };
            _rollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
            _deleteService = _mockRepository.StrictMock<IDeleteSchedulePartService>();
            _resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
            _effectiveRestrictionCreator = _mockRepository.StrictMock<IEffectiveRestrictionCreator>();
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
            _resourceCalculateDaysDecider = _mockRepository.StrictMock<IResourceCalculateDaysDecider>();
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
        		_deleteService,
        		_resourceOptimizationHelper,
        		_effectiveRestrictionCreator,
        		_resourceCalculateDaysDecider,
        		_workShiftOriginalStateContainer,
        		_optimizationOverLimitDecider,
        		_schedulingOptionsCreator,
        		_mainShiftOptimizeActivitySpecificationSetter
        		);
        }

        [Test]
        public void VerifyExecuteWithBetterPeriodValue()
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
			Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
            Expect.Call(_optimizationOverLimitDecider.OverLimit())
                 .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
            Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
            Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();

            makePeriodBetter();

            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .IgnoreArguments()
                .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                .Return(_fullWeeksPeriodDictionary).Repeat.Any();
            //Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();
            Expect.Call(_mostOverStaffDay.DaySchedulePart())
                .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_mostUnderStaffDay.DaySchedulePart())
                .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                .Return(_mostUnderStaffDay);
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                .Return(_mostOverStaffDay);
            Expect.Call(_mostUnderStaffDay.Day)
                .Return(_mostUnderStaffDate);
            
            Expect.Call(_mostUnderStaffSchedulePart.Clone())
                .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_mostOverStaffSchedulePart.Clone())
                .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
			Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);
			Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
			Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
			Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
			Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
			Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
			Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
			Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
			Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
			Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
            Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
               .Return(null);
            Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,_mostUnderStaffSchedulePart))
                .Return(new List<DateOnly> { _mostUnderStaffDate });
            Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,_mostOverStaffSchedulePart))
                .Return(new List<DateOnly> { _mostOverStaffDate });
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
			Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
			Expect.Call(_personAssignment.MainShift).Return(_mainShift);
			Expect.Call(_mostOverStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
			Expect.Call(_personAssignment.MainShift).Return(_mainShift);
			//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(new Dictionary<DateOnly, IScheduleDay>{{_mostOverStaffDate, _mostOverStaffSchedulePart},{_mostUnderStaffDate, _mostUnderStaffSchedulePart}}).Repeat.AtLeastOnce();
			Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays).Repeat.AtLeastOnce();
			//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays);
			Expect.Call(
					() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                     .Return(true);
            Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate))
                .Return(true);
			Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                .Return(true);
            Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate))
                .Return(false);
			//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate])
			//    .Return(_mostOverStaffSchedulePart);
			//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostUnderStaffDate])
			//    .Return(_mostUnderStaffSchedulePart);
            Expect.Call(() => _rollbackService.Modify(_mostOverStaffSchedulePart, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();

            // lock day
            _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
            _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));

            // rollback
            _rollbackService.ClearModificationCollection();

        }

        private void makePeriodBetter()
        {
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1);
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
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
				Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix).Repeat.AtLeastOnce();

                makePeriodWorse();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                    .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                    .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.Day)
                    .Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day)
                    .Return(_mostOverStaffDate);
                Expect.Call(_mostUnderStaffSchedulePart.Clone())
                    .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.Clone())
                    .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                    .Return(null);
                Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,_mostUnderStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostUnderStaffDate }).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,_mostOverStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostOverStaffDate }).Repeat.AtLeastOnce();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
				Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(_mostOverStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays).Repeat.AtLeastOnce();
				Expect.Call(
					() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate))
                    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate))
                    .Return(false);
				//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate])
				//    .Return(_mostOverStaffSchedulePart).IgnoreArguments().Repeat.Any();
                Expect.Call(() =>_rollbackService.Modify(_mostOverStaffSchedulePart,new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();
                //Expect.Call(_schedulingOptions.ConsiderShortBreaks)
                //    .Return(true).Repeat.AtLeastOnce();
                // rollback
                _rollbackService.ClearModificationCollection();
                _rollbackService.Rollback();

                // lock day
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        private void makePeriodWorse()
        {
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);
        }

        [Test]
        public void VerifyExecuteWithSamePeriodValue()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
				Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
            	Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
            		.Return(false).Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();

                makePeriodSame();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.Day)
                    .Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day)
                    .Return(_mostOverStaffDate);
				//Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				//Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				//Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
				//Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				//Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				//Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
				//Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				//Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				//Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
                Expect.Call(_mostUnderStaffSchedulePart.Clone())
                    .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.Clone())
                    .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);
                Expect.Call(_mostUnderStaffDay.DaySchedulePart())
                    .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart())
                    .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,_mostUnderStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostUnderStaffDate });
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,_mostOverStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostOverStaffDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
				Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(_mostOverStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays).Repeat.AtLeastOnce();
				Expect.Call(
					() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate))
                    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate))
                    .Return(false);
				//Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate])
				//    .Return(_mostOverStaffSchedulePart).IgnoreArguments().Repeat.Any();
                Expect.Call(() =>_rollbackService.Modify(_mostOverStaffSchedulePart,new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();

                // NO rollback
                _rollbackService.ClearModificationCollection();
                //Expect.Call(_schedulingOptions.ConsiderShortBreaks)
                //    .Return(true);
                // lock day
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

        private void makePeriodSame()
        {
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(1);
        }

        [Test]
        public void VerifyExecuteSecondDayCannotScheduled()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                    .Return(_schedulingOptions);
				Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
                Expect.Call(_optimizationOverLimitDecider.OverLimit())
                    .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix).Repeat.AtLeastOnce();
            	Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);
                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).IgnoreArguments()
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_mostOverStaffDay.DaySchedulePart())
                    .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.DaySchedulePart())
                    .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                    .Return(_mostUnderStaffDay);
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                    .Return(_mostOverStaffDay);
                Expect.Call(_mostUnderStaffDay.Day)
                    .Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day)
                    .Return(_mostOverStaffDate);
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
				Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
                Expect.Call(_mostUnderStaffSchedulePart.Clone())
                    .Return(_mostUnderStaffSchedulePart);
                Expect.Call(_mostOverStaffSchedulePart.Clone())
                    .Return(_mostOverStaffSchedulePart);
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                    .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                    .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart, _mostUnderStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostUnderStaffDate });
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart, _mostOverStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostOverStaffDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
                        .Return(_effectiveRestriction);
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();
				Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays).Repeat.AtLeastOnce();
				//Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
				//    .Return(true);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
					.Return(true);
				Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).IgnoreArguments().Return(true);
				Expect.Call(
					() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments().Repeat.AtLeastOnce();
				//Expect.Call(() =>
				//    _scheduleMatrix.LockPeriod(new DateOnlyPeriod(new DateOnly(2010, 1, 9), new DateOnly(2010, 1, 9))));
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
					.Return(false);
				Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(_mostOverStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				//Expect.Call(_personAssignment.MainShift).Return(_mainShift);
                //Expect.Call(_schedulingOptions.ConsiderShortBreaks)
                //    .Return(true).Repeat.AtLeastOnce();
                // rollback
                _rollbackService.ClearModificationCollection();
                _rollbackService.Rollback();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate.AddDays(1), true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate.AddDays(1), true, true));

                // lock day
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }

		//[Test]
		//public void ShouldRollBackIfSameContractTime()
		//{
		//     using (_mockRepository.Record())
		//    {
		//        Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
		//            .Return(_schedulingOptions);
		//        Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
		//        Expect.Call(() => _schedulingOptions.UseCustomTargetTime = new TimeSpan());
		//        Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
		//             .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
		//        Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
		//            .Return(false).Repeat.AtLeastOnce();
		//        Expect.Call(_bitArrayConverter.SourceMatrix)
		//            .Return(_scheduleMatrix).Repeat.AtLeastOnce();
		//        Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
		//            .Return(2);
		//        Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).
		//            Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
		//        Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
		//            .Return(_fullWeeksPeriodDictionary).Repeat.Any();
		//        Expect.Call(_mostUnderStaffDay.DaySchedulePart())
		//            .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
		//        Expect.Call(_mostOverStaffDay.DaySchedulePart())
		//            .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
		//        Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
		//            .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
		//        Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
		//            .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
		//        Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
		//        Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
		//        Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
		//        Expect.Call(_mostUnderStaffSchedulePart.Clone())
		//            .Return(_mostUnderStaffSchedulePart);
		//        Expect.Call(_mostOverStaffSchedulePart.Clone())
		//            .Return(_mostOverStaffSchedulePart);
		//        Expect.Call(_mostUnderStaffDay.Day)
		//            .Return(_mostUnderStaffDate);
		//        Expect.Call(_mostOverStaffDay.Day)
		//            .Return(_mostOverStaffDate);
		//        Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
		//            .Return(null);
		//        Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart, _mostUnderStaffSchedulePart))
		//            .Return(new List<DateOnly> { _mostUnderStaffDate });
		//        Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart, _mostOverStaffSchedulePart))
		//            .Return(new List<DateOnly> { _mostOverStaffDate });
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
		//        Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
		//            .Return(_effectiveRestriction);
		//        Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
		//            .Return(_effectiveRestriction);
		//        Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
		//            .Return(true);
		//        Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);
		//        // rollback
		//        _rollbackService.ClearModificationCollection();
		//        _rollbackService.Rollback();
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate.AddDays(1), true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate.AddDays(1), true, true));

		//        // lock day
		//        _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
		//    }

		//    bool result;

		//    using (_mockRepository.Playback())
		//    {
		//        result = _target.Execute();
		//        Assert.IsTrue(result);
		//    }
		//}

		//[Test]
		//public void ShouldRollBackIfSameContractTime()
		//{
		//     using (_mockRepository.Record())
		//    {
		//        Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
		//            .Return(_schedulingOptions);
		//        Expect.Call(_workShiftOriginalStateContainer.OriginalWorkTime()).Return(new TimeSpan());
		//        Expect.Call(() => _schedulingOptions.UseCustomTargetTime = new TimeSpan());
		//        Expect.Call(_optimizationOverLimitDecider.OverLimit()).IgnoreArguments()
		//             .Return(new List<DateOnly>()).Repeat.AtLeastOnce();
		//        Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
		//            .Return(false).Repeat.AtLeastOnce();
		//        Expect.Call(_bitArrayConverter.SourceMatrix)
		//            .Return(_scheduleMatrix).Repeat.AtLeastOnce();
		//        Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
		//            .Return(2);
		//        Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).
		//            Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
		//        Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
		//            .Return(_fullWeeksPeriodDictionary).Repeat.Any();
		//        Expect.Call(_mostUnderStaffDay.DaySchedulePart())
		//            .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
		//        Expect.Call(_mostOverStaffDay.DaySchedulePart())
		//            .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
		//        Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
		//            .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
		//        Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
		//            .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
		//        Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
		//        Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
		//        Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
		//        Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
		//        Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8));
		//        Expect.Call(_mostUnderStaffSchedulePart.Clone())
		//            .Return(_mostUnderStaffSchedulePart);
		//        Expect.Call(_mostOverStaffSchedulePart.Clone())
		//            .Return(_mostOverStaffSchedulePart);
		//        Expect.Call(_mostUnderStaffDay.Day)
		//            .Return(_mostUnderStaffDate);
		//        Expect.Call(_mostOverStaffDay.Day)
		//            .Return(_mostOverStaffDate);
		//        Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
		//            .Return(null);
		//        Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart, _mostUnderStaffSchedulePart))
		//            .Return(new List<DateOnly> { _mostUnderStaffDate });
		//        Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart, _mostOverStaffSchedulePart))
		//            .Return(new List<DateOnly> { _mostOverStaffDate });
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
		//        Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
		//            .Return(_effectiveRestriction);
		//        Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
		//            .Return(_effectiveRestriction);
		//        Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
		//            .Return(true);
		//        Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);
		//        // rollback
		//        _rollbackService.ClearModificationCollection();
		//        _rollbackService.Rollback();
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate.AddDays(1), true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate.AddDays(1), true, true));

		//        // lock day
		//        _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
		//    }

		//    bool result;

		//    using (_mockRepository.Playback())
		//    {
		//        result = _target.Execute();
		//        Assert.IsTrue(result);
		//    }
		//}

        [Test]
        public void VerifyExecuteFirstDayCannotScheduled()
        {
            using (_mockRepository.Record())
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
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary)
                    .Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();
                Expect.Call(_mostUnderStaffDay.DaySchedulePart())
                    .Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart())
                    .Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate))
                    .Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate))
                    .Return(_mostOverStaffDay).Repeat.AtLeastOnce();
            	Expect.Call(_mostUnderStaffSchedulePart.ProjectionService()).Return(_projectionService);
            	Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
            	Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MinValue);
				Expect.Call(_mostOverStaffSchedulePart.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.MaxValue);
                Expect.Call(_mostUnderStaffSchedulePart.Clone())
                    .Return(_mostUnderStaffSchedulePart);
                Expect.Call(_mostOverStaffSchedulePart.Clone())
                    .Return(_mostOverStaffSchedulePart);
                Expect.Call(_mostUnderStaffDay.Day)
                    .Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day)
                    .Return(_mostOverStaffDate);
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments()
                    .Return(null);
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart, _mostUnderStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostUnderStaffDate });
                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart, _mostOverStaffSchedulePart))
                    .Return(new List<DateOnly> { _mostOverStaffDate });
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions)).IgnoreArguments()
                    .Return(_effectiveRestriction);
                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions)).IgnoreArguments()
                    .Return(_effectiveRestriction);
				Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState).Return(_originalDays).Repeat.AtLeastOnce();
				Expect.Call(
					() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer, null)).IgnoreArguments()
                    .Return(false);
				Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, _schedulingOptions, false, _effectiveRestriction, _resourceCalculateDelayer)).IgnoreArguments()
					.Return(false);
				Expect.Call(_mostUnderStaffSchedulePart.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
                // rollback
                _rollbackService.ClearModificationCollection();
                _rollbackService.Rollback();
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate.AddDays(1), true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate.AddDays(1), true, true));

                // lock day
                _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
            }

            using (_mockRepository.Playback())
            {
                bool result = _target.Execute();
                Assert.IsTrue(result);
            }
        }
    }
}
