using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
	[TestFixture]
	public class GroupPersonsBuilderTest
	{
		private IGroupPersonsBuilder _groupPersonsBuilder;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPerson _person4;
		private IPerson _person5;
		private MockRepository _mocks;
        private IGroupPagePerDateHolder _groupPagePerDateHolder;
		private IGroupPagePerDate _groupPagePerDate;
		private IList<IPerson> _allPerson;
		private IScheduleDictionary _scheduleDictionary;
		private IGroupPersonFactory _groupPersonFactory;
		private IGroupPersonConsistentChecker _groupPersonConsistentChecker;
		private WorkShiftFinderResultHolder _schedulingResults;
	    private ISchedulingResultStateHolder _stateHolder;
		private ISchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person1 = _mocks.StrictMock<IPerson>();
			_person2 = _mocks.StrictMock<IPerson>();
			_person3 = _mocks.StrictMock<IPerson>();
			_person4 = _mocks.StrictMock<IPerson>();
			_person5 = _mocks.StrictMock<IPerson>();
			_groupPagePerDate = _mocks.StrictMock<IGroupPagePerDate>();
            _groupPagePerDateHolder = _mocks.StrictMock<IGroupPagePerDateHolder>();
			_allPerson = new List<IPerson> {_person1, _person2, _person3, _person4, _person5};
	    	_schedulingOptions = new SchedulingOptions {UseGroupSchedulingCommonCategory = true};
		    _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_groupPersonFactory = _mocks.StrictMock<IGroupPersonFactory>();
			_groupPersonConsistentChecker = _mocks.StrictMock<IGroupPersonConsistentChecker>();
			_schedulingResults = new WorkShiftFinderResultHolder();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldGetListOfGroupPersonsOnDate()
		{
			var date = new DateOnly(2010, 10, 4);
			var groupPage = _mocks.StrictMock<IGroupPage>();
			var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
			var selectedPersons = new List<IPerson> {_person1, _person2, _person3};
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
		    Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
		    Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
			Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
			Expect.Call(groupPage.RootGroupCollection).Return(
				new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup }));
			Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(_allPerson));
		    Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
			Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup> ()));
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, selectedPersons,date, _schedulingOptions)).Return(true).IgnoreArguments();
			Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
			Expect.Call(_groupPersonFactory.CreateGroupPerson(selectedPersons, date, "ROOT")).Return(groupPerson);
			Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(selectedPersons));
			Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null);
			Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);

			_mocks.ReplayAll();
			_groupPersonsBuilder = new GroupPersonsBuilder(  _stateHolder, _groupPersonFactory, _groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);
			
		    Assert.That(ret.Count,Is.EqualTo(1));
			
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldGetListOfTwoIfPersonsInDifferentGrouping()
		{
			var date = new DateOnly(2010, 10, 4);
			var groupPage = _mocks.StrictMock<IGroupPage>();
			var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
			var rootGroup2 = _mocks.StrictMock<IRootPersonGroup>();
			var personColl1 = new List<IPerson> {_person1, _person4, _person5};
			var personColl2 = new List<IPerson> {_person2, _person3};
			var members1 = new List<IPerson> {_person1};
			var members2 = new List<IPerson> {_person2,_person3};

			var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var groupPerson2 = _mocks.StrictMock<IGroupPerson>();

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
			Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
			Expect.Call(groupPage.RootGroupCollection).Return(
				new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup, rootGroup2 }));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
			Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl1));
			Expect.Call(rootGroup2.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl2));
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl1, date,_schedulingOptions)).Return(true);
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl2, date, _schedulingOptions)).Return(true);
			Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
			Expect.Call(rootGroup2.Description).Return(new Description("ROOT2"));
			
			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person1 }, date, "ROOT")).Return(groupPerson);
			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person2, _person3 }, date, "ROOT2")).Return(groupPerson2);

			Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members1));
			Expect.Call(groupPerson2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members2));
			Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null).Repeat.Twice();
			Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);
			Expect.Call(() => groupPerson2.CommonPossibleStartEndCategory = null);

			Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup> ()));
			Expect.Call(rootGroup2.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup> ()));

			_mocks.ReplayAll();
            _groupPersonsBuilder = new GroupPersonsBuilder(_stateHolder, _groupPersonFactory,
				_groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);

			Assert.That(ret.Count, Is.EqualTo(2));
			_mocks.VerifyAll();
		}

		[Test] 
		public void ShouldNotGetGroupPersonIfShiftCategoriesAreDifferent()
		{
			var date = new DateOnly(2010, 10, 4);
			var groupPage = _mocks.StrictMock<IGroupPage>();
			var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
			var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
			Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
			Expect.Call(groupPage.RootGroupCollection).Return(
				new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup }));
			Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(_allPerson));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _allPerson, date, _schedulingOptions)).Return(false);
			Expect.Call(rootGroup.Description).Return(new Description("RootGroup"));
			_mocks.ReplayAll();
            _groupPersonsBuilder = new GroupPersonsBuilder(_stateHolder, _groupPersonFactory, 
                _groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);

			Assert.That(ret.Count, Is.EqualTo(0));
			Assert.That(_schedulingResults.GetResults().Count,Is.GreaterThan(0));
			_mocks.VerifyAll();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotReturnGroupPersonWithEmptyMemberList()
		{
			var date = new DateOnly(2010, 10, 4);
			var groupPage = _mocks.StrictMock<IGroupPage>();
			var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
			var rootGroup2 = _mocks.StrictMock<IRootPersonGroup>();
			var personColl1 = new List<IPerson> { _person1, _person4, _person5 };
			var personColl2 = new List<IPerson> { _person2, _person3 };
			var members1 = new List<IPerson> { _person1 };
			
			var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var groupPerson2 = _mocks.StrictMock<IGroupPerson>();

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
			Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
			Expect.Call(groupPage.RootGroupCollection).Return(
				new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup, rootGroup2 }));
			Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl1));
			Expect.Call(rootGroup2.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl2));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl1, date, _schedulingOptions)).Return(true);
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl2, date, _schedulingOptions)).Return(true);
			Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
			Expect.Call(rootGroup2.Description).Return(new Description("ROOT2"));

			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person1 }, date, "ROOT")).Return(groupPerson);
			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person2, _person3 }, date, "ROOT2")).Return(groupPerson2);

			Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members1));
			Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null);
			Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);
			// empty for some internal reason in GroupPerson
			Expect.Call(groupPerson2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));

			Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));
			Expect.Call(rootGroup2.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));

			_mocks.ReplayAll();
            _groupPersonsBuilder = new GroupPersonsBuilder(_stateHolder, _groupPersonFactory, 
                _groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);

			Assert.That(ret.Count, Is.EqualTo(1));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnOwnGroupPersonWhenMemberOnSubgroup()
		{
			var date = new DateOnly(2010, 10, 4);
			var groupPage = _mocks.StrictMock<IGroupPage>();
			var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
			var childPersonGroup = _mocks.StrictMock<IChildPersonGroup>();
			var personColl1 = new List<IPerson> { _person1, _person4, _person5 };
			var personColl2 = new List<IPerson> { _person2, _person3 };
			var members1 = new List<IPerson> { _person1 };
			var members2 = new List<IPerson> { _person2, _person3 };

			var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var groupPerson2 = _mocks.StrictMock<IGroupPerson>();

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
			Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
			Expect.Call(groupPage.RootGroupCollection).Return(
				new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup }));
			Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup> { childPersonGroup }));
			Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl1));
			Expect.Call(childPersonGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(personColl2));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.Twice();
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson).Repeat.Twice();
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl1, date, _schedulingOptions)).Return(true);
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, personColl2, date, _schedulingOptions)).Return(true);
			Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
			Expect.Call(childPersonGroup.Description).Return(new Description("CHILD"));

			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person1 }, date, "ROOT")).Return(groupPerson);
			
			Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person2, _person3 }, date, "CHILD")).Return(groupPerson2);

			Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members1));
			Expect.Call(groupPerson2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members2));
			Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null).Repeat.Twice();
			Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);
			Expect.Call(() => groupPerson2.CommonPossibleStartEndCategory = null);

			Expect.Call(childPersonGroup.ChildGroupCollection).Return(
				new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));
			_mocks.ReplayAll();
			_groupPersonsBuilder = new GroupPersonsBuilder(  _stateHolder, _groupPersonFactory, _groupPersonConsistentChecker, _schedulingResults,_groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);

			Assert.That(ret.Count, Is.EqualTo(2));
			_mocks.VerifyAll();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotTryToCheckPersonNotLoadedInDictionary()
        {
            _allPerson.Remove(_person1);
            var date = new DateOnly(2010, 10, 4);
            var groupPage = _mocks.StrictMock<IGroupPage>();
            var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
            var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var selectedAndInDictionary = new List<IPerson> {_person2, _person3};

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
            Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
            Expect.Call(groupPage.RootGroupCollection).Return(
                new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup }));
            Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(_allPerson));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
            Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));
			Expect.Call(_groupPersonConsistentChecker.AllPersonsHasSameOrNoneScheduled(_scheduleDictionary, _allPerson, date, _schedulingOptions)).Return(true);
            Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
            Expect.Call(_groupPersonFactory.CreateGroupPerson(selectedAndInDictionary, date, "ROOT")).Return(groupPerson);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(selectedPersons));
            Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null);
            Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);

            _mocks.ReplayAll();
            _groupPersonsBuilder = new GroupPersonsBuilder(_stateHolder, _groupPersonFactory, 
                _groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, true, _schedulingOptions);

            Assert.That(ret.Count, Is.EqualTo(1));
            
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldNotTryToCheckShiftCategoryConsistencyWhenFalse()
        {
            _allPerson.Remove(_person1);
            var date = new DateOnly(2010, 10, 4);
            var groupPage = _mocks.StrictMock<IGroupPage>();
            var rootGroup = _mocks.StrictMock<IRootPersonGroup>();
            var selectedPersons = new List<IPerson> { _person1, _person2, _person3 };
            var groupPerson = _mocks.StrictMock<IGroupPerson>();
            var selectedAndInDictionary = new List<IPerson> { _person2, _person3 };

            Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate).Return(_groupPagePerDate);
            Expect.Call(_groupPagePerDate.GetGroupPageByDate(date)).Return(groupPage);
            Expect.Call(groupPage.RootGroupCollection).Return(
                new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { rootGroup }));
            Expect.Call(rootGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(_allPerson));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_scheduleDictionary.Keys).Return(_allPerson);
            Expect.Call(rootGroup.ChildGroupCollection).Return(new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));
            Expect.Call(rootGroup.Description).Return(new Description("ROOT"));
            Expect.Call(_groupPersonFactory.CreateGroupPerson(selectedAndInDictionary, date, "ROOT")).Return(groupPerson);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(selectedPersons));
            Expect.Call(_groupPersonConsistentChecker.CommonPossibleStartEndCategory).Return(null);
            Expect.Call(() => groupPerson.CommonPossibleStartEndCategory = null);

            _mocks.ReplayAll();
            _groupPersonsBuilder = new GroupPersonsBuilder(_stateHolder, _groupPersonFactory, 
                _groupPersonConsistentChecker, _schedulingResults, _groupPagePerDateHolder);

            var ret = _groupPersonsBuilder.BuildListOfGroupPersons(date, selectedPersons, false, _schedulingOptions);

            Assert.That(ret.Count, Is.EqualTo(1));

            _mocks.VerifyAll();
        }
	}

}
