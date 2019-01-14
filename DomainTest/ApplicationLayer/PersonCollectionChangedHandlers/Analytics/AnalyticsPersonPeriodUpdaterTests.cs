using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[DomainTest]
	public class AnalyticsPersonPeriodUpdaterTests : IExtendSystem
	{
		public AnalyticsPersonPeriodUpdater Target;
		public IAnalyticsPersonPeriodRepository PersonPeriodRepository;
		public FakeAnalyticsSkillRepository AnalyticsSkillRepository;
		public FakePersonRepository PersonRepository;

		public FakeEventPublisher EventPublisher;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;
		public IAnalyticsIntervalRepository AnalyticsIntervalRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		private Guid testPerson1Id;
		private readonly Guid _businessUnitId = Guid.NewGuid();

		private readonly AnalyticsSkill fakeSkill3 = new AnalyticsSkill
		{
			SkillId = 3,
			SkillCode = Guid.NewGuid()
		};

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsPersonPeriodUpdater>();
		}

		private void basicSetup()
		{
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			AnalyticsSkillRepository.SetSkills(new List<AnalyticsSkill> { fakeSkill3 });
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));

			var p1 = new Person
			{
				Email = "test1@test.se"
			}.WithName(new Name("Test1", "Testsson")).WithId();
			p1.SetEmploymentNumber("E321");
			var p2 = new Person
			{
				Email = "test2@test.se"
			}.WithName(new Name("Test2", "Testsson")).WithId();
			p2.SetEmploymentNumber("E321");
			var p3 = new Person
			{
				Email = "test3@test.se"
			}.WithName(new Name("Test3", "Testsson")).WithId();
			p3.SetEmploymentNumber("E321");

			testPerson1Id = p1.Id.GetValueOrDefault();

			PersonRepository.Add(p1);
			PersonRepository.Add(p2);
			PersonRepository.Add(p3);
		}

		[Test]
		public void NoPersonPeriodOnPerson_HandlePersonPeriodChanged_NoPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(0, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(0);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriod_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding a person period
			PersonRepository.FindPeople(new List<Guid> { testPerson1Id })
				.First()
				.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then there should be one person period for that person
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void TwoNewPersonPeriods_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding two person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 2, 1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then there should be one person period for that person
			Assert.AreEqual(2, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void NewPersonPeriodWithLaterStart_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2020, 1, 1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void NewPersonPeriodWithStartBeforeAnalyticsDates_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalyticsWithNewStartDate()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2001, 1, 1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void NewPersonPeriodOnStartAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2015, 1, 1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void NewPersonPeriodOnEndAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenAddedPersonPeriod()
		{
			basicSetup();

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod()
			{
				PersonId = 1,
				PersonCode = testPerson1Id
			});

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(0, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodEndChanged()
		{
			basicSetup();

			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var personPeriodId = Guid.NewGuid();
			var startDate = new DateTime(2017, 12, 31);
			person.AddPersonPeriod(newTestPersonPeriod(startDate, personPeriodId));
			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				ValidFromDateIdLocal = AnalyticsDateRepository.Date(startDate).DateId,
				ValidToDateIdLocal = AnalyticsDateRepository.Date(startDate).DateId+1,
				PersonPeriodCode = personPeriodId
			});

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodStartChanged()
		{
			basicSetup();

			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var personPeriodId = Guid.NewGuid();
			var startDate = new DateTime(2017, 12, 31);
			person.AddPersonPeriod(newTestPersonPeriod(startDate, personPeriodId));
			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				ValidFromDateIdLocal = AnalyticsDateRepository.Date(startDate).DateId -1,
				ValidToDateIdLocal = AnalyticsDateRepository.Date(startDate).DateId,
				PersonPeriodCode = personPeriodId
			});

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPublishAnalyticsPersonPeriodRangeChangedEventWhenDeletedPersonPeriod()
		{
			basicSetup();

			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31)));

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishAnalyticsPersonPeriodRangeChangedEventWhenPersonPeriodAlreadyDeleted()
		{
			basicSetup();

			PersonPeriodRepository.AddOrUpdatePersonPeriod(new AnalyticsPersonPeriod
			{
				PersonId = 1,
				PersonCode = testPerson1Id,
				PersonPeriodCode = Guid.NewGuid(),
				ToBeDeleted = true
			});

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodRangeChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnDayAfterEndAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalytics()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2017, 12, 31).AddDays(1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonCollectionChangedEvent)).Should().Be.EqualTo(1);
			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(AnalyticsPersonPeriodSkillsChangedEvent)).Should().Be.EqualTo(0);
		}

		[Test]
		public void NewPersonPeriodOnDayBeforeStartAnalyticsDate_HandlePersonPeriodChanged_NewPersonPeriodAddedInAnalyticsWithNewStartDate()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			person.AddPersonPeriod(newTestPersonPeriod(new DateTime(2015, 1, 1).AddDays(-1)));

			// When handling event
			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			// Then
			Assert.AreEqual(1, PersonPeriodRepository.GetPersonPeriods(testPerson1Id).Count);
			PersonPeriodRepository.GetPersonPeriods(testPerson1Id)
				.First().ValidFromDate.Should()
				.Be.EqualTo(new DateTime(2015, 1, 1));
		}

		[Test]
		public void NewPersonPeriodWithSkillNotYetInAnalytics_HandleEvent_ShouldThrow()
		{
			basicSetup();

			// Given when adding person period
			var person = PersonRepository.FindPeople(new List<Guid> { testPerson1Id }).First();
			var r = newTestPersonPeriod(new DateTime(2015, 1, 1).AddDays(-1));

			IActivity act = new Activity("for test");
			ISkillType skType = SkillTypeFactory.CreateSkillTypePhone();
			var skill = new Domain.Forecasting.Skill("for test", "sdf", Color.Blue, 3, skType)
			{
				Activity = act,
				TimeZone = TimeZoneInfo.Local
			};
			r.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			person.AddPersonPeriod(r);

			Assert.Throws<ArgumentException>(() =>
			{
				Target.Handle(new PersonCollectionChangedEvent
				{
					PersonIdCollection = { testPerson1Id },
					LogOnBusinessUnitId = _businessUnitId
				});
			});
		}
		
		[Test]
		public void ShouldPublishEventForTimeZoneChanges()
		{
			basicSetup();

			PersonRepository.FindPeople(new List<Guid> { testPerson1Id })
				.First()
				.AddPersonPeriod(newTestPersonPeriod(new DateTime(2016, 1, 1)));

			Target.Handle(new PersonCollectionChangedEvent
			{
				PersonIdCollection = { testPerson1Id },
				LogOnBusinessUnitId = _businessUnitId
			});

			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(PossibleTimeZoneChangeEvent))
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishEventForTimeZoneChangesIfNoPersonChanges()
		{
			//basicSetup();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			Target.Handle(new PersonCollectionChangedEvent
			{
				LogOnBusinessUnitId = _businessUnitId
			});

			EventPublisher.PublishedEvents.Any(a => a.GetType() == typeof(PossibleTimeZoneChangeEvent))
				.Should().Be.False();
		}


		private static PersonPeriod newTestPersonPeriod(DateTime startDate, Guid? id = null)
		{
			return new PersonPeriod(new DateOnly(startDate),
				new PersonContract(new Contract("ContractNameTest"),
					new PartTimePercentage("100%"),
					new ContractSchedule("ScheduleName")),
				new Team
				{
					Site = new Site("SiteName")
				}).WithId(id ?? Guid.NewGuid());
		}
	}
}
