using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	[DomainTest]
	[TestFixture]
	public class RemovePeriodAsInAdherenceCommandHandlerTest
	{
		public RemoveApprovedPeriodCommandHandler Target;
		public FakeEventPublisher Publisher;
		public FakeUserTimeZone TimeZone;

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

	}
}