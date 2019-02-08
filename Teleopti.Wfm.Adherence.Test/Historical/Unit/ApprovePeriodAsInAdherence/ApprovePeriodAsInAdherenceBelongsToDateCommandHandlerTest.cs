using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ApprovePeriodAsInAdherence
{
	[DomainTest]
	public class ApprovePeriodAsInAdherenceBelongsToDateCommandHandlerTest
	{
		public ApprovePeriodAsInAdherenceCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;
		public FakeDatabase Database;

		[Test]
		public void ShouldApproveWithShiftDate()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 09:00", "2018-10-26 17:00")
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-26 09:00:00",
				EndDateTime = "2018-10-26 10:00:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-26".Date());
		}

		[Test]
		public void ShouldApproveWithShiftDateOfNightShift()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 23:00", "2018-10-27 07:00")
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-27 06:00:00",
				EndDateTime = "2018-10-27 07:00:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-26".Date());
		}

		[Test]
		public void ShouldApproveWithShiftDateInChina()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithAssignment("2018-10-27")
				.WithAssignedActivity("2018-10-26 18:00", "2018-10-26 23:00") //01:00AM - 06:00AM in china
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-26 22:00:00",
				EndDateTime = "2018-10-26 23:00:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-27".Date());
		}

		[Test]
		public void ShouldApproveWithShiftDateOfShiftStarting()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 00:30", "2018-10-26 08:00")
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-25 23:30:00",
				EndDateTime = "2018-10-25 23:45:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-26".Date());
		}
		
		[Test]
		public void ShouldApproveWithShiftDateOfShiftEnding()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 18:30", "2018-10-26 23:30")
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-27 00:15:00",
				EndDateTime = "2018-10-27 00:30:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-26".Date());
		}
		
		[Test]
		public void ShouldApproveWithAgentsDateInChinaIfNoShiftNear()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 09:00", "2018-10-26 17:00")
				;

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-26 19:00:00",
				EndDateTime = "2018-10-26 20:00:00"
			});

			Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-27".Date());
		}
	}
}