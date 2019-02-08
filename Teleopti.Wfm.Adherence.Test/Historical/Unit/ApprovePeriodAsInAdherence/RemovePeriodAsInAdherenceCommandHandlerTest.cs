using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ApprovePeriodAsInAdherence
{
	[DomainTest]
	public class RemovePeriodAsInAdherenceCommandHandlerTest
	{
		public RemoveApprovedPeriodCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;
		public FakeDatabase Database;

		[Test]
		public void ShouldRemove()
		{
			var person = Guid.NewGuid();

			Target.Handle(new RemoveApprovedPeriodCommand
			{
				PersonId = person,
				StartDateTime = "2018-03-08 08:05:00",
				EndDateTime = "2018-03-08 08:15:00"
			});

			var published = Publisher.PublishedEvents.OfType<ApprovedPeriodRemovedEvent>().Single();
			published.PersonId.Should().Be(person);
			published.StartTime.Should().Be("2018-03-08 08:05:00".Utc());
			published.EndTime.Should().Be("2018-03-08 08:15:00".Utc());
		}

		[Test]
		public void ShouldRemoveFromUsersTimeZone()
		{
			var person = Guid.NewGuid();
			TimeZone.IsSweden();

			Target.Handle(new RemoveApprovedPeriodCommand
			{
				PersonId = person,
				StartDateTime = "2018-03-08 08:00:00",
				EndDateTime = "2018-03-08 09:00:00"
			});

			var published = Publisher.PublishedEvents.OfType<ApprovedPeriodRemovedEvent>().Single();
			published.StartTime.Should().Be("2018-03-08 07:00:00".Utc());
			published.EndTime.Should().Be("2018-03-08 08:00:00".Utc());
		}

		[Test]
		public void ShouldRemoveFromUsersTimeZone24Hours()
		{
			var person = Guid.NewGuid();
			TimeZone.IsSweden();

			Target.Handle(new RemoveApprovedPeriodCommand
			{
				PersonId = person,
				StartDateTime = "2018-03-08 15:00:00",
				EndDateTime = "2018-03-08 16:00:00"
			});

			var published = Publisher.PublishedEvents.OfType<ApprovedPeriodRemovedEvent>().Single();
			published.StartTime.Should().Be("2018-03-08 14:00:00".Utc());
			published.EndTime.Should().Be("2018-03-08 15:00:00".Utc());
		}

		[Test]
		public void ShouldRemoveWithShiftDate()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment("2018-10-26")
				.WithAssignedActivity("2018-10-26 09:00", "2018-10-26 17:00")
				;

			Target.Handle(new RemoveApprovedPeriodCommand
			{
				PersonId = person,
				StartDateTime = "2018-10-26 09:00:00",
				EndDateTime = "2018-10-26 10:00:00"
			});

			Publisher.PublishedEvents.OfType<ApprovedPeriodRemovedEvent>().Single()
				.BelongsToDate.Should().Be("2018-10-26".Date());
		}
	}
}