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
	[DomainTest]
	[AddDatasourceId]
	public class PeriodChangesTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenTeamChanged()
		{
			var person = Guid.NewGuid();
			var previousTeam = Guid.NewGuid();
			var newTeam = Guid.NewGuid();
			Now.Is("2016-02-01 00:00");
			Data.WithAgent(person, "pierre")
				.WithPeriod("2016-01-02", previousTeam)
				.WithPeriod("2016-02-01", newTeam);

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == person)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Now.Is("2016-02-01 00:00");
			Data
				.WithPerson(personId, "pierre")
				.WithPeriod("2016-01-02", Guid.NewGuid(), siteId, businessUnitId)
				.WithPeriod("2016-02-01", teamId, siteId, businessUnitId);

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == personId);
			@event.PersonId.Should().Be(personId);
			@event.Timestamp.Should().Be("2016-02-01 00:00".Utc());
			@event.TeamId.Should().Be(teamId);
			@event.SiteId.Should().Be(siteId);
			@event.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPublishWithTeamAndSiteName()
		{
			var person = Guid.NewGuid();
			var previousTeam = Guid.NewGuid();
			var newTeam = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Now.Is("2016-02-01 00:00");
			Data
				.WithPerson(person, "pierre")
				.WithSite(siteId, "siteName")
				.WithPeriod("2016-01-02", previousTeam, siteId)
				.WithTeam(newTeam, "teamName")
				.WithPeriod("2016-02-01", newTeam, siteId);

			Target.Handle(new TenantHourTickEvent());

			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == person);
			@event.TeamId.Should().Be(newTeam);
			@event.SiteId.Should().Be(siteId);
			@event.TeamName.Should().Be("teamName");
			@event.SiteName.Should().Be("siteName");
		}

		[Test]
		public void ShouldNotPublishWhenTeamDoesNotChange()
		{
			var teamId = Guid.NewGuid();
			Data.WithAgent("pierre")
				.WithPeriod("2016-01-02", teamId)
				.WithPeriod("2016-02-01", teamId);
			Now.Is("2016-02-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenPeriodChangesInIstanbul()
		{
			var person = Guid.NewGuid();
			var previousTeam = Guid.NewGuid();
			var newTeam = Guid.NewGuid();
			Now.Is("2016-02-01 22:00");
			Data.WithAgent(person, "pierre", TimeZoneInfoFactory.IstanbulTimeZoneInfo())
				.WithPeriod("2016-01-02", previousTeam)
				.WithPeriod("2016-02-02", newTeam);

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == person)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldWorkWithoutPersonPeriod()
		{
			var person = Guid.NewGuid();
			Now.Is("2016-02-01 22:00");
			Data.WithPerson("pierre");

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == person)
				.Should().Be.Empty();
		}
	}
}