using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[DomainTest]
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
			Data.WithAgent("pierre")
				.WithPeriod("2016-02-01");

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			Now.Is("2016-02-01 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2016-02-01", teamId, siteId, businessUnitId);

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.Timestamp.Should().Be("2016-02-01 00:00".Utc());
			@event.TeamId.Should().Be(teamId);
			@event.SiteId.Should().Be(siteId);
			@event.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWhenPeriodChangesInIstanbul()
		{
			Now.Is("2016-02-01 22:00");
			Data.WithAgent("pierre", TimeZoneInfoFactory.IstanbulTimeZoneInfo())
				.WithPeriod("2016-02-02");

			Target.Handle(new TenantHourTickEvent());
			
			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}

		[Test]
		public void ShouldWorkWithoutPersonPeriod()
		{
			Now.Is("2016-02-01 22:00");
			Data.WithPerson("pierre");

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}
	}
}