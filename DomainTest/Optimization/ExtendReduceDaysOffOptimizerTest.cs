using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
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
        private ISchedulingOptionsCreator _schedulingOptionsCreator;
        private ISchedulingOptions _schedulingOptions;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitDecider;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
    	private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
        private IProjectionService _projectionService;
        private IVisualLayerCollection _visualLayerCollection;
	    private OverLimitResults _overLimitCount;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
            _personalSkillPeriodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _personalScheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _decisionMaker = _mocks.StrictMock<IExtendReduceDaysOffDecisionMaker>();
            _matrixConverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _scheduleServiceForFlexibleAgents = _mocks.StrictMock<IScheduleService>();
            _optimizerPreferences = new OptimizationPreferences();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
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
            _schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
            _schedulingOptions = new SchedulingOptions();
            _optimizationOverLimitDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
        	_mainShiftOptimizeActivitySpecificationSetter =
        		_mocks.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
            _projectionService = _mocks.StrictMock<IProjectionService>();
            _visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            _target = new ExtendReduceDaysOffOptimizer(_personalSkillPeriodValueCalculator,
                                                       _personalScheduleResultDataExtractor, _decisionMaker,
                                                       _matrixConverter, _scheduleServiceForFlexibleAgents,
                                                       _optimizerPreferences, _rollbackService,
                                                       _resourceCalculateDelayer, _effectiveRestrictionCreator,
                                                       _decider, _originalStateContainerForTagChange,
                                                       _workTimeBackToLegalStateService,
                                                       _nightRestWhiteSpotSolverService, _validatorList,
                                                       _dayOffsInPeriodCalculator, _dayOffTemplate,
                                                       _dayOffOptimizerConflictHandler, 
                                                       _dayOffOptimizerValidator,
                                                       _optimizationOverLimitDecider,
                                                       _schedulingOptionsCreator,
													   _mainShiftOptimizeActivitySpecificationSetter);

            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _extendReduceTimeDecisionMakerResult = new ExtendReduceTimeDecisionMakerResult();
            DateTime mainShiftStart = new DateTime(2012, 2, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod mainShiftPeriod = new DateTimePeriod(mainShiftStart, mainShiftStart.AddHours(8));
            _personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(),
                                                                                      mainShiftPeriod);
	        _overLimitCount = new OverLimitResults(0, 0, 0, 0, 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldSuccess()
        {
            _extendReduceTimeDecisionMakerResult.DayToLengthen = DateOnly.MaxValue;
            _extendReduceTimeDecisionMakerResult.DayToShorten = DateOnly.MinValue;
            _effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                             new WorkTimeLimitation(), null, null, null,
                                                             new List<IActivityRestriction>());
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, (TimeZoneInfo.Utc));
			var useCategory = new PossibleStartEndCategory { ShiftCategory = _personAssignment.ShiftCategory };
            using (_mocks.Record())
            {
                commonMocks();
                int x;
				IList<IScheduleDay> y;
	            Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimitCount);
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y))
                    .Return(true).OutRef(1, y);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix, _schedulingOptions, _rollbackService)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly>{DateOnly.MinValue});
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(), null)).
                    IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState)
                    .Return(new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
				Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, null, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                   .Return(true).Repeat.AtLeastOnce();

                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteMainShift(_scheduleDay));
                Expect.Call(() => _scheduleDay.CreateAndAddDayOff(_dayOffTemplate));
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_decider.DecideDates(_scheduleDay, _scheduleDay)).Return(new List<DateOnly>
                                                                                         {DateOnly.MinValue});
                Expect.Call(_dayOffOptimizerValidator.Validate(_extendReduceTimeDecisionMakerResult.DayToShorten.Value,
                                                               _matrix)).Return(true);

                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(1);
				Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix, _schedulingOptions, _rollbackService)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
				Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod())).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_optimizationOverLimitDecider.HasOverLimitIncreased(_overLimitCount)).Return(false);
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
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, (TimeZoneInfo.Utc));
			var useCategory = new PossibleStartEndCategory { ShiftCategory = _personAssignment.ShiftCategory };
            using (_mocks.Record())
            {
                commonMocks();
                int x;
				IList<IScheduleDay> y;
				Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimitCount);
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
				Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix, _schedulingOptions, _rollbackService)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.AtLeastOnce();
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(), null)).
                    IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(
                    new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
				Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, null, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                    .Return(true).Repeat.AtLeastOnce();

                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMaximumTargetDaysOff(_schedulePeriod)).Return(true);

                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(100);
                Expect.Call(_rollbackService.ModificationCollection).Return(
                    new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _scheduleDay }));
                Expect.Call(() => _rollbackService.Rollback());
            	Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod())).IgnoreArguments();
	            Expect.Call(_optimizationOverLimitDecider.HasOverLimitIncreased(_overLimitCount)).Return(false);
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
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, (TimeZoneInfo.Utc));
			var useCategory = new PossibleStartEndCategory { ShiftCategory = _personAssignment.ShiftCategory };
            using (_mocks.Record())
            {
                commonMocks();
                int x;
				IList<IScheduleDay> y;
				Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimitCount);
                Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit())
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_schedulePeriod, out x, out y)).Return(true).OutRef(1, y);
                Expect.Call(_decisionMaker.Execute(_matrixConverter, _personalScheduleResultDataExtractor,
                                                   _validatorList)).Return(_extendReduceTimeDecisionMakerResult);
                Expect.Call(_personalSkillPeriodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
                Expect.Call(_dayOffsInPeriodCalculator.OutsideOrAtMinimumTargetDaysOff(_schedulePeriod)).Return(false);
                Expect.Call(() => _scheduleDay.DeleteDayOff());
                Expect.Call(() => _rollbackService.Modify(_scheduleDay));
                Expect.Call(_workTimeBackToLegalStateService.Execute(_matrix, _schedulingOptions, _rollbackService)).Return(true);
                Expect.Call(_workTimeBackToLegalStateService.RemovedDays).Return(new List<DateOnly> { DateOnly.MinValue });
                Expect.Call(_originalStateContainerForTagChange.OldPeriodDaysState).Return(
                    new Dictionary<DateOnly, IScheduleDay>
                        {
                            {_extendReduceTimeDecisionMakerResult.DayToLengthen.Value, _scheduleDay},
                            {_extendReduceTimeDecisionMakerResult.DayToShorten.Value, _scheduleDay}
                        }).Repeat.AtLeastOnce();
				Expect.Call(_scheduleServiceForFlexibleAgents.SchedulePersonOnDay(_scheduleDay, _schedulingOptions, null, _resourceCalculateDelayer, useCategory, _rollbackService)).IgnoreArguments()
                    .Return(false).Repeat.AtLeastOnce();
                Expect.Call(_nightRestWhiteSpotSolverService.Resolve(_matrix, _schedulingOptions, _rollbackService)).IgnoreArguments()
                    .Return(true).Repeat.AtLeastOnce();
                Expect.Call(_originalStateContainerForTagChange.IsFullyScheduled()).Return(false);
                Expect.Call(_rollbackService.ModificationCollection).Return(new List<IScheduleDay>{ _scheduleDay });
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService).Repeat.Any();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Any();
                Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod()).Repeat.Any();
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(new DateOnly(), null)).
                    IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(() => _rollbackService.Rollback());
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
            Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences))
                .Return(_schedulingOptions).Repeat.AtLeastOnce();
            Expect.Call(_matrix.GetScheduleDayByKey(_extendReduceTimeDecisionMakerResult.DayToLengthen.Value))
                .Return(_scheduleDayPro).Repeat.Any();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay)
                .Repeat.Any();
            Expect.Call(_matrix.GetScheduleDayByKey(_extendReduceTimeDecisionMakerResult.DayToShorten.Value))
                .Return(_scheduleDayPro).Repeat.Any();
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions)).IgnoreArguments()
                .Return(_effectiveRestriction).Repeat.Any();
	        Expect.Call(_scheduleDay.GetEditorShift()).Return(null).Repeat.Any();
        	Expect.Call(
        		() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(null, null, null, DateOnly.MinValue)).
        		IgnoreArguments().Repeat.Any();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MaxIs"), Test]
        public void ShouldNotContinueIfRestrictionOverMaxIsGreaterThenZero()
        {
            using (_mocks.Record())
            {
				Expect.Call(_optimizationOverLimitDecider.OverLimitsCounts()).Return(_overLimitCount);
	            Expect.Call(_optimizationOverLimitDecider.MoveMaxDaysOverLimit()).Return(true);
            }
            Assert.IsFalse(_target.Execute());
        }
        
    }
}