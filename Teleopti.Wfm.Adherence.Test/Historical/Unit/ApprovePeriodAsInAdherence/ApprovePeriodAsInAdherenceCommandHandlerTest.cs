using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.Approval;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ApprovePeriodAsInAdherence
{
	[DomainTest]
	[TestFixture]
	public class ApprovePeriodAsInAdherenceCommandHandlerTest
	{
		public ApprovePeriodAsInAdherenceCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;
		
		[Test]
		public void ShouldApprove()
		{
			var person = Guid.NewGuid();

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-01-29 08:05:00",
				EndDateTime = "2018-01-29 08:15:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single();
			published.PersonId.Should().Be(person);
			published.StartTime.Should().Be("2018-01-29 08:05:00".Utc());
			published.EndTime.Should().Be("2018-01-29 08:15:00".Utc());
		}

		[Test]
		public void ShouldApproveFromUsersTimeZone()
		{
			var person = Guid.NewGuid();
			TimeZone.IsSweden();

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-01-29 08:00:00",
				EndDateTime = "2018-01-29 09:00:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single();
			published.StartTime.Should().Be("2018-01-29 07:00:00".Utc());
			published.EndTime.Should().Be("2018-01-29 08:00:00".Utc());
		}

		[Test]
		public void ShouldApproveFromUsersTimeZone24Hours()
		{
			var person = Guid.NewGuid();
			TimeZone.IsSweden();

			Target.Handle(new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-01-29 15:00:00",
				EndDateTime = "2018-01-29 16:00:00"
			});

			var published = Publisher.PublishedEvents.OfType<PeriodApprovedAsInAdherenceEvent>().Single();
			published.StartTime.Should().Be("2018-01-29 14:00:00".Utc());
			published.EndTime.Should().Be("2018-01-29 15:00:00".Utc());
		}

		[Test]
		public void ShouldNotAllowEndTimeBeforeStartTime()
		{
			var person = Guid.NewGuid();
			var command = new ApprovePeriodAsInAdherenceCommand
			{
				PersonId = person,
				StartDateTime = "2018-01-29 17:00:00",
				EndDateTime = "2018-01-29 16:00:00"
			};
			Publisher.Clear();
			
			Assert.Throws<ArgumentOutOfRangeException>(() => Target.Handle(command));
			Publisher.PublishedEvents.Should().Be.Empty();
		}

	}
}