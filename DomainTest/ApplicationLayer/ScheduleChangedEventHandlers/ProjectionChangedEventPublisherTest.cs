using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			LogOnDatasource = DomainTestAttribute.DefaultTenantName;
			LogOnBusinessUnitId = DomainTestAttribute.DefaultBusinessUnitId;
		}
	}

	[DomainTest]
	[AddDatasourceId]
	public class ProjectionChangedEventPublisherTest
	{
		public ProjectionChangedEventPublisher Target;
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;

		[Test]
		public void ShouldPublishLeavingDateScheduleDay()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithActivity(phone)
				.WithAssignment("2016-06-28")
				.WithAssignedActivity("2016-06-28 08:00", "2016-06-28 17:00")
				.WithTerminalDate("2016-06-28")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-28 08:00".Utc(),
				EndDateTime = "2016-06-28 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay28 = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single().ScheduleDays.Single(x => x.Date == "2016-06-28".Utc());
			scheduleDay28.Shift.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPublishAfterLeavingDateScheduleDay()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithActivity(phone)
				.WithAssignment("2016-06-29")
				.WithAssignedActivity("2016-06-29 08:00", "2016-06-29 17:00")
				.WithTerminalDate("2016-06-28")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-29 08:00".Utc(),
				EndDateTime = "2016-06-29 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay29 = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single().ScheduleDays.Single(x => x.Date == "2016-06-29".Utc());
			scheduleDay29.Shift.Should().Be.Null();
			scheduleDay29.SiteId.Should().Be(Guid.Empty);
			scheduleDay29.TeamId.Should().Be(Guid.Empty);
			scheduleDay29.PersonPeriodId.Should().Be(Guid.Empty);
		}

		[Test]
		public void ShouldPublishWithActivity()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithActivity(phone)
				.WithScenario(scenario)
				.WithAssignment("2016-06-30")
				.WithAssignedActivity("2016-06-30 08:00", "2016-06-30 17:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-30 08:00".Utc(),
				EndDateTime = "2016-06-30 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-06-30".Utc());
			scheduleDay.Shift.Layers.Single().StartDateTime.Should().Be("2016-06-30 08:00".Utc());
			scheduleDay.Shift.Layers.Single().EndDateTime.Should().Be("2016-06-30 17:00".Utc());
			scheduleDay.Shift.Layers.Single().PayloadId.Should().Be(phone);
		}
		[Test]
		public void ShouldPublishWithMergedActivities()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithActivity(phone)
				.WithScenario(scenario)
				.WithAssignment("2016-06-30")
				.WithAssignedActivity("2016-06-30 08:00", "2016-06-30 09:00")
				.WithAssignedActivity("2016-06-30 08:45", "2016-06-30 10:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-06-30 08:00".Utc(),
				EndDateTime = "2016-06-30 17:00".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-06-30".Utc());
			scheduleDay.Shift.Layers.Single().PayloadId.Should().Be(phone);
		}

		[Test]
		public void ShouldPublishWithDayOffData()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithDayOffTemplate("Day off", "DO")
				.WithAssignment("2016-10-02")
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-02 00:00".Utc(),
				EndDateTime = "2016-10-02 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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
				.WithAssignment("2016-10-08")
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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
				.WithAssignment("2016-10-08")
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

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.IsFullDayAbsence.Should().Be.True();
		}

		[Test]
		public void ShouldPublishWithPartTimeAbsenceOnOverTimeAndDayOff()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-08")
				.WithAssignedOvertimeActivity("2016-10-08 08:00", "2016-10-08 11:00")
				.WithDayOff()
				.WithPersonAbsence("2016-10-08 08:00", "2016-10-08 09:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			scheduleDay.IsFullDayAbsence.Should().Be.False();
		}

		[Test]
		public void ShouldIncludeTheDayBeforeBecauseOfNightShift()
		{
			var person = Guid.NewGuid();
			var scenario = Guid.NewGuid();
			Database
				.WithAgent(person, "jågej")
				.WithScenario(scenario)
				.WithAssignment("2016-10-07")
				.WithAssignedActivity("2016-10-07 22:00", "2016-10-08 06:00")
				.WithAssignment("2016-10-08")
				.WithDayOff()
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var day1 = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc());
			var day2 = Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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
				.WithAssignment("2016-10-07")
				.WithAssignedActivity("2016-10-07 22:00", "2016-10-08 06:00")
				;

			Target.Handle(new ScheduleChangedEventForTest
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			 Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Single()
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
				.WithAssignment("2016-10-07")
				.WithAssignedActivity("2016-10-07 08:00", "2016-10-07 17:00")
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

			Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>().Last()
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
				.WithScenario(scenario)
				.WithAgent(person1, "jågej")
				.WithAssignment("2016-10-07")
				.WithAgent(person2, "hejman")
				.WithAssignment("2016-10-08")
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

			Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>()
				.Single(x => x.PersonId == person1)
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
				.Version.Should().Be(1);
			Publisher.PublishedEvents.OfType<ProjectionChangedEventNew>()
				.Last(x => x.PersonId == person2)
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc())
				.Version.Should().Be(2);
		}

	}
}