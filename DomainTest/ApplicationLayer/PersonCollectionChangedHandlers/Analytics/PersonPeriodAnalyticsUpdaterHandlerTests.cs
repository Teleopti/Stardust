﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class PersonPeriodAnalyticsUpdaterHandlerTests
	{
		private PersonPeriodAnalyticsUpdater _target;
		private IAnalyticsPersonPeriodRepository _personPeriodRepository;
		private IAnalyticsSkillRepository _analyticsSkillRepository;
		private IPersonRepository _personRepository;
		private FakeEventPublisher _eventPublisher;

		private Guid testPerson1Id;

		[SetUp]
		public void Setup()
		{

			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));

			_analyticsSkillRepository = new FakeAnalyticsSkillRepository();
			_personRepository = new FakePersonRepository();
			_eventPublisher = new FakeEventPublisher();

			var p1 = new Person
			{
				Name = new Name("Test1", "Testsson"),
				Email = "test1@test.se",
				EmploymentNumber = "E321"
			}.WithId();
			var p2 = new Person
			{
				Name = new Name("Test2", "Testsson"),
				Email = "test2@test.se",
				EmploymentNumber = "E321"
			}.WithId();
			var p3 = new Person
			{
				Name = new Name("Test3", "Testsson"),
				Email = "test3@test.se",
				EmploymentNumber = "E321"
			}.WithId();

			testPerson1Id = p1.Id.GetValueOrDefault();

			_personRepository.Add(p1);
			_personRepository.Add(p2);
			_personRepository.Add(p3);

			_target = new PersonPeriodAnalyticsUpdater(_personRepository, _personPeriodRepository, _analyticsSkillRepository, _eventPublisher);
		}

		[Test]
		public void NoPersonPeriodOnPerson_HandlePersonPeriodChanged_NoPersonPeriodAddedInAnalytics()
		{
			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});
			
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriod_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding a person period
			_personRepository.FindPeople(new List<Guid> { testPerson1Id }).First().AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then there should be one person period for that person
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);

			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void TwoNewPersonPeriods_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding two person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 2, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then there should be one person period for that person
			Assert.AreEqual(2, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(2);
		}

		[Test]
		public void NewPersonPeriodWithLaterStart_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2020, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodWithStartBeforeAnalyticsDates_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2001, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnStartAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2015, 1, 1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void NewPersonPeriodOnEndAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void NewPersonPeriodOnDayAfterEndAnalyticsDate_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod((new DateTime(2017, 12, 31)).AddDays(1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnDayBeforeStartAnalyticsDate_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod((new DateTime(2015, 1, 1)).AddDays(-1)));

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		private static PersonPeriod newTestPersonPeriod(DateTime startDate, Guid? id = null)
		{
			var personPeriod = new PersonPeriod(new DateOnly(startDate),
				new PersonContract(new Contract("ContractNameTest"),
					new PartTimePercentage("100%"),
					new ContractSchedule("ScheduleName")),
				new Team
				{
					Site = new Site("SiteName")
				});
			personPeriod.SetId(id ?? Guid.NewGuid());
			return personPeriod;
		}

		
	}
}
