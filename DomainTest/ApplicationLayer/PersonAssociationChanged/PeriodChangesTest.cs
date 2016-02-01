using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	[Toggle(Toggles.RTA_TeamChanges_36043)]
	public class PeriodChangesTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenPeriodChanges()
		{
			Now.Is("2016-02-01 00:00");
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}
		
		[Test]
		public void ShouldPublishWithPersonId()
		{
			Now.Is("2016-02-01 00:00");
			var personId = Guid.NewGuid();
			Data.HasPerson(personId, "pierre")
				.WithPeriod("2016-02-01");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().PersonId
				.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWithTimestamp()
		{
			Now.Is("2016-02-01 00:00");
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().Timestamp
				.Should().Be("2016-02-01 00:00".Utc());
		}

		[Test]
		public void ShouldPublishWithTeamId()
		{
			Now.Is("2016-02-01 00:00");
			var teamId = Guid.NewGuid();
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01", teamId);

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().TeamId
				.Should().Be(teamId);
		}

		[Test]
		public void ShouldPublishWithSiteId()
		{
			Now.Is("2016-02-01 00:00");
			var siteId = Guid.NewGuid();
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01", Guid.NewGuid(), siteId);

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().SiteId
				.Should().Be(siteId);
		}

		[Test]
		public void ShouldPublishWithBusinessUnitId()
		{
			Now.Is("2016-02-01 00:00");
			var businessUnitId = Guid.NewGuid();
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01", Guid.NewGuid(), Guid.NewGuid(), businessUnitId);

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().BusinessUnitId
				.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWhenPeriodChangesInIstanbul()
		{
			Now.Is("2016-02-01 22:00");
			Data.HasPerson("pierre", TimeZoneInfoFactory.IstanbulTimeZoneInfo())
				.WithPeriod("2016-02-02");

			Target.Handle(new TenantHearbeatEvent());
			
			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}
	}
}