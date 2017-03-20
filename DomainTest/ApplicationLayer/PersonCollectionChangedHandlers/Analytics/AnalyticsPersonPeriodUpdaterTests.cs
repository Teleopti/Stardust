using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
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
			_personRepository = new FakePersonRepositoryLegacy2();
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
				Email = "test1@test.se",
			}.WithName(new Name("Test1", "Testsson")).WithId();
			p1.SetEmploymentNumber("E321");
			var p2 = new Person
			{
				Email = "test2@test.se",
			}.WithName(new Name("Test2", "Testsson")).WithId();
			p2.SetEmploymentNumber("E321");
			var p3 = new Person
			{
				Email = "test3@test.se",
			}.WithName(new Name("Test3", "Testsson")).WithId();
			p3.SetEmploymentNumber("E321");

			testPerson1Id = p1.Id.GetValueOrDefault();

			_personRepository.Add(p1);
			_personRepository.Add(p2);
			_personRepository.Add(p3);
			
			var personPeriodFilter = new PersonPeriodFilter(_analyticsDateRepository);
			var personPeriodTransformer = new PersonPeriodTransformer(_personPeriodRepository, 
				_analyticsSkillRepository, 
				_analyticsBusinessUnitRepository, 
				_analyticsTeamRepository, 
				new ReturnNotDefined(),
				_analyticsTimeZoneRepository,
				_analyticsIntervalRepository,
				_globalSettingDataRepository, new AnalyticsPersonPeriodDateFixer(_analyticsDateRepository, _analyticsIntervalRepository));

			_target = new AnalyticsPersonPeriodUpdater(_personRepository, _personPeriodRepository, new EventPopulatingPublisher(new CurrentEventPublisher(_eventPublisher), new DummyInfrastructureInfoPopulator()), new ThisAnalyticsUnitOfWork(new FakeUnitOfWork()), personPeriodFilter, personPeriodTransformer);
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
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenAddedPersonPeriod()
		{
			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod()
			{
				PersonId = 1,
				PersonCode = testPerson1Id
			});

			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			Assert.AreEqual(0, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodEndChanged()
		{
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var personPeriodId = Guid.NewGuid();
			var startDate = new DateTime(2017, 12, 31);
			person.AddPersonPeriod(newTestPersonPeriod(startDate, personPeriodId));
			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				ValidFromDateIdLocal = _analyticsDateRepository.Date(startDate).DateId,
				ValidToDateIdLocal = _analyticsDateRepository.Date(startDate).DateId+1,
				PersonPeriodCode = personPeriodId
			});

			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodStartChanged()
		{
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var personPeriodId = Guid.NewGuid();
			var startDate = new DateTime(2017, 12, 31);
			person.AddPersonPeriod(newTestPersonPeriod(startDate, personPeriodId));
			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				ValidFromDateIdLocal = _analyticsDateRepository.Date(startDate).DateId -1,
				ValidToDateIdLocal = _analyticsDateRepository.Date(startDate).DateId,
				PersonPeriodCode = personPeriodId
			});

			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenDeletedPersonPeriod()
		{
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31)));

			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodAlreadyDeleted()
		{
			_personPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				PersonPeriodCode = Guid.NewGuid(),
				ToBeDeleted = true
			});

			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			_eventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnDayAfterEndAnalyticsDate_HandlePersonPeriodChanged_NoNewPersonPeriodAddedInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31).AddDays(1)));

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
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2015, 1, 1).AddDays(-1)));

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
		}

		[Test]
		public void NewPersonPeriodWithSkillNotYetInAnalytics_HandleEvent_ShouldWorkAndPublishEvents()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var r = newTestPersonPeriod(new DateTime(2015, 1, 1).AddDays(-1));

			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Domain.Forecasting.Skill("for test", "sdf", Color.Blue, 3, skType);
			skill.Activity = act;
			skill.TimeZone = TimeZoneInfo.Local;

			r.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			person.AddPersonPeriod(r);

			// When handling event
			_target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id }
			});

			// Then
			Assert.AreEqual(1, _personPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void NewPersonPeriodWithSkillNotYetInAnalytics_HandleEvent_ShouldWorkAndPublishEventsWithSkillsWhichExistsInAnalytics()
		{
			// Given when adding person period
			var person = _personRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var r = newTestPersonPeriod(new DateTime(2015, 1, 1).AddDays(-1));

			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillType();
			ISkill skill = new Domain.Forecasting.Skill("for test", "sdf", Color.Blue, 3, skType);
			skill.SetId(fakeSkill3.SkillCode);
			skill.Activity = act;
			skill.TimeZone = TimeZoneInfo.Local;

			ISkill skill2 = new Domain.Forecasting.Skill("for test2", "sdf2", Color.Blue, 3, skType);
			skill.Activity = act;
			skill.TimeZone = TimeZoneInfo.Local;

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
