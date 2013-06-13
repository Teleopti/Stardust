using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupMatrixHelperTest
    {
        private GroupMatrixHelper _target;
        private MockRepository _mocks;
        private DaysOffPreferences _daysOffPreferences;
        private IScheduleMatrixPro _activeScheduleMatrix;
        private IList<IScheduleMatrixPro> _allScheduleMatrixes;
        private IGroupMatrixContainerCreator _groupMatrixContainerCreator;
    	private IGroupPersonConsistentChecker _groupPersonConsistentChecker;
    	private ISchedulingOptions _schedulingOptions;
    	private IWorkShiftBackToLegalStateServicePro _workShiftBackToLegalStateServicePro;
    	private IResourceOptimizationHelper _resourceOptimizationHelper;
    	private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private IPerson _person;
    	private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
    	private IScheduleDay _scheduleDay;
    	private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
    	private ISchedulePartModifyAndRollbackService _rollbackService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _daysOffPreferences = new DaysOffPreferences();

            _allScheduleMatrixes = new List<IScheduleMatrixPro>();
            _activeScheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _allScheduleMatrixes.Add(_activeScheduleMatrix);

            _groupMatrixContainerCreator = _mocks.StrictMock<IGroupMatrixContainerCreator>();
        	_groupPersonConsistentChecker = _mocks.StrictMock<IGroupPersonConsistentChecker>();
        	_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
        	_workShiftBackToLegalStateServicePro = _mocks.StrictMock<IWorkShiftBackToLegalStateServicePro>();
        	_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
        	_mainShiftOptimizeActivitySpecificationSetter =
        		_mocks.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();

        	_target = new GroupMatrixHelper(_groupMatrixContainerCreator, _groupPersonConsistentChecker,
        	                                _workShiftBackToLegalStateServicePro, _resourceOptimizationHelper,
											_mainShiftOptimizeActivitySpecificationSetter);

			_schedulingOptions = new SchedulingOptions();
        	_person = _mocks.StrictMock<IPerson>();
        	_scheduleDay = _mocks.StrictMock<IScheduleDay>();
        	_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
        	_rollbackService = _mocks.Stub<ISchedulePartModifyAndRollbackService>();
        }

		[Test]
		public void ScheduleSinglePersonShouldWork()
		{
			DateOnly date = new DateOnly();
			IPerson person = PersonFactory.CreatePerson();
			IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
			ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson);
				Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date, _schedulingOptions)).
					Return(true);
				Expect.Call(groupSchedulingService.ScheduleOneDayOnOnePerson(date, person, _schedulingOptions, groupPerson,
				                                                             _allScheduleMatrixes)).Return(true);
			}

			bool result;

			using (_mocks.Playback())
			{
				result = _target.ScheduleSinglePerson(date, person, groupSchedulingService, rollbackService, _schedulingOptions,
				                                      _groupPersonBuilderForOptimization, _allScheduleMatrixes);
			}

			Assert.IsTrue(result);
		}

		//[Test]
		//public void ScheduleSinglePersonWithoutRollbackShouldWork()
		//{
		//    DateOnly date = new DateOnly();
		//    IPerson person = PersonFactory.CreatePerson();
		//    IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
		//    IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

		//    using (_mocks.Record())
		//    {
		//        Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson);
		//        Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date, _schedulingOptions)).
		//            Return(true);
		//        Expect.Call(groupSchedulingService.ScheduleOneDayOnOnePerson(date, person, _schedulingOptions, groupPerson,
		//                                                                     _allScheduleMatrixes)).Return(true);
		//    }

		//    bool result;

		//    using (_mocks.Playback())
		//    {
		//        result = _target.ScheduleSinglePerson(date, person, groupSchedulingService, _schedulingOptions,
		//                                              _groupPersonBuilderForOptimization, _allScheduleMatrixes);
		//    }

		//    Assert.IsTrue(result);
		//}

		//[Test]
		//public void ScheduleSinglePersonWithoutRollbackShouldReturnFalseOnConsistentCheckerFailure()
		//{
		//    DateOnly date = new DateOnly();
		//    IPerson person = PersonFactory.CreatePerson();
		//    IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
		//    IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

		//    using (_mocks.Record())
		//    {
		//        Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson);
		//        Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date, _schedulingOptions)).Return(false);
				
		//    }

		//    bool result;

		//    using (_mocks.Playback())
		//    {
		//        result = _target.ScheduleSinglePerson(date, person, groupSchedulingService, _schedulingOptions,
		//                                              _groupPersonBuilderForOptimization, _allScheduleMatrixes);
		//    }

		//    Assert.IsFalse(result);
		//}

		//[Test]
		//public void ScheduleSinglePersonWithoutRollbackShouldReturnFalseOnScheduleOneDayFailure()
		//{
		//    DateOnly date = new DateOnly();
		//    IPerson person = PersonFactory.CreatePerson();
		//    IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
		//    IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

		//    using (_mocks.Record())
		//    {
		//        Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, date)).Return(groupPerson);
		//        Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date, _schedulingOptions)).
		//            Return(true);
		//        Expect.Call(groupSchedulingService.ScheduleOneDayOnOnePerson(date, person, _schedulingOptions, groupPerson,
		//                                                                     _allScheduleMatrixes)).Return(false);
		//    }

		//    bool result;

		//    using (_mocks.Playback())
		//    {
		//        result = _target.ScheduleSinglePerson(date, person, groupSchedulingService, _schedulingOptions,
		//                                              _groupPersonBuilderForOptimization, _allScheduleMatrixes);
		//    }

		//    Assert.IsFalse(result);
		//}

		[Test]
		public void GoBackToLegalStateShouldReturnListOfSchedulesRemoved()
		{
			DateOnly date = new DateOnly();
			IList<DateOnly> daysOffToReschedule = new List<DateOnly>{date};
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();
			IPerson person = PersonFactory.CreatePerson();
			IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {person}));
				Expect.Call(_activeScheduleMatrix.Person).Return(person);
				Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(date, date));
				Expect.Call(_workShiftBackToLegalStateServicePro.Execute(_activeScheduleMatrix, _schedulingOptions, _rollbackService)).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(date);
				Expect.Call(_workShiftBackToLegalStateServicePro.RemovedSchedules).Return(new List<IScheduleDay> { _scheduleDay });
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
			}

			IList<IScheduleDay> result;

			using (_mocks.Playback())
			{
				result = _target.GoBackToLegalState(daysOffToReschedule, groupPerson, _schedulingOptions, _allScheduleMatrixes, _rollbackService);
			}

			Assert.AreEqual(1, result.Count);
			Assert.AreSame(_scheduleDay, result[0]);
		}

        [Test]
        public void VerifyCreateGroupMatrixContainerForOnePerson()
        {
            IVirtualSchedulePeriod schedulePeriodActivePerson = _mocks.StrictMock<IVirtualSchedulePeriod>();

            IPerson activePerson = _mocks.StrictMock<IPerson>();

            DateOnly dayOffToRemove = new DateOnly(2001, 02, 01);
            IList<DateOnly> daysOffToRemove = new List<DateOnly>{ dayOffToRemove };
            
            DateOnly dayOffToAdd = new DateOnly(2001, 02, 05);
            IList<DateOnly> daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            using(_mocks.Record())
            {
                Expect.Call(_activeScheduleMatrix.Person).Return(activePerson);
                Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriodActivePerson);
                Expect.Call(schedulePeriodActivePerson.DateOnlyPeriod).Return(new DateOnlyPeriod(2001, 01, 01, 2001, 12, 31));
                Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, _activeScheduleMatrix, _daysOffPreferences))
                    .Return(new GroupMatrixContainer()).Repeat.Times(1);
            }
            using(_mocks.Playback())
            {
                IList<GroupMatrixContainer> result =
                    _target.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, activePerson, _daysOffPreferences);
                Assert.AreEqual(1, result.Count());
            }
            
        }

        [Test]
        public void VerifyCreateGroupMatrixContainerForTwoPerson()
        {
            // add another matrix
            _allScheduleMatrixes.Add(_activeScheduleMatrix);

            IVirtualSchedulePeriod schedulePeriodActivePerson = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod schedulePeriodAnotherPerson = _mocks.StrictMock<IVirtualSchedulePeriod>();

            IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

            IPerson activePerson = _mocks.StrictMock<IPerson>();
            IPerson anotherPerson = _mocks.StrictMock<IPerson>();
            IList<IPerson> persons = new List<IPerson> { activePerson, anotherPerson };

            DateOnly dayOffToRemove = new DateOnly(2001, 02, 01);
            IList<DateOnly> daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            DateOnly dayOffToAdd = new DateOnly(2001, 02, 05);
            IList<DateOnly> daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            using (_mocks.Record())
            {
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(persons));
                Expect.Call(_activeScheduleMatrix.Person).Return(activePerson);
                Expect.Call(_activeScheduleMatrix.Person).Return(anotherPerson);
                Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriodActivePerson);
                Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriodAnotherPerson);
                Expect.Call(schedulePeriodActivePerson.DateOnlyPeriod).Return(new DateOnlyPeriod(2001, 01, 01, 2001, 12, 31));
                Expect.Call(schedulePeriodAnotherPerson.DateOnlyPeriod).Return(new DateOnlyPeriod(2001, 01, 01, 2001, 12, 31));
                Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd, _activeScheduleMatrix, _daysOffPreferences))
                    .Return(new GroupMatrixContainer()).Repeat.Times(2);
            }
            using (_mocks.Playback())
            {
                IList<GroupMatrixContainer> result =
                    _target.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, groupPerson, _daysOffPreferences);
                Assert.AreEqual(2, result.Count());
            }

        }

        [Test]
        public void VerifyGroupMatrixContainerReturnsNullBecauseNoMatrixFound()
        {
            IVirtualSchedulePeriod schedulePeriodActivePerson = _mocks.StrictMock<IVirtualSchedulePeriod>();

            IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

            IPerson activePerson = _mocks.StrictMock<IPerson>();
            IList<IPerson> persons = new List<IPerson> { activePerson };

            DateOnly dayOffToRemove = new DateOnly(2001, 02, 01);
            IList<DateOnly> daysOffToRemove = new List<DateOnly> { dayOffToRemove };

            DateOnly dayOffToAdd = new DateOnly(2001, 02, 05);
            IList<DateOnly> daysOffToAdd = new List<DateOnly> { dayOffToAdd };

            using (_mocks.Record())
            {
                Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(persons));
                Expect.Call(_activeScheduleMatrix.Person).Return(activePerson);
                Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriodActivePerson);
                Expect.Call(schedulePeriodActivePerson.DateOnlyPeriod).Return(new DateOnlyPeriod(2000, 01, 01, 2000, 12, 31));
            }
            using (_mocks.Playback())
            {
                IList<GroupMatrixContainer> result =
                    _target.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, groupPerson, _daysOffPreferences);
                Assert.IsNull(result);
            }

        }

		[Test]
		public void VerifyGroupMatrixContainerReturnsNullBecauseMatrixContainerCreatorReturnsNull()
		{
			IVirtualSchedulePeriod schedulePeriodActivePerson = _mocks.StrictMock<IVirtualSchedulePeriod>();

			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

			IPerson activePerson = _mocks.StrictMock<IPerson>();
			IList<IPerson> persons = new List<IPerson> {activePerson};

			DateOnly dayOffToRemove = new DateOnly(2001, 02, 01);
			IList<DateOnly> daysOffToRemove = new List<DateOnly> {dayOffToRemove};

			DateOnly dayOffToAdd = new DateOnly(2001, 02, 05);
			IList<DateOnly> daysOffToAdd = new List<DateOnly> {dayOffToAdd};

			using (_mocks.Record())
			{
				Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(persons));
				Expect.Call(_activeScheduleMatrix.Person).Return(activePerson);
				Expect.Call(_activeScheduleMatrix.SchedulePeriod).Return(schedulePeriodActivePerson);
				Expect.Call(schedulePeriodActivePerson.DateOnlyPeriod).Return(new DateOnlyPeriod(2001, 01, 01, 2001, 12, 31));
				Expect.Call(_groupMatrixContainerCreator.CreateGroupMatrixContainer(daysOffToRemove, daysOffToAdd,
				                                                                    _activeScheduleMatrix, _daysOffPreferences))
					.Return(null);
			}
			using (_mocks.Playback())
			{
				IList<GroupMatrixContainer> result =
					_target.CreateGroupMatrixContainers(_allScheduleMatrixes, daysOffToRemove, daysOffToAdd, groupPerson,
					                                    _daysOffPreferences);
				Assert.IsNull(result);
			}
		}

        [Test]
        public void VerifyExecuteDayOffMovesSuccessful()
        {
            ILockableBitArray lockableBitArray = new LockableBitArray(7, false, false, null);

            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();

            GroupMatrixContainer groupMatrixContainer1 = new GroupMatrixContainer { Matrix = _activeScheduleMatrix, WorkingArray = lockableBitArray, OriginalArray = lockableBitArray };
            GroupMatrixContainer groupMatrixContainer2 = new GroupMatrixContainer { Matrix = _activeScheduleMatrix, WorkingArray = lockableBitArray, OriginalArray = lockableBitArray };
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer1, groupMatrixContainer2 };
            int numberOfMatrixContainers = groupMatrixContainers.Count;

            using(_mocks.Record())
            {
                Expect.Call(dayOffDecisionMakerExecuter.Execute(lockableBitArray, lockableBitArray,_activeScheduleMatrix, null, false, false,false))
                    .Return(true)
                    .Repeat.Times(numberOfMatrixContainers);
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.ExecuteDayOffMoves(groupMatrixContainers, dayOffDecisionMakerExecuter, rollbackService));
            }
        }

        [Test]
        public void VerifyExecuteDayOffMovesUnsuccessfulBecauseExecuterFails()
        {
            ILockableBitArray lockableBitArray = new LockableBitArray(7, false, false, null);

            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();

            GroupMatrixContainer groupMatrixContainer1 = new GroupMatrixContainer { Matrix = _activeScheduleMatrix, WorkingArray = lockableBitArray, OriginalArray = lockableBitArray };
            GroupMatrixContainer groupMatrixContainer2 = new GroupMatrixContainer { Matrix = _activeScheduleMatrix, WorkingArray = lockableBitArray, OriginalArray = lockableBitArray };
            IList<GroupMatrixContainer> groupMatrixContainers = new List<GroupMatrixContainer> { groupMatrixContainer1, groupMatrixContainer2 };

            using(_mocks.Record())
            {
                Expect.Call(dayOffDecisionMakerExecuter.Execute(lockableBitArray, lockableBitArray, _activeScheduleMatrix, null, false, false, false))
                    .Return(true);
                Expect.Call(dayOffDecisionMakerExecuter.Execute(lockableBitArray, lockableBitArray, _activeScheduleMatrix, null, false, false, false))
                    .Return(false);
            	Expect.Call(rollbackService.ModificationCollection).Return(new List<IScheduleDay>{_scheduleDay});
                rollbackService.Rollback();
            	Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
            	Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(DateOnly.MinValue);
            	Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
            }
            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.ExecuteDayOffMoves(groupMatrixContainers, dayOffDecisionMakerExecuter, rollbackService));
            }
        }

        [Test]
        public void VerifyScheduleRemovedDayOffDaysSuccessful()
        {

            DateOnly date1 = new DateOnly(2001, 01, 01);

            IList<DateOnly> dates = new List<DateOnly>{ date1} ;

            IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();
            IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
            ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();


            using (_mocks.Record())
            {
            	Expect.Call(groupSchedulingService.ScheduleOneDay(date1, _schedulingOptions, groupPerson, _allScheduleMatrixes)).IgnoreArguments().Return(true);
            	Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date1,
            	                                                                           _schedulingOptions)).Return(true);
            	Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_person}));
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, date1)).Return(groupPerson);
            }
	
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.ScheduleRemovedDayOffDays(dates, groupPerson, groupSchedulingService, rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _allScheduleMatrixes));
            }
        }

        [Test]
        public void VerifyScheduleRemovedDayOffDaysUnsuccessfulBecauseSchedulingFails()
        {
            DateOnly date1 = new DateOnly(2001, 01, 01);
            DateOnly date2 = new DateOnly(2001, 02, 01);

            IList<DateOnly> dates = new List<DateOnly> { date1, date2 };

            IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();
        	IList<IPerson> members = new List<IPerson> {_person};
            IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
            ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();

            using (_mocks.Record())
            {
            	Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, date1,
            	                                                                           _schedulingOptions)).IgnoreArguments().Return(true).Repeat.Twice();
            	Expect.Call(groupSchedulingService.ScheduleOneDay(date1, _schedulingOptions, groupPerson,
            	                                                  _allScheduleMatrixes)).IgnoreArguments().Return(true);
				Expect.Call(groupSchedulingService.ScheduleOneDay(date2, _schedulingOptions, groupPerson,
																  _allScheduleMatrixes)).IgnoreArguments().Return(false);
            	Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members)).Repeat.AtLeastOnce();
            	Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, date1)).Return(groupPerson).Repeat.Any();
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, date2)).Return(groupPerson).Repeat.Any();
				Expect.Call(rollbackService.ModificationCollection).Return(new List<IScheduleDay> { _scheduleDay });
				rollbackService.Rollback();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(DateOnly.MinValue);
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue, true, true));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(DateOnly.MinValue.AddDays(1), true, true));
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.ScheduleRemovedDayOffDays(dates, groupPerson, groupSchedulingService, rollbackService, _schedulingOptions, _groupPersonBuilderForOptimization, _allScheduleMatrixes));
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ScheduleBackToLegalStateDaysShouldReturnFalseIfSchedulingFails()
		{
			_person=new Person();
			IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
			ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, new DateTimePeriod());
			var editorShift = new EditableShiftMapper().CreateEditorShift(personAssignment);
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();
			IOptimizationPreferences optimizationPreferences = new OptimizationPreferences();

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly());
				Expect.Call(_scheduleDay.GetEditorShift()).Return(editorShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions,
																						   optimizationPreferences,
																						   editorShift,
				                                                                           new DateOnly())).IgnoreArguments();
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly())).Return(groupPerson);
				Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, new DateOnly(),
																						   _schedulingOptions)).Return(false);

			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.ScheduleBackToLegalStateDays(new List<IScheduleDay> { _scheduleDay }, groupSchedulingService, rollbackService, _schedulingOptions, optimizationPreferences, _groupPersonBuilderForOptimization, _allScheduleMatrixes));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ScheduleBackToLegalStateDaysShouldReturnTrueIfSuccessfulScheduling()
		{
			_person=new Person();
			IGroupSchedulingService groupSchedulingService = _mocks.StrictMock<IGroupSchedulingService>();
			ISchedulePartModifyAndRollbackService rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("hej"),
			                                                                                           _person,
			                                                                                           new DateTimePeriod(),
			                                                                                           new ShiftCategory("hej"),
			                                                                                           new Scenario("hej"));
			var editorShift = new EditableShiftMapper().CreateEditorShift(personAssignment);
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();
			IOptimizationPreferences optimizationPreferences = new OptimizationPreferences();

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly());
				Expect.Call(_scheduleDay.GetEditorShift()).Return(editorShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions,
																						   optimizationPreferences,
																						   editorShift,
																						   new DateOnly())).IgnoreArguments();
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_person, new DateOnly())).Return(groupPerson);
				Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(groupPerson, new DateOnly(),
																						   _schedulingOptions)).Return(true);
				Expect.Call(groupSchedulingService.ScheduleOneDayOnOnePerson(new DateOnly(), _person, _schedulingOptions, groupPerson, _allScheduleMatrixes)).Return(true);

			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.ScheduleBackToLegalStateDays(new List<IScheduleDay> { _scheduleDay }, groupSchedulingService, rollbackService, _schedulingOptions, optimizationPreferences, _groupPersonBuilderForOptimization, _allScheduleMatrixes));
			}
		}

    }

}
