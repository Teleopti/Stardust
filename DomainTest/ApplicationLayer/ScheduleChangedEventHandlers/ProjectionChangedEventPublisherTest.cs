using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedEventForTest : ScheduleChangedEvent
	{
		public ScheduleChangedEventForTest()
		{
			LogOnDatasource = FakeDatabase.DefaultTenantName;
			LogOnBusinessUnitId = FakeDatabase.DefaultBusinessUnitId;
		}
	}

	[TestFixture]
	[DomainTest]
	public class ProjectionChangedEventPublisherTest
	{
		public ProjectionChangedEventPublisher Target;
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;

		[Test]
		public void ShouldPublishWithDayOffData3()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithDayOffTemplate("Day off", "DO")
				.WithAssignment("2016-10-02", person)
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-02 00:00".Utc(),
				EndDateTime = "2016-10-02 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-02".Utc());
			scheduleDay.DayOff.Should().Not.Be.Null();
			scheduleDay.IsFullDayAbsence.Should().Be.False();
			scheduleDay.ShortName.Should().Be("DO");
			scheduleDay.Name.Should().Be("Day off");
		}

		[Test]
		public void ShouldPublishWithDayOffStartTimeAndEndTime()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej", TimeZoneInfoFactory.StockholmTimeZoneInfo())
				.WithScenario(scenario)
				.WithDayOffTemplate("Day off", "DO")
				.WithAssignment("2016-10-08", person)
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.DayOff.StartDateTime.Should().Be.EqualTo("2016-10-07 22:00".Utc());
			scheduleDay.DayOff.EndDateTime.Should().Be.EqualTo("2016-10-08 22:00".Utc());
		}

		[Test]
		public void ShouldPublishWithFullDayAbsence()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithPersonAbsence("2016-10-08")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldPublishWithFullDayAbsenceForConfidentialAbsence()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithConfidentialAbsence()
				.WithPersonAbsence("2016-10-08")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldPublishWithFullDayAbsenceOnDayOff()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-08", person)
				.WithDayOff()
				.WithPersonAbsence("2016-10-08")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldIncludeTheDayBeforeBecauseOfNightShift()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-07", person)
				.WithActivty("2016-10-07 22:00", "2016-10-08 06:00")
				.WithAssignment("2016-10-08", person)
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var day1 = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc());
			var day2 = Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			day1.Shift.Layers.Should().Have.Count.EqualTo(1);
			day2.DayOff.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPublishWithVersion()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-07", person)
				.WithActivty("2016-10-07 22:00", "2016-10-08 06:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			 Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
				.Version.Should().Be(1);
		}

		[Test]
		public void ShouldIncrementVersion()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-07", person)
				.WithActivty("2016-10-07 08:00", "2016-10-07 17:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-07 08:00".Utc(),
				EndDateTime = "2016-10-07 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});
			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-07 08:00".Utc(),
				EndDateTime = "2016-10-07 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			Publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Last()
			   .ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
			   .Version.Should().Be(2);
		}


		[Test]
		public void ShouldPublishVersionForEachDayAndPerson()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person1, "jågej")
				.WithAgent(person2, "hejman")
				.WithScenario(scenario)
				.WithAssignment("2016-10-07", person1)
				.WithAssignment("2016-10-08", person2)
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-07 08:00".Utc(),
				EndDateTime = "2016-10-07 17:00".Utc(),
				PersonId = person1,
				ScenarioId = scenario
			});
			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-07 08:00".Utc(),
				EndDateTime = "2016-10-07 17:00".Utc(),
				PersonId = person2,
				ScenarioId = scenario
			});
			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-07 08:00".Utc(),
				EndDateTime = "2016-10-07 17:00".Utc(),
				PersonId = person2,
				ScenarioId = scenario
			});

			Publisher.PublishedEvents.OfType<ProjectionChangedEvent>()
				.Single(x => x.PersonId == person1)
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
				.Version.Should().Be(1);
			Publisher.PublishedEvents.OfType<ProjectionChangedEvent>()
				.Last(x => x.PersonId == person2)
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
				.Version.Should().Be(2);
		}
	}
}