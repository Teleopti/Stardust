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
	public class PublishPersonTerminatedChangeEventTest
	{
		public MutableNow Now;

		[Test]
		public void ShouldPublishWhenChangingTerminationlDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			Now.Is("2017-01-25");

			person.TerminatePerson("2017-01-31".Date(), new PersonAccountUpdaterDummy());

			((Person) person).PopAllEvents().OfType<PersonTerminalDateChangedEvent>().Should().Not.Be.Empty();
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

			person.TerminatePerson("2017-01-31".Date(), new PersonAccountUpdaterDummy());

			var @event = ((Person) person).PopAllEvents().OfType<PersonTerminalDateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.SiteId.Should().Be(siteId);
			@event.SiteName.Should().Be("siteName");
			@event.TeamId.Should().Be(teamId);
			@event.TeamName.Should().Be("teamName");
		}

		[Test]
		public void ShouldPublishWhenActivating()
		{
			var person = PersonFactory.CreatePersonWithId();
			Now.Is("2017-01-01");
			person.TerminatePerson("2017-01-02".Date(), new PersonAccountUpdaterDummy());
			((Person) person).PopAllEvents();
			Now.Is("2017-01-25");
			person.ActivatePerson(new PersonAccountUpdaterDummy());

			((Person) person).PopAllEvents().OfType<PersonTerminalDateChangedEvent>()
				.Single()
				.TerminationDate.Should().Be(null);
		}
	}
}