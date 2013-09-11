using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftPerAgentGridPresenterTest
	{
		private ShiftPerAgentGridPresenter _target;
		private MockRepository _mock;
		private IShiftPerAgentGrid _view;
		private IPerson _person;
		private IDistributionInformationExtractor _extractor;
		private ISchedulerStateHolder _schedulerState;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IShiftPerAgentGrid>();
			_target = new ShiftPerAgentGridPresenter(_view);
			_person = PersonFactory.CreatePerson("person");
			_person.SetId(Guid.NewGuid());
			_extractor = _mock.StrictMock<IDistributionInformationExtractor>();
			_schedulerState = _mock.StrictMock<ISchedulerStateHolder>();
		}

		[Test]
		public void ShouldGetShiftCategoryCount()
		{
			var shiftCategory1 = new ShiftCategory("shiftCategory1");
			var shiftCategory2 = new ShiftCategory("shiftCategory2");
			var shiftCategoryPerAgent1 = new ShiftCategoryPerAgent(_person, shiftCategory1, 1);
			var shiftCategoryPerAgent2 = new ShiftCategoryPerAgent(_person, shiftCategory2, 2);
			var shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent> {shiftCategoryPerAgent1, shiftCategoryPerAgent2};
			var otherPerson = PersonFactory.CreatePerson("other");

			var count = _target.ShiftCategoryCount(_person, shiftCategory1, shiftCategoryPerAgentList);
			Assert.AreEqual(1, count);

			count = _target.ShiftCategoryCount(_person, shiftCategory2, shiftCategoryPerAgentList);
			Assert.AreEqual(2, count);

			count = _target.ShiftCategoryCount(otherPerson, shiftCategory1, shiftCategoryPerAgentList);
			Assert.AreEqual(0, count);
		}

		[Test]
		public void ShouldSort()
		{
			var otherPerson = PersonFactory.CreatePerson("other");
			otherPerson.SetId(Guid.NewGuid());
			var shiftCategory = new ShiftCategory("shiftCategory");
			var shiftCategoryPerAgent1 = new ShiftCategoryPerAgent(_person, shiftCategory, 1);
			var shiftCategoryPerAgent2 = new ShiftCategoryPerAgent(otherPerson, shiftCategory, 2);
			var shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent> { shiftCategoryPerAgent1, shiftCategoryPerAgent2 };
			var personInvolved = new List<IPerson> {_person, otherPerson};

			using (_mock.Record())
			{
				Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftCategoryPerAgents ).Return(shiftCategoryPerAgentList).Repeat.AtLeastOnce();
				Expect.Call(_extractor.PersonInvolved).Return(personInvolved).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftCategories).Return(new List<IShiftCategory> {shiftCategory}).Repeat.AtLeastOnce();
				Expect.Call(_view.SchedulerState).Return(_schedulerState).Repeat.AtLeastOnce();
				Expect.Call(_schedulerState.CommonAgentName(_person)).Return("B").Repeat.AtLeastOnce();
				Expect.Call(_schedulerState.CommonAgentName(otherPerson)).Return("A").Repeat.AtLeastOnce();

			}
			using (_mock.Playback())
			{
				_target.Sort(0);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, _person.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, otherPerson.Id);

				_target.Sort(0);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, otherPerson.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, _person.Id);

				_target.Sort(1);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, _person.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, otherPerson.Id);

				_target.Sort(1);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, otherPerson.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, _person.Id);

				_target.ReSort();
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, otherPerson.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, _person.Id);
			}
		}

		[Test]
		public void ShouldAddPersonsWithNoShiftCategoryPerAgentToSortedList()
		{
			var otherPerson = PersonFactory.CreatePerson("other");
			otherPerson.SetId(Guid.NewGuid());
			var shiftCategory = new ShiftCategory("shiftCategory");
			var shiftCategoryPerAgent1 = new ShiftCategoryPerAgent(_person, shiftCategory, 1);
			var shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent> { shiftCategoryPerAgent1 };
			var personInvolved = new List<IPerson> { _person, otherPerson };

			using (_mock.Record())
			{
				Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftCategoryPerAgents ).Return(shiftCategoryPerAgentList).Repeat.AtLeastOnce();
				Expect.Call(_extractor.PersonInvolved).Return(personInvolved).Repeat.AtLeastOnce();
				Expect.Call(_extractor.ShiftCategories).Return(new List<IShiftCategory> { shiftCategory }).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				_target.Sort(1);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, otherPerson.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, _person.Id);

				_target.Sort(1);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, _person.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, otherPerson.Id);
			}	
		}

		[Test]
		public void ShouldNotSortIfSortColumnDontExist()
		{
			var otherPerson = PersonFactory.CreatePerson("other");
			otherPerson.SetId(Guid.NewGuid());
			var personInvolved = new List<IPerson> { _person, otherPerson };

			using (_mock.Record())
			{
				Expect.Call(_view.ExtractorModel).Return(_extractor).Repeat.Twice();
                Expect.Call(_extractor.ShiftCategoryPerAgents).Return(new List<ShiftCategoryPerAgent>());
				Expect.Call(_extractor.PersonInvolved).Return(personInvolved);
				Expect.Call(_extractor.ShiftCategories).Return(new List<IShiftCategory>());

			}
			using (_mock.Playback())
			{
				_target.Sort(1);
				Assert.AreEqual(_target.SortedPersonInvolved()[0].Id, _person.Id);
				Assert.AreEqual(_target.SortedPersonInvolved()[1].Id, otherPerson.Id);
			}	
		}
	}
}
