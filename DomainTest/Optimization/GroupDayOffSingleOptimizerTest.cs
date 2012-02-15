using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffSingleOptimizerTest
    {
        private GroupDayOffSingleOptimizer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractorProvider _dataExtractorProvider;
        private DayOffPlannerSessionRuleSet _ruleSet;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private ILockableBitArrayChangesTracker _lockableBitArrayChangesTracker;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IGroupSchedulingService _groupSchedulingService;
        readonly IList<IDayOffLegalStateValidator> _dayOffLegalStateValidators = new List<IDayOffLegalStateValidator>();
        private readonly IList<IPerson> _selectedPersons = new List<IPerson>();
        private IGroupPersonsBuilder _groupPersonBuilder;
        private IGroupMatrixHelper _groupMatrixHelper;
        private IScheduleMatrixPro _activeScheduleMatrix;
        private readonly IList<IScheduleMatrixPro> _allScheduleMatrixes = new List<IScheduleMatrixPro>();
        private IScheduleMatrixPro _scheduleMatrix2;
        private IPerson _person;
        private IGroupPerson _groupPerson;
        private DateOnly _dayOffToRemove;
        private IList<DateOnly> _daysOffToRemove;
        private IList<DateOnly> _daysOffToAdd;
        private IGroupMatrixContainerCreator _groupMatrixContainerCreator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _dataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
            _ruleSet = new DayOffPlannerSessionRuleSet();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            _lockableBitArrayChangesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
            _groupPersonBuilder = _mocks.StrictMock<IGroupPersonsBuilder>();
            _groupMatrixHelper = _mocks.StrictMock<IGroupMatrixHelper>();
            _groupMatrixContainerCreator = _mocks.StrictMock<IGroupMatrixContainerCreator>();
            _activeScheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleMatrix2 = _mocks.StrictMock<IScheduleMatrixPro>();

            _allScheduleMatrixes.Add(_activeScheduleMatrix);
            _allScheduleMatrixes.Add(_scheduleMatrix2);
           
            
            _target = new GroupDayOffSingleOptimizer(_converter,
                                      _decisionMaker,
                                      _dataExtractorProvider,
                                      _ruleSet,
                                      _dayOffDecisionMakerExecuter,
                                      _lockableBitArrayChangesTracker,
                                      _schedulePartModifyAndRollbackService,
                                      _groupSchedulingService,
                                      _dayOffLegalStateValidators,
                                      _selectedPersons,
                                      _groupPersonBuilder,
                                      _groupMatrixHelper);

            _dayOffToRemove = new DateOnly(2011, 10, 01);
            _daysOffToRemove = new List<DateOnly> { _dayOffToRemove };

             var dayOffToAdd = new DateOnly(2011, 10, 05);
            _daysOffToAdd = new List<DateOnly> { dayOffToAdd };

             _person = new Person();
             _person.AddPersonPeriod(new PersonPeriod(_dayOffToRemove,
                     new PersonContract(new Contract("contract"),
                         new PartTimePercentage("percentage"),
                         new ContractSchedule("contractschedule")),
                         new Team()));
             _groupPerson = new GroupPerson(new List<IPerson>{_person}, _dayOffToRemove,"namn");
            
        }

        [Test]
        public void ShouldReturnFalseIfMatrixOrAllMatrixesIsNull()
        {
            Assert.That(_target.Execute(null,_allScheduleMatrixes), Is.False);
            Assert.That(_target.Execute(_activeScheduleMatrix,null), Is.False);
        }

        [Test]
        public void ShouldReturnFalseIfDecisionMakerFindsNoDays()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();

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

            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNoGroupPerson()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();
            var groupMatrixContainer = new GroupMatrixContainer();

            //_schedulePartModifyAndRollbackService.ClearModificationCollection();
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
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(_daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           groupMatrixContainer);
            Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(_dayOffToRemove, _selectedPersons, true)).Return(null);
            Expect.Call(_activeScheduleMatrix.Person).Return(_person);
            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfCannotCreateContainers()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();

            //_schedulePartModifyAndRollbackService.ClearModificationCollection();
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
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(_daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           null);
            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfHelperDoesNotValidateMoves()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();
            var groupMatrixContainer = new GroupMatrixContainer();
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            //_schedulePartModifyAndRollbackService.ClearModificationCollection();
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
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(_dayOffToRemove, _selectedPersons, true)).Return(new List<IGroupPerson> { _groupPerson });
            Expect.Call(_activeScheduleMatrix.Person).Return(_person);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(_daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           groupMatrixContainer);
            Expect.Call(_groupMatrixHelper.ValidateDayOffMoves(groupMatrixContainers, _dayOffLegalStateValidators)).
                Return(false);

            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfHelperDoesNotExecuteMoves()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();
            var groupMatrixContainer = new GroupMatrixContainer();
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            //_schedulePartModifyAndRollbackService.ClearModificationCollection();
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
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(_dayOffToRemove, _selectedPersons, true)).Return(new List<IGroupPerson> { _groupPerson });
            Expect.Call(_activeScheduleMatrix.Person).Return(_person);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(_daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           groupMatrixContainer);
            Expect.Call(_groupMatrixHelper.ValidateDayOffMoves(groupMatrixContainers, _dayOffLegalStateValidators)).
                Return(true);
            Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).
                Return(false);

            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnFalseAndRollbackIfCannotScheduleGroupPerson()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();
            var groupMatrixContainer = new GroupMatrixContainer();
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

            //_schedulePartModifyAndRollbackService.ClearModificationCollection();
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
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(_dayOffToRemove, _selectedPersons, true)).Return(new List<IGroupPerson> { _groupPerson });
            Expect.Call(_activeScheduleMatrix.Person).Return(_person);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer( _daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           groupMatrixContainer);
            Expect.Call(_groupMatrixHelper.ValidateDayOffMoves(groupMatrixContainers, _dayOffLegalStateValidators)).
                Return(true);
            Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).
                Return(true);
            Expect.Call(_groupSchedulingService.ScheduleOneDay(_dayOffToRemove, _groupPerson)).Return(false);
            //Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());

            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfCanScheduleGroupPerson()
        {
            var scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            ILockableBitArray originalArray = new LockableBitArray(7, false, false, null);
            ILockableBitArray workingBitArray = new LockableBitArray(7, false, false, null);
            IList<double?> dataExtractorValues = new List<double?>();
            var groupMatrixContainer = new GroupMatrixContainer();
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer };

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
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffRemoved(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToRemove);
            Expect.Call(_lockableBitArrayChangesTracker.DaysOffAdded(workingBitArray, originalArray, _activeScheduleMatrix, false))
                .Return(_daysOffToAdd);
            Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(_dayOffToRemove, _selectedPersons, true)).Return(new List<IGroupPerson> { _groupPerson });
            Expect.Call(_activeScheduleMatrix.Person).Return(_person);
            Expect.Call(_groupMatrixHelper.GroupMatrixContainerCreator).Return(_groupMatrixContainerCreator);
            Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(_daysOffToRemove, _daysOffToAdd, _activeScheduleMatrix, _ruleSet)).Return(
                                                                           groupMatrixContainer);
            Expect.Call(_groupMatrixHelper.ValidateDayOffMoves(groupMatrixContainers, _dayOffLegalStateValidators)).
                Return(true);
            Expect.Call(_groupMatrixHelper.ExecuteDayOffMoves(groupMatrixContainers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService)).
                Return(true);
            Expect.Call(_groupSchedulingService.ScheduleOneDay(_dayOffToRemove, _groupPerson)).Return(true);
            

            _mocks.ReplayAll();
            Assert.That(_target.Execute(_activeScheduleMatrix, _allScheduleMatrixes), Is.True);
            _mocks.VerifyAll();
        }
  
    }
}
