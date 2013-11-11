using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupPersonBuilderForOptimizationTest
	{
		private MockRepository _mock;
		private IGroupPersonBuilderForOptimization _target;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IGroupPersonFactory _groupPersonFactory;
		private IGroupPagePerDateHolder _groupPagePerDateHolder;
		private DateOnly _dateToTest;
		private IGroupPage _groupPage;
		private IPerson _person;
		private IRootPersonGroup _rootPersonGroup;
		private IChildPersonGroup _childPersonGroup;
		private IScheduleDictionary _scheduleDictionary;
		private Guid _guid;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_groupPersonFactory = _mock.StrictMock<IGroupPersonFactory>();
			_groupPagePerDateHolder = _mock.StrictMock<IGroupPagePerDateHolder>();
			_target = new GroupPersonBuilderForOptimization(_schedulingResultStateHolder, _groupPersonFactory, _groupPagePerDateHolder);
			_dateToTest = new DateOnly();
			_groupPage = _mock.StrictMock<IGroupPage>();
			_person = PersonFactory.CreatePerson();
			_person.SetId(Guid.Empty);
			_rootPersonGroup = _mock.StrictMock<IRootPersonGroup>();
			_childPersonGroup = _mock.StrictMock<IChildPersonGroup>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_guid = Guid.NewGuid();
		}

		[Test]
		public void ShouldReturnSingleAgentTeamIfPersonNotBelongsToAGroup()
		{
			var groupPerson = _mock.StrictMock<IGroupPerson>();
			using (_mock.Record())
			{
				Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(_dateToTest)).Return(_groupPage);
				Expect.Call(_groupPage.RootGroupCollection).Return(
					new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup>{_rootPersonGroup}));
				Expect.Call(_rootPersonGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));
				Expect.Call(_rootPersonGroup.ChildGroupCollection).Return(
					new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>{_childPersonGroup}));
				Expect.Call(_childPersonGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));
				Expect.Call(_childPersonGroup.ChildGroupCollection).Return(
					new ReadOnlyCollection<IChildPersonGroup>(new List<IChildPersonGroup>()));
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary.Keys).Return(new Collection<IPerson> { _person });
				Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> { _person }, _dateToTest, "arne-arne", Guid.Empty)).Return(groupPerson);
			}

			using (_mock.Playback())
			{
				var result = _target.BuildGroupPerson(_person, _dateToTest);

				Assert.That(result, Is.EqualTo(groupPerson));
			}
		}

		[Test]
		public void ShouldReturnGroupPersonIfFoundInAGroup()
		{
			using (_mock.Record())
			{
				Expect.Call(_groupPagePerDateHolder.GroupPersonGroupPagePerDate.GetGroupPageByDate(_dateToTest)).Return(_groupPage);
				Expect.Call(_groupPage.RootGroupCollection).Return(
					new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { _rootPersonGroup }));
				Expect.Call(_rootPersonGroup.PersonCollection).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>{_person})).Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary.Keys).Return(new Collection<IPerson> {_person});
				Expect.Call(_rootPersonGroup.Description).Return(new Description("hoj"));
				Expect.Call(_groupPersonFactory.CreateGroupPerson(new List<IPerson> {_person}, _dateToTest, "hoj", _guid));
				Expect.Call(_rootPersonGroup.Id).Return(_guid);
			}

			IGroupPerson groupPerson;
			using (_mock.Playback())
			{
				groupPerson = _target.BuildGroupPerson(_person, _dateToTest);
			}

			Assert.IsNotNull(groupPerson);
		}
	}
}