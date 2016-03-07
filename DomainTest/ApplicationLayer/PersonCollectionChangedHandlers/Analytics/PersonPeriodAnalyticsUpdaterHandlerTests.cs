using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class PersonPeriodAnalyticsUpdaterHandlerTests
	{
		private PersonPeriodAnalyticsUpdater _target;
		private IAnalyticsPersonPeriodRepository _personPeriodRepository;
		private IPersonRepository _personRepository;

		readonly Guid testPerson1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
		readonly Guid testPerson2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
		readonly Guid testPerson3Id = Guid.Parse("00000000-0000-0000-0000-000000000003");

		readonly Guid testPersonPeriod1Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
		readonly Guid testPersonPeriod2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
		readonly Guid testPersonPeriod3Id = Guid.Parse("00000000-0000-0000-0000-000000000003");

		[SetUp]
		public void Setup()
		{

			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));


			_personRepository = new FakePersonRepository();

			var p1 = new Person
			{
				Name = new Name("Test1", "Testsson"),
				Email = "test1@test.se",
				EmploymentNumber = "E321"
			};
			p1.SetId(testPerson1Id);
			var p2 = new Person
			{
				Name = new Name("Test2", "Testsson"),
				Email = "test2@test.se",
				EmploymentNumber = "E321"
			};
			p2.SetId(testPerson2Id);
			var p3 = new Person
			{
				Name = new Name("Test3", "Testsson"),
				Email = "test3@test.se",
				EmploymentNumber = "E321"
			};
			p3.SetId(testPerson3Id);


			_personRepository.Add(p1);
			_personRepository.Add(p2);
			_personRepository.Add(p3);


			_target = new PersonPeriodAnalyticsUpdater(_personRepository, _personPeriodRepository);


		}

		[Test]
		public void NoPersonPeriodOnPerson_HandlePersonPeriodChanged_NoPersonPeriodAddedInAnalytics()
		{
			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});
			
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void NewPersonPeriod_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding a person period
			_personRepository.FindPeople(new List<Guid> { testPerson1Id }).First().AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then there should be one person period for that person
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void TwoNewPersonPeriods_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding two person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 2, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then there should be one person period for that person
			Assert.AreEqual(2, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void NewPersonPeriodWithLaterStart_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2020, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void NewPersonPeriodWithStartBeforeAnalyticsDates_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2001, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void NewPersonPeriodOnStartAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2015, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		[Test]
		public void NewPersonPeriodOnEndAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}


		[Test]
		public void NewPersonPeriodOnDayAfterEndAnalyticsDate_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod((new DateTime(2017, 12, 31)).AddDays(1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}


		[Test]
		public void NewPersonPeriodOnDayBeforeStartAnalyticsDate_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod((new DateTime(2015, 1, 1)).AddDays(-1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent()
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count());
		}

		private static PersonPeriod newTestPersonPeriod(DateTime startDate, Guid? id = null)
		{
			var personPeriod = new PersonPeriod(new DateOnly(startDate),
				new PersonContract(new Contract("ContractNameTest"),
					new PartTimePercentage("100%"),
					new ContractSchedule("ScheduleName")),
				new Team
				{
					Site = new Site("SiteName"),
				});
			personPeriod.SetId(id ?? Guid.NewGuid());
			return personPeriod;
		}
	}
}
