using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PublishPersonTeamChangedEventTest
	{
		public MutableNow Now;

		[Test]
		public void ShouldPublishWhenChangingTeam()
		{
			Now.Is("2017-01-25");
			var team = TeamFactory.CreateSimpleTeam();
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam("2017-01-01".Date(), team);

			person.ChangeTeam(team, person.Period("2017-01-25".Date()));

			((Person) person).PopAllEvents(null).OfType<PersonTeamChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			Now.Is("2017-01-25");
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var organization = OrganizationFactory.Create(businessUnitId, siteId, "siteName", teamId, "teamName");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(personId, "2017-01-01".Date(), organization.Team);

			person.ChangeTeam(organization.Team, person.Period("2017-01-25".Date()));

			var @event = ((Person) person).PopAllEvents(null).OfType<PersonTeamChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.CurrentBusinessUnitId.Should().Be(businessUnitId);
			@event.CurrentSiteId.Should().Be(siteId);
			@event.CurrentSiteName.Should().Be("siteName");
			@event.CurrentTeamId.Should().Be(teamId);
			@event.CurrentTeamName.Should().Be("teamName");
		}
	}
}