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
	[TestFixture]
	[DomainTest]
	public class ProjectionChangedEventPublisherTest
	{
		public MutableNow now;
		public FakeEventPublisher publisher;
		public ProjectionChangedEventPublisher target;
		public FakeDatabase Database;

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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-02 00:00".Utc(),
				EndDateTime = "2016-10-02 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var scheduleDay = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
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

			target.Handle(new ScheduleChangedEvent
			{
				StartDateTime = "2016-10-08 00:00".Utc(),
				EndDateTime = "2016-10-08 23:59".Utc(),
				PersonId = person,
				ScenarioId = scenario
			});

			var day1 = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-07".Utc());
			var day2 = publisher.PublishedEvents.OfType<ProjectionChangedEvent>().Single()
				.ScheduleDays.Single(x => x.Date == "2016-10-08".Utc());
			day1.Shift.Layers.Should().Have.Count.EqualTo(1);
			day2.DayOff.Should().Not.Be.Null();
		}
	}
}