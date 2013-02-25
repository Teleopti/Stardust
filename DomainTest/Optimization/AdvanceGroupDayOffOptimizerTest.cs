using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class AdvanceGroupDayOffOptimizerTest
    {
        private AdvanceGroupDayOffOptimizer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractorProvider _dataExtractorProvider;
        private IDaysOffPreferences _daysOffPreferences;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IGroupMatrixHelper _groupMatrixHelper;
        private IScheduleMatrixPro _activeScheduleMatrix;
        private IList<IScheduleMatrixPro> _allScheduleMatrixes = new List<IScheduleMatrixPro>();
        private IScheduleMatrixPro _scheduleMatrix2;
        private ISchedulingOptions _schedulingOptions;
        private IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        private IGroupPerson _groupPerson;
        private IPerson _person;
        private ValidatorResult _validatorResult;
        private IOptimizationPreferences _optimizationPreferences;
        private ITeamSteadyStateMainShiftScheduler _teamSteadyStateMainShiftScheduler;
        private ITeamSteadyStateHolder _teamSteadyStateHolder;
        private IScheduleDictionary _scheduleDictionary;
        private ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
        private ILockableBitArray _originalArray;
        private ILockableBitArray _workingBitArray;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private IList<double?> _dataExtractorValues;
        private IList<DateOnly> _daysOffToRemove;
        private IList<DateOnly> _daysOffToAdd;
        private IRestrictionAggregator _restrictionAggregator;
        private IDynamicBlockFinder _dynamicBlockFinder;
        private IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodDataGenerator;
        private IWorkShiftFilterService _workShiftFilerService;
        private IWorkShiftSelector _workShiftSelector;
        private ITeamScheduling _teamScheduling;
        private IEffectiveRestriction _effectiveRestriction;
        private IShiftProjectionCache _shiftProjectionCache;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            

            _mocks = new MockRepository();
            _smartDayOffBackToLegalStateService = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _dataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
            _daysOffPreferences = new DaysOffPreferences();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            _lockableBitArrayChangesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _groupMatrixHelper = _mocks.StrictMock<IGroupMatrixHelper>();
            _activeScheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleMatrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _schedulingOptions.UseSameDayOffs = true;
            _groupOptimizationValidatorRunner = _mocks.StrictMock<IGroupOptimizationValidatorRunner>();
            _groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
            _validatorResult = new ValidatorResult();
            _optimizationPreferences = new OptimizationPreferences();
            _teamSteadyStateMainShiftScheduler = _mocks.StrictMock<ITeamSteadyStateMainShiftScheduler>();
            _teamSteadyStateHolder = _mocks.StrictMock<ITeamSteadyStateHolder>();
            _originalArray = new LockableBitArray(7, false, false, null);
            _workingBitArray = new LockableBitArray(7, false, false, null);
            _scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _dataExtractorValues = new List<double?>();
            var dayOffToRemove = new DateOnly(2001, 02, 01);
            _daysOffToRemove = new List<DateOnly> { dayOffToRemove };
            var dayOffToAdd = new DateOnly(2001, 02, 05);
            _daysOffToAdd = new List<DateOnly> { dayOffToAdd };
            _person = PersonFactory.CreatePerson();
            _groupPerson = new GroupPerson(new List<IPerson> { _person }, DateOnly.MinValue, "Hej", null);

            _dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
            _groupPersonBuilderBasedOnContractTime = _mocks.StrictMock<IGroupPersonBuilderBasedOnContractTime>();
            _restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _skillDayPeriodDataGenerator = _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
            _workShiftFilerService = _mocks.StrictMock<IWorkShiftFilterService>();
            _shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
            _teamScheduling = _mocks.StrictMock<ITeamScheduling>();
        }

        [Test]
        public void VerifyCreation()
        {

            _target = createTarget();
            Assert.IsNotNull(_target);

        }

		[Test]
		public void ShouldContinueWithScheduleRemovedDaysOffIfTeamSteadyStateFails()
		{
            //_target = createTarget();
            //_daysOffPreferences.ConsiderWeekBefore = false;
            //_daysOffPreferences.ConsiderWeekAfter = false;
            //_target = createTarget();

            //using (_mocks.Record())
            //{
            //    commonMocks(true, true, _groupPerson);
            //    Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
            //    Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_daysOffToRemove[0], _groupPerson, _groupSchedulingService,
            //                                                                _schedulePartModifyAndRollbackService,
            //                                                                _schedulingOptions, _groupPersonBuilderForOptimization,
            //                                                                _allScheduleMatrixes, _scheduleDictionary)).Return(false);

            //    Expect.Call(_groupMatrixHelper.ScheduleRemovedDayOffDays(_daysOffToRemove, _groupPerson,
            //                                                             _groupSchedulingService,
            //                                                             _schedulePartModifyAndRollbackService,
            //                                                             _schedulingOptions,
            //                                                             _groupPersonBuilderForOptimization,
            //                                                             _allScheduleMatrixes)).IgnoreArguments().Return(
            //                                                                true);

            //    Expect.Call(_groupMatrixHelper.ScheduleBackToLegalStateDays(new List<IScheduleDay>(),
            //                                                                _groupSchedulingService,
            //                                                                _schedulePartModifyAndRollbackService,
            //                                                                _schedulingOptions, _optimizationPreferences,
            //                                                                _groupPersonBuilderForOptimization,
            //                                                                _allScheduleMatrixes)).Return(true);

            //    Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);

            //    Expect.Call(_dynamicBlockFinder.ExtractBlockDays(_daysOffToRemove[0])).Return(_daysOffToRemove);
            //    Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _daysOffToRemove[0])).Return(_groupPerson);

            //}
            //using (_mocks.Playback())
            //{
            //    bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
            //                                  _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
            //                                  _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>{_person},
            //                                  new List<IScheduleMatrixPro>() { });
            //    Assert.IsTrue(result);
            //}	
		}

		[Test]
		public void ShouldUseTeamSteadyState()
		{
            //_daysOffPreferences.ConsiderWeekBefore = false;
            //_daysOffPreferences.ConsiderWeekAfter = false;
            //_target = createTarget();

            //using (_mocks.Record())
            //{
            //    commonMocks(true, true, _groupPerson);
            //    Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
            //    Expect.Call(_teamSteadyStateMainShiftScheduler.ScheduleTeam(_daysOffToRemove[0], _groupPerson, _groupSchedulingService,
            //                                                                _schedulePartModifyAndRollbackService,
            //                                                                _schedulingOptions, _groupPersonBuilderForOptimization,
            //                                                                _allScheduleMatrixes, _scheduleDictionary)).Return(true);
            //    Expect.Call(_groupMatrixHelper.ScheduleBackToLegalStateDays(new List<IScheduleDay>(),
            //                                                                _groupSchedulingService,
            //                                                                _schedulePartModifyAndRollbackService,
            //                                                                _schedulingOptions, _optimizationPreferences,
            //                                                                _groupPersonBuilderForOptimization,
            //                                                                _allScheduleMatrixes)).Return(true);
            //}
            //using (_mocks.Playback())
            //{
            //    bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions, _optimizationPreferences, _teamSteadyStateMainShiftScheduler, _teamSteadyStateHolder, _scheduleDictionary);
            //    Assert.IsTrue(result);
            //}		
		}

		[Test]
        public void VerifySuccessfulExecuteWithOnePerson()
		{
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;
            _target = createTarget();

            using (_mocks.Record())
            {
                commonMocks(true, true, _groupPerson);
                
                IList<IScheduleDayPro> scheduleDayPros = new List<IScheduleDayPro>();
                Expect.Call(_activeScheduleMatrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayPros)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix2.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix2.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayPros)).Repeat.AtLeastOnce();
                Expect.Call(_dynamicBlockFinder.ExtractBlockDays(_daysOffToRemove[0])).Return(_daysOffToRemove);
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _daysOffToRemove[0])).Return(_groupPerson);

                Expect.Call(_groupPersonBuilderBasedOnContractTime.SplitTeams(_groupPerson, _daysOffToRemove[0])).Return
                    (new List<IGroupPerson> {_groupPerson});
                Expect.Call(_restrictionAggregator.Aggregate(_daysOffToRemove, _groupPerson, new List<IScheduleMatrixPro>{_scheduleMatrix2 },_schedulingOptions )).IgnoreArguments().Return(
                    _effectiveRestriction);
                IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activityInternalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
                Expect.Call(_skillDayPeriodDataGenerator.Generate(_groupPerson, _daysOffToRemove)).Return(
                    activityInternalData);
                IList<IShiftProjectionCache> shiftProjectionCaheList = new List<IShiftProjectionCache>{_shiftProjectionCache};
                Expect.Call(_workShiftFilerService.Filter(_daysOffToRemove[0], _person,
                                                          new List<IScheduleMatrixPro> {_scheduleMatrix2},
                                                          _effectiveRestriction, _schedulingOptions)).IgnoreArguments().Return(shiftProjectionCaheList);
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(true);
                Expect.Call(() => _teamScheduling.Execute(_daysOffToRemove[0], _daysOffToRemove,
                                                    new List<IScheduleMatrixPro> {_scheduleMatrix2}, _groupPerson, _shiftProjectionCache,
                                                    new List<DateOnly>() {}, new List<IPerson>() {})).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson> {_person },
                                              new List<IScheduleMatrixPro>() { });
                Assert.IsTrue(result);
                Assert.AreEqual(_workingBitArray, _target.WorkingBitArray);
            }	
        }

        [Test]
        public void VerifyUnsuccessfulExecuteDecisionMakerNotFindDay()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;
            var solvers = new List<IDayOffBackToLegalStateSolver>();

            using (_mocks.Record())
            {
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(_scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(_originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(_workingBitArray);
                Expect.Call(_scheduleResultDataExtractor.Values())
                    .Return(_dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(_workingBitArray, _dataExtractorValues))
                    .Return(false);

                Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(_workingBitArray)).Return(solvers);
                Expect.Call(_smartDayOffBackToLegalStateService.Execute(solvers, 100)).Return(true);

                Expect.Call(_scheduleResultDataExtractor.Values())
                    .Return(_dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(_workingBitArray, _dataExtractorValues))
                    .Return(false);

            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>() {},
                                              new List<IScheduleMatrixPro>() {});
                Assert.IsFalse(result);
            }

        }

        [Test]
        public void VerifyUnsuccessfulExecuteGroupPersonNull()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            using (_mocks.Record())
            {
                commonMocks(true, true, null);
            }
            using (_mocks.Playback())
            {
                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>(),
                                              new List<IScheduleMatrixPro>());
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyUnsuccessfulExecuteValidationFail()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            using (_mocks.Record())
            {
                commonMocks(false, false, _groupPerson);
            }
            using (_mocks.Playback())
            {
                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>(),
                                              new List<IScheduleMatrixPro>());
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyUnsuccessfulExecuteDayOffsFail()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            using (_mocks.Record())
            {
                commonMocks(false, true, _groupPerson);
            }
            using (_mocks.Playback())
            {
                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>(),
                                              new List<IScheduleMatrixPro>());
                Assert.IsFalse(result);
            }

        }

        [Test]
        public void VerifyUnsuccessfulExecute()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;
            _daysOffToRemove = new List<DateOnly>();
            using (_mocks.Record())
            {
                var solvers = new List<IDayOffBackToLegalStateSolver>();
                Expect.Call(_converter.Convert(false, false))
                    .Return(_originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(_workingBitArray);
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(_scheduleResultDataExtractor);
                Expect.Call(_scheduleResultDataExtractor.Values())
                    .Return(_dataExtractorValues);

                Expect.Call(_decisionMaker.Execute(_workingBitArray, _dataExtractorValues))
                    .Return(true);
                Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(_workingBitArray)).Return(solvers);
                Expect.Call(_smartDayOffBackToLegalStateService.Execute(solvers, 100)).Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_workingBitArray, _originalArray,
                                                                           _activeScheduleMatrix, false))
                    .Return(_daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_workingBitArray, _originalArray,
                                                                         _activeScheduleMatrix, false))
                    .Return(_daysOffToAdd);
            }
            using (_mocks.Playback())
            {
                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>(), new List<IScheduleMatrixPro>());
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyUnsuccessfulExecuteReschedulingFailWhenNotInSteadyState()
        {
            _target = createTarget();
            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            using (_mocks.Record())
            {
                commonMocks(true, true, _groupPerson);
                
                IList<IScheduleDayPro> schduleDayProList = new List<IScheduleDayPro>();
                Expect.Call(_activeScheduleMatrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(schduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix2.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrix2.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(schduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(_teamSteadyStateHolder.IsSteadyState(_groupPerson)).Return(false);

                Expect.Call(_dynamicBlockFinder.ExtractBlockDays(_daysOffToRemove[0])).Return(_daysOffToRemove);
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, _daysOffToRemove[0])).Return(_groupPerson);
            }
            using (_mocks.Playback())
            {
                _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions,
                                              _optimizationPreferences, _teamSteadyStateMainShiftScheduler,
                                              _teamSteadyStateHolder, _scheduleDictionary, new List<IPerson>{_person},
                                              new List<IScheduleMatrixPro>());
                Assert.IsFalse(_target.TeamSchedulingSuccessfullForTesting);
            }

        }

        private void commonMocks(bool executeDaysOffMoveSuccess, bool validatorSuccess, IGroupPerson groupPersonToReturn)
        {
            var solvers = new List<IDayOffBackToLegalStateSolver>();
            var groupMatrixContainer = new GroupMatrixContainer();
            var groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };
            _validatorResult.Success = validatorSuccess;

            Expect.Call(_converter.Convert(false, false))
                    .Return(_originalArray);
            Expect.Call(_converter.Convert(false, false))
                .Return(_workingBitArray);
            Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(_scheduleResultDataExtractor);
            Expect.Call(_scheduleResultDataExtractor.Values())
                    .Return(_dataExtractorValues);
            //Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(_workingBitArray)).Return(solvers);
            //Expect.Call(_smartDayOffBackToLegalStateService.Execute(solvers, 100)).Return(true);
            Expect.Call(_decisionMaker.Execute(_workingBitArray, _dataExtractorValues))
                    .Return(true);
            Expect.Call(_smartDayOffBackToLegalStateService.BuildSolverList(_workingBitArray)).Return(solvers);
            Expect.Call(_smartDayOffBackToLegalStateService.Execute(solvers, 100)).Return(true);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_workingBitArray, _originalArray, _activeScheduleMatrix, false))
                    .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(_workingBitArray, _originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_activeScheduleMatrix.Person).Return(_person).Repeat.AtLeastOnce();
            
            Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly(2001, 02, 01))).Return(
                    groupPersonToReturn);

            if (groupPersonToReturn == null)
                return;

            Expect.Call(_groupOptimizationValidatorRunner.Run(_person, _daysOffToAdd, _daysOffToRemove, false)).IgnoreArguments().Return
                    (_validatorResult);

            if (!validatorSuccess)
                return;

            Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, _daysOffToRemove, _daysOffToAdd, groupPersonToReturn, _daysOffPreferences)).IgnoreArguments()
                .Return(groupMatrixContainers);
            Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).IgnoreArguments()
                .Return(executeDaysOffMoveSuccess);

            if (!executeDaysOffMoveSuccess)
                return;

            Expect.Call(_groupMatrixHelper.GoBackToLegalState(new List<DateOnly> { DateOnly.MinValue }, groupPersonToReturn,
                                                              _schedulingOptions, _allScheduleMatrixes,
                                                              _schedulePartModifyAndRollbackService)).IgnoreArguments().Return(
                                                                new List<IScheduleDay>());
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(_workingBitArray, _originalArray, _activeScheduleMatrix, false))
                    .Return(_daysOffToRemove);
        }


        private AdvanceGroupDayOffOptimizer createTarget()
        {
            _allScheduleMatrixes = new List<IScheduleMatrixPro>();
            _allScheduleMatrixes.Add(_activeScheduleMatrix);
            _allScheduleMatrixes.Add(_scheduleMatrix2);
            return new AdvanceGroupDayOffOptimizer(_converter,
                   _decisionMaker, _dataExtractorProvider, _daysOffPreferences, _dayOffDecisionMakerExecuter, _lockableBitArrayChangesTracker, _schedulePartModifyAndRollbackService, _groupMatrixHelper, _groupOptimizationValidatorRunner,
                   _groupPersonBuilderForOptimization, _smartDayOffBackToLegalStateService, _restrictionAggregator, _dynamicBlockFinder, _groupPersonBuilderBasedOnContractTime, _schedulingOptions, _skillDayPeriodDataGenerator, _workShiftFilerService,
                   _workShiftSelector, _teamScheduling);
        }

    }

    
}

