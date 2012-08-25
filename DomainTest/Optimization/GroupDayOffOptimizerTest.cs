using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffOptimizerTest
    {
        private GroupDayOffOptimizer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractorProvider _dataExtractorProvider;
        private IDaysOffPreferences _daysOffPreferences;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IGroupSchedulingService _groupSchedulingService;
        private IGroupMatrixHelper _groupMatrixHelper;
        private IScheduleMatrixPro _activeScheduleMatrix;
        private readonly IList<IScheduleMatrixPro> _allScheduleMatrixes = new List<IScheduleMatrixPro>();
        private IScheduleMatrixPro _scheduleMatrix2;
        private ISchedulingOptions _schedulingOptions;
		private IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private IGroupPerson _groupPerson;
    	private ValidatorResult _validatorResult;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _dataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
            _daysOffPreferences = new DaysOffPreferences();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            _lockableBitArrayChangesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
            _groupMatrixHelper = _mocks.StrictMock<IGroupMatrixHelper>();
            _activeScheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleMatrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
        	_schedulingOptions.UseSameDayOffs = true;
			_groupOptimizationValidatorRunner = _mocks.StrictMock<IGroupOptimizationValidatorRunner>();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
        	_groupPerson = _mocks.StrictMock<IGroupPerson>();
        	_workShiftBackToLegalStateServicePro = _mocks.StrictMock<IWorkShiftBackToLegalStateServicePro>();

            _allScheduleMatrixes.Add(_activeScheduleMatrix);
            _allScheduleMatrixes.Add(_scheduleMatrix2);
        	_validatorResult = new ValidatorResult();
        }

        [Test]
        public void VerifyCreation()
        {
            _target = createTarget();
            Assert.IsNotNull(_target);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySuccessfulExecuteWithOnePerson()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var activePerson = _mocks.StrictMock<IPerson>();

            var dayOffToRemove = new DateOnly(2001, 02, 01);
            var daysOffToRemove = new List<DateOnly> { dayOffToRemove };
            
            var dayOffToAdd = new DateOnly(2001, 02, 05);
            var daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            var groupMatrixContainer = new GroupMatrixContainer();
            var groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            IList<double?> dataExtractorValues = new List<double?>();

            _target = createTarget();
			_validatorResult.Success = true;

            using (_mocks.Record())
            {
               // _schedulePartModifyAndRollbackService.ClearModificationCollection();
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray,_activeScheduleMatrix, false))
                    .Return(daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToAdd);
                Expect.Call(_activeScheduleMatrix.Person)
                    .Return(activePerson).Repeat.Twice();
                Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, _groupPerson, _daysOffPreferences))
                    .Return(groupMatrixContainers);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(activePerson, dayOffToRemove)).Return(
					_groupPerson);
				Expect.Call(_groupOptimizationValidatorRunner.Run(activePerson, daysOffToAdd, daysOffToRemove, true)).IgnoreArguments().Return(	_validatorResult);
                Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter,_schedulePartModifyAndRollbackService)).IgnoreArguments()
                    .Return(true);
            	Expect.Call(_groupMatrixHelper.GoBackToLegalState(daysOffToRemove, groupPerson, _schedulingOptions)).IgnoreArguments().Return
            		(new List<DateOnly>());
                Expect.Call(_groupMatrixHelper.ScheduleRemovedDayOffDays(daysOffToRemove, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization)).IgnoreArguments()
                    .Return(true);

            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsTrue(result);
            }

        }

        [Test]
        public void VerifyUnsuccessfulExecuteDecisionMakerNotFindDay()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            var dataExtractorValues = new List<double?>();

            _target = createTarget();

            using (_mocks.Record())
            {
               // _schedulePartModifyAndRollbackService.ClearModificationCollection();
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(false);

            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsFalse(result);
            }

        }

        [Test]
        public void VerifyUnsuccessfulExecuteGroupPersonNull()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);
            var activePerson = _mocks.StrictMock<IPerson>();

            var dayOffToRemove = new DateOnly(2001, 02, 01);
            var daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            var dayOffToAdd = new DateOnly(2001, 02, 05);
            var daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            var dataExtractorValues = new List<double?>();

            _target = createTarget();

            using (_mocks.Record())
            {
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToAdd);
                Expect.Call(_activeScheduleMatrix.Person)
                    .Return(activePerson);
				//Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, _groupPerson, _daysOffPreferences)).IgnoreArguments()
				//    .Return(new List<GroupMatrixContainer>());
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(activePerson, dayOffToRemove)).Return(
					null);
				//Expect.Call(_groupOptimizationValidatorRunner.Run(activePerson, daysOffToAdd, daysOffToRemove, true)).IgnoreArguments().Return(_validatorResult);
	//            Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove,
	//                                                       daysOffToAdd, groupPerson, _daysOffPreferences))
	//.Return(groupMatrixContainers);
            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsFalse(result);
            }

        }


        [Test]
        public void VerifyUnsuccessfulExecuteValidationFail()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);
            var activePerson = _mocks.StrictMock<IPerson>();

            var dayOffToRemove = new DateOnly(2001, 02, 01);
            var daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            var dayOffToAdd = new DateOnly(2001, 02, 05);
            var daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            IList<double?> dataExtractorValues = new List<double?>();

            _target = createTarget();

            using (_mocks.Record())
            {
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToAdd);
                Expect.Call(_activeScheduleMatrix.Person)
                    .Return(activePerson).Repeat.AtLeastOnce();
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(activePerson, dayOffToRemove));
            	Expect.Call(_groupOptimizationValidatorRunner.Run(activePerson, daysOffToAdd, daysOffToRemove, false)).IgnoreArguments().Return
            		(_validatorResult);
            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsFalse(result);
            }

        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyUnsuccessfulExecuteDayOffsFail()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);
            var activePerson = _mocks.StrictMock<IPerson>();

            var dayOffToRemove = new DateOnly(2001, 02, 01);
            var daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            var dayOffToAdd = new DateOnly(2001, 02, 05);
            var daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            var groupMatrixContainer = new GroupMatrixContainer();
            var groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            IList<double?> dataExtractorValues = new List<double?>();

            _target = createTarget();
        	_validatorResult.Success = true;

            using (_mocks.Record())
            {
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToAdd);
                Expect.Call(_activeScheduleMatrix.Person)
                    .Return(activePerson).Repeat.Twice();
				Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, _groupPerson, _daysOffPreferences))
					.Return(groupMatrixContainers);
				Expect.Call(_groupOptimizationValidatorRunner.Run(activePerson, daysOffToAdd, daysOffToRemove, false)).IgnoreArguments().Return
					(_validatorResult);
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(activePerson, dayOffToRemove)).Return(
            		_groupPerson);
                Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).IgnoreArguments()
                    .Return(false);

            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsFalse(result);
            }

        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyUnsuccessfulExecuteReschedulingFail()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            var originalArray = new LockableBitArray(7, false, false, null);
            var workingBitArray = new LockableBitArray(7, false, false, null);
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var activePerson = _mocks.StrictMock<IPerson>();

            var dayOffToRemove = new DateOnly(2001, 02, 01);
            var daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            var dayOffToAdd = new DateOnly(2001, 02, 05);
            var daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            var groupMatrixContainer = new GroupMatrixContainer();
            var groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            _daysOffPreferences.ConsiderWeekBefore = false;
            _daysOffPreferences.ConsiderWeekAfter = false;

            IList<double?> dataExtractorValues = new List<double?>();

            _target = createTarget();
			_validatorResult.Success = true;

            using (_mocks.Record())
            {
                Expect.Call(_dataExtractorProvider.CreatePersonalSkillDataExtractor(_activeScheduleMatrix))
                    .Return(scheduleResultDataExtractor);
                Expect.Call(_converter.Convert(false, false))
                    .Return(originalArray);
                Expect.Call(_converter.Convert(false, false))
                    .Return(workingBitArray);
                Expect.Call(scheduleResultDataExtractor.Values())
                    .Return(dataExtractorValues);
                Expect.Call(_decisionMaker.Execute(workingBitArray, dataExtractorValues))
                    .Return(true);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToRemove);
                Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                    .Return(daysOffToAdd);
                Expect.Call(_activeScheduleMatrix.Person)
                    .Return(activePerson).Repeat.Twice();
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(activePerson, dayOffToRemove)).Return(
            		groupPerson);
				Expect.Call(_groupOptimizationValidatorRunner.Run(activePerson, daysOffToAdd, daysOffToRemove, false)).IgnoreArguments().Return
					(_validatorResult);
				Expect.Call(_groupMatrixHelper.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, _groupPerson, _daysOffPreferences)).IgnoreArguments()
					.Return(groupMatrixContainers);
                Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).IgnoreArguments()
                    .Return(true);
				Expect.Call(_groupMatrixHelper.GoBackToLegalState(new List<DateOnly> { dayOffToRemove }, groupPerson, _schedulingOptions)).Return(
            		new List<DateOnly>());
                Expect.Call(_groupMatrixHelper.ScheduleRemovedDayOffDays(daysOffToRemove, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, _schedulingOptions, _groupPersonBuilderForOptimization)).IgnoreArguments()
                    .Return(false);

            }
            using (_mocks.Playback())
            {

                bool result = _target.Execute(_activeScheduleMatrix, _allScheduleMatrixes, _schedulingOptions);
                Assert.IsFalse(result);
            }

        }

        private GroupDayOffOptimizer createTarget()
        {
            return new GroupDayOffOptimizer(_converter,
                                      _decisionMaker,
                                      _dataExtractorProvider,
                                      _daysOffPreferences,
                                      _dayOffDecisionMakerExecuter, 
                                      _lockableBitArrayChangesTracker, 
                                      _schedulePartModifyAndRollbackService, 
                                      _groupSchedulingService, 
                                      _groupMatrixHelper,
									  _groupOptimizationValidatorRunner,
									  _groupPersonBuilderForOptimization);
        }
    }
}
