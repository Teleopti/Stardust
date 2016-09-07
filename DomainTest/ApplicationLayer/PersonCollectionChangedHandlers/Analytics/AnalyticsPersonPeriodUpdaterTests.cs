﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsPersonPeriodUpdaterTests
	{
		private AnalyticsPersonPeriodUpdater _target;
		private IAnalyticsPersonPeriodRepository _personPeriodRepository;
		private FakeAnalyticsSkillRepository _analyticsSkillRepository;
		private IPersonRepository _personRepository;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private IAnalyticsTeamRepository _analyticsTeamRepository;
		private FakeEventPublisher _eventPublisher;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private IGlobalSettingDataRepository _globalSettingDataRepository;
		private IAnalyticsIntervalRepository _analyticsIntervalRepository;

		private Guid testPerson1Id;

		private readonly AnalyticsSkill fakeSkill3 = new AnalyticsSkill
		{
			SkillId = 3,
			SkillCode = Guid.NewGuid()
		};

		[SetUp]
		public void Setup()
		{

			_personPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			_analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();

			_analyticsSkillRepository = new FakeAnalyticsSkillRepository();
			var skills = new List<AnalyticsSkill> {fakeSkill3};
			_analyticsSkillRepository.SetSkills(skills);
			_personRepository = new FakePersonRepository();
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_analyticsTeamRepository = new FakeAnalyticsTeamRepository();
			_eventPublisher = new FakeEventPublisher();
			_analyticsDateRepository = new FakeAnalyticsDateRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));
			_analyticsTimeZoneRepository = new FakeAnalyticsTimeZoneRepository();
			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();
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

			var auow = MockRepository.GenerateMock<ICurrentAnalyticsUnitOfWork>();
			auow.Stub(a => a.Current()).Return(new FakeUnitOfWork());

			var personPeriodFilter = new PersonPeriodFilter(_analyticsDateRepository);
			var personPeriodTransformer = new PersonPeriodTransformer(_personPeriodRepository, 
				_analyticsSkillRepository, 
				_analyticsBusinessUnitRepository, 
				_analyticsTeamRepository, 
				new ReturnNotDefined(),
				_analyticsDateRepository,
				_analyticsTimeZoneRepository,
				_analyticsIntervalRepository,
				_globalSettingDataRepository);

			_target = new AnalyticsPersonPeriodUpdater(_personRepository, _personPeriodRepository, _eventPublisher, auow, personPeriodFilter, personPeriodTransformer);
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
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(0);
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
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(0);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodWithStartBeforeAnalyticsDates_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalyticsWithNewStartDate()
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
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_personPeriodRepository.GetPersonPeriods(testPerson1Id)
				.First().ValidFromDate.Should()
				.Be.EqualTo(new DateTime(2015, 1, 1));
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);
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
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(0);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnDayBeforeStartAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalyticsWithNewStartDate()
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
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_personPeriodRepository.GetPersonPeriods(testPerson1Id)
				.First().ValidFromDate.Should()
				.Be.EqualTo(new DateTime(2015, 1, 1));

			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void NewPersonPeriodWithSkillNotYetInAnalytics_HandleEvent_ShouldWorkAndPublishEvents()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var r = newTestPersonPeriod((new DateTime(2015, 1, 1)).AddDays(-1));

			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Domain.Forecasting.Skill("for test", "sdf", Color.Blue, 3, skType);
			skill.Activity = act;
			skill.TimeZone = (TimeZoneInfo.Local);

			r.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			person.AddPersonPeriod(r);

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
		public void NewPersonPeriodWithSkillNotYetInAnalytics_HandleEvent_ShouldWorkAndPublishEventsWithSkillsWhichExistsInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var r = newTestPersonPeriod((new DateTime(2015, 1, 1)).AddDays(-1));

			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Domain.Forecasting.Skill("for test", "sdf", Color.Blue, 3, skType);
			skill.SetId(fakeSkill3.SkillCode);
			skill.Activity = act;
			skill.TimeZone = (TimeZoneInfo.Local);

			ISkill skill2 = new Domain.Forecasting.Skill("for test2", "sdf2", Color.Blue, 3, skType);
			skill.Activity = act;
			skill.TimeZone = (TimeZoneInfo.Local);

			r.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			r.AddPersonSkill(new PersonSkill(skill2, new Percent(2)));
			person.AddPersonPeriod(r);

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(1);

			var skillEvent = ((AnalyticsPersonPeriodSkillsChangedEvent)
				_eventPublisher.PublishedEvents.First(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)));

			skillEvent.AnalyticsActiveSkillsId.Count.Should().Be.EqualTo(1);
			skillEvent.AnalyticsInactiveSkillsId.Count.Should().Be.EqualTo(0);
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
