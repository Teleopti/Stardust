using System.Collections.Generic;
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
    public class MoveTimeOptimizerTest
    {
        private MoveTimeOptimizer _target;
        private MockRepository _mockRepository;
        private IPeriodValueCalculator _periodValueCalculator;
        private IScheduleResultDataExtractor _personalSkillsDataExtractor;
        private IMoveTimeDecisionMaker _decisionMaker;
        private IScheduleMatrixLockableBitArrayConverter _bitArrayConverter;
		private IScheduleService _scheduleService;
        private IOptimizerOriginalPreferences _optimizerPreferences;
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
        private IScheduleMatrixOriginalStateContainer _scheduleMatrixOriginalStateContainer;
        private IScheduleMatrixOriginalStateContainer _workShiftOriginalStateContainer;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _periodValueCalculator = _mockRepository.StrictMock<IPeriodValueCalculator>();
            _personalSkillsDataExtractor = _mockRepository.StrictMock<IScheduleResultDataExtractor>();
            _decisionMaker = _mockRepository.StrictMock<IMoveTimeDecisionMaker>();
            _bitArrayConverter = _mockRepository.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
			_scheduleService = _mockRepository.StrictMock<IScheduleService>();
            _optimizerPreferences = _mockRepository.StrictMock<IOptimizerOriginalPreferences>();
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
            _schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();
            _effectiveRestrictionCreator = _mockRepository.StrictMock<IEffectiveRestrictionCreator>();
            _effectiveRestriction = _mockRepository.StrictMock<IEffectiveRestriction>();
            _resourceCalculateDaysDecider = _mockRepository.StrictMock<IResourceCalculateDaysDecider>();
            _scheduleMatrixOriginalStateContainer = _mockRepository.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _workShiftOriginalStateContainer = _mockRepository.StrictMock<IScheduleMatrixOriginalStateContainer>();
            
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
                _scheduleMatrixOriginalStateContainer,
                _workShiftOriginalStateContainer);
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
            IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 1 };
            Expect.Call(_optimizerPreferences.AdvancedPreferences)
                .Return(advancedPreferences)
                .Repeat.AtLeastOnce();

            Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.Any();

            makePeriodBetter();

            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .IgnoreArguments()
                .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });
                
            Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();
                
            Expect.Call(_optimizerPreferences.SchedulingOptions).Return(_schedulingOptions).Repeat.Any();
            Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short).Repeat.Once();
            Expect.Call(_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();

            Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();

            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay);
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay);

            Expect.Call(_mostUnderStaffDay.Day).Return(_mostUnderStaffDate);
            Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);
            Expect.Call(_mostUnderStaffSchedulePart.Clone()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_mostOverStaffSchedulePart.Clone()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();
            Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);

            Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,
                                                                       _mostUnderStaffSchedulePart)).Return(
                                                                           new List<DateOnly> { _mostUnderStaffDate });

            Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,
                                                                  _mostOverStaffSchedulePart)).Return(
                                                                      new List<DateOnly> { _mostOverStaffDate });

            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));

            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);

            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, _schedulingOptions))
                    .Return(_effectiveRestriction);

            Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, false, _effectiveRestriction))
                     .Return(true);
            Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);

            Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, false, _effectiveRestriction))
                .Return(true);

            Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate)).Return(false);
            Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate]).Return(_mostOverStaffSchedulePart).Repeat.Any();
            Expect.Call(() => _rollbackService.Modify(_mostOverStaffSchedulePart, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();

            // lock day
            _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostUnderStaffDate, _mostUnderStaffDate));
            _scheduleMatrix.LockPeriod(new DateOnlyPeriod(_mostOverStaffDate, _mostOverStaffDate));

            // rollback
            _rollbackService.ClearModificationCollection();

            Expect.Call(_schedulingOptions.ConsiderShortBreaks)
                .Return(true);

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
            IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 1 };
            Expect.Call(_optimizerPreferences.AdvancedPreferences)
                .Return(advancedPreferences)
                .Repeat.AtLeastOnce();

            Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                .IgnoreArguments()
                .Return(new List<DateOnly>());
        }

        [Test]
        public void VerifyExecuteWithWorsePeriodValue()
        {

            var schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();

            using (_mockRepository.Record())
            {
                IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 1 };
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();

                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();
                
                makePeriodWorse();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .IgnoreArguments()
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });

                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();

                Expect.Call(_optimizerPreferences.SchedulingOptions).Return(schedulingOptions).Repeat.Any();

                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();
                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short).Repeat.Once();

                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.Day).Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);

                Expect.Call(_mostUnderStaffSchedulePart.Clone()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.Clone()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);

                Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,
                                                                       _mostUnderStaffSchedulePart)).Return(
                                                                           new List<DateOnly> { _mostUnderStaffDate }).Repeat.AtLeastOnce();

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,
                                                                      _mostOverStaffSchedulePart)).Return(
                                                                          new List<DateOnly> { _mostOverStaffDate }).Repeat.AtLeastOnce();

                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));

                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, schedulingOptions))
                    .Return(_effectiveRestriction);

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, schedulingOptions))
                        .Return(_effectiveRestriction);

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, false, _effectiveRestriction))
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);
                
                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, false, _effectiveRestriction))
                    .Return(true);

                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate)).Return(false);
                Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate]).Return(_mostOverStaffSchedulePart).Repeat.Any();
                Expect.Call(
                    () =>
                    _rollbackService.Modify(_mostOverStaffSchedulePart,
                                            new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();
                Expect.Call(schedulingOptions.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();

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
            var schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();

            using (_mockRepository.Record())
            {

                IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 1 };
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();

                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();

                makePeriodSame();

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .IgnoreArguments()
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });

                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();

                Expect.Call(_optimizerPreferences.SchedulingOptions).Return(schedulingOptions).Repeat.Any();
                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short).Repeat.Once();
                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();

                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.Day).Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);

                Expect.Call(_mostUnderStaffSchedulePart.Clone()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffSchedulePart.Clone()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);

                Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,
                                                                       _mostUnderStaffSchedulePart)).Return(
                                                                           new List<DateOnly> { _mostUnderStaffDate });

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,
                                                                      _mostOverStaffSchedulePart)).Return(
                                                                          new List<DateOnly> { _mostOverStaffDate });

                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, schedulingOptions))
                    .Return(_effectiveRestriction);

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, schedulingOptions))
                        .Return(_effectiveRestriction);

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, false, _effectiveRestriction))
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, false, _effectiveRestriction))
                    .Return(true);

                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostOverStaffDate)).Return(false);
                Expect.Call(_workShiftOriginalStateContainer.OldPeriodDaysState[_mostOverStaffDate]).Return(_mostOverStaffSchedulePart).Repeat.Any();
                Expect.Call(
                    () =>
                    _rollbackService.Modify(_mostOverStaffSchedulePart,
                                            new ScheduleTagSetter(KeepOriginalScheduleTag.Instance))).IgnoreArguments();

                // NO rollback
                _rollbackService.ClearModificationCollection();

                Expect.Call(schedulingOptions.ConsiderShortBreaks)
                    .Return(true);

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

            var schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();
            
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);

                Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor))
                    .IgnoreArguments()
                    .Return(new List<DateOnly> { _mostUnderStaffDate, _mostOverStaffDate });

                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();
                Expect.Call(_optimizerPreferences.SchedulingOptions).Return(schedulingOptions).Repeat.Any();

                Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay);
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay);

                Expect.Call(_mostUnderStaffDay.Day).Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);
                Expect.Call(_mostUnderStaffSchedulePart.Clone()).Return(_mostUnderStaffSchedulePart);
                Expect.Call(_mostOverStaffSchedulePart.Clone()).Return(_mostOverStaffSchedulePart);
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,
                                                                           _mostUnderStaffSchedulePart)).Return(
                                                                               new List<DateOnly> { _mostUnderStaffDate });

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,
                                                                      _mostOverStaffSchedulePart)).Return(
                                                                          new List<DateOnly> { _mostOverStaffDate });

                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));


                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, schedulingOptions))
                    .Return(_effectiveRestriction);

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, schedulingOptions))
                        .Return(_effectiveRestriction);

                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Short).Repeat.Once();
                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, false, _effectiveRestriction))
                    .Return(true);
                Expect.Call(_workShiftOriginalStateContainer.WorkShiftChanged(_mostUnderStaffDate)).Return(true);

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostOverStaffSchedulePart, false, _effectiveRestriction))
                    .Return(false);

                Expect.Call(schedulingOptions.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();

                IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 1 };
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();

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

        [Test]
        public void VerifyExecuteFirstDayCannotScheduled()
        {
            var schedulingOptions = _mockRepository.StrictMock<ISchedulingOptions>();
            
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayConverter.SourceMatrix).Return(_scheduleMatrix).Repeat.AtLeastOnce();

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(2);

            	Expect.Call(_decisionMaker.Execute(_bitArrayConverter, _personalSkillsDataExtractor)).
            		Return(new List<DateOnly> {_mostUnderStaffDate, _mostOverStaffDate});

                Expect.Call(_scheduleMatrix.FullWeeksPeriodDictionary).Return(_fullWeeksPeriodDictionary).Repeat.Any();

                Expect.Call(_optimizerPreferences.SchedulingOptions).Return(schedulingOptions).Repeat.Any();
                Expect.Call(schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long).Repeat.Once();

                Expect.Call(_mostUnderStaffDay.DaySchedulePart()).Return(_mostUnderStaffSchedulePart).Repeat.AtLeastOnce();
                Expect.Call(_mostOverStaffDay.DaySchedulePart()).Return(_mostOverStaffSchedulePart).Repeat.AtLeastOnce();

                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostUnderStaffDate)).Return(_mostUnderStaffDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.GetScheduleDayByKey(_mostOverStaffDate)).Return(_mostOverStaffDay).Repeat.AtLeastOnce();

                Expect.Call(_mostUnderStaffSchedulePart.Clone()).Return(_mostUnderStaffSchedulePart);
                Expect.Call(_mostOverStaffSchedulePart.Clone()).Return(_mostOverStaffSchedulePart);

                Expect.Call(_mostUnderStaffDay.Day).Return(_mostUnderStaffDate);
                Expect.Call(_mostOverStaffDay.Day).Return(_mostOverStaffDate);
                Expect.Call(_deleteService.Delete(null, null, null, null)).IgnoreArguments().Return(null);

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostUnderStaffSchedulePart,
                                                                      _mostUnderStaffSchedulePart)).Return(
                                                                          new List<DateOnly> {_mostUnderStaffDate});

                Expect.Call(_resourceCalculateDaysDecider.DecideDates(_mostOverStaffSchedulePart,
                                                                      _mostOverStaffSchedulePart)).Return(
                                                                          new List<DateOnly> { _mostOverStaffDate });

                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostUnderStaffDate, true, true));
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_mostOverStaffDate, true, true));

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostUnderStaffSchedulePart, schedulingOptions))
                    .Return(_effectiveRestriction);

                Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_mostOverStaffSchedulePart, schedulingOptions))
                        .Return(_effectiveRestriction);

                IOptimizerAdvancedPreferences advancedPreferences = new OptimizerAdvancedPreferences{ MaximumMovableWorkShiftPercentagePerPerson = 1 };
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();

                Expect.Call(_scheduleService.SchedulePersonOnDay(_mostUnderStaffSchedulePart, false, _effectiveRestriction))
                    .Return(false);

                Expect.Call(schedulingOptions.ConsiderShortBreaks)
                    .Return(true).Repeat.AtLeastOnce();

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

        [Test]
        public void VerifyIsMovedDaysUnderLimit()
        {
 
            using(_mockRepository.Record())
            {
                Expect.Call(_bitArrayConverter.Workdays()).Return(12);
                var advancedPreferences = new OptimizerAdvancedPreferences {MaximumMovableWorkShiftPercentagePerPerson = 0.25};
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixOriginalStateContainer.CountChangedWorkShifts())
                    .Return(1)
                    .Repeat.AtLeastOnce();
            }
            using(_mockRepository.Playback())
            {
                Assert.IsFalse(_target.MovedDaysOverMaxDaysLimit());
            }
        }

        [Test]
        public void VerifyIsMovedDaysOverLimit()
        {

            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayConverter.Workdays()).Return(12);
                var advancedPreferences = new OptimizerAdvancedPreferences { MaximumMovableWorkShiftPercentagePerPerson = 0.25 };
                Expect.Call(_optimizerPreferences.AdvancedPreferences)
                    .Return(advancedPreferences)
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixOriginalStateContainer.CountChangedWorkShifts())
                    .Return(4)
                    .Repeat.AtLeastOnce();
                Expect.Call(_bitArrayConverter.SourceMatrix)
                    .Return(_scheduleMatrix)
                    .Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix.Person)
                    .Return(PersonFactory.CreatePerson("Test"));
            }
            using (_mockRepository.Playback())
            {
                Assert.IsTrue(_target.MovedDaysOverMaxDaysLimit());
            }
        }

    }
}
