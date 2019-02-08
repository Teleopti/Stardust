using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[AddDatasourceId]
	[DomainTest]
	public class TerminalDateChangedTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		
		[Test]
		public void ShouldPublishPropertiesWhenChangedFromTheFutureToThePast()
		{
			Now.Is("2016-01-18 08:15");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data
				.WithSite(siteId, "site")
				.WithTeam(teamId, "team")
				.WithAgent(personId, "pierre", "2016-12-31", teamId);
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Data.WithTerminalDate("2016-01-01");
			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-12-31".Utc(),
				TerminationDate = "2016-01-01".Utc()
			});

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			result.PersonId.Should().Be(personId);
			result.SiteId.Should().Be(null);
			result.SiteName.Should().Be(null);
			result.TeamId.Should().Be(null);
			result.TeamName.Should().Be(null);
			result.ExternalLogons.Should().Be.Empty();
			result.Timestamp.Should().Be("2016-01-18 08:15".Utc());
		}
		
		[Test]
		public void ShouldNotPublishWhenPushedForward()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-06-30", teamId);
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-06-30".Utc(),
				TerminationDate = "2016-12-31".Utc()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishWhenPushedForwardIndefinetely()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-06-30", teamId);
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-06-30".Utc(),
				TerminationDate = null
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Be.Empty();
		}
		
	}
}