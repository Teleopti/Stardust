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
		public void ShouldPublishWhenChangingTerminationDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			Now.Is("2017-01-25");

			person.TerminatePerson("2017-01-31".Date(), new PersonAccountUpdaterDummy());

			((Person) person).PopAllEvents(null).OfType<PersonTerminalDateChangedEvent>().Should().Not.Be.Empty();
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

			var @event = ((Person) person).PopAllEvents(null).OfType<PersonTerminalDateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWhenActivating()
		{
			var person = PersonFactory.CreatePersonWithId();
			Now.Is("2017-01-01");
			person.TerminatePerson("2017-01-02".Date(), new PersonAccountUpdaterDummy());
			((Person) person).PopAllEvents(null);
			Now.Is("2017-01-25");
			person.ActivatePerson(new PersonAccountUpdaterDummy());

			((Person) person).PopAllEvents(null).OfType<PersonTerminalDateChangedEvent>()
				.Single()
				.TerminationDate.Should().Be(null);
		}

		[Test]
		public void ShouldNotPublishMoreThanOneEvent()
		{
			var person = PersonFactory.CreatePersonWithId();
			Now.Is("2017-01-25");

			person.TerminatePerson("2017-01-31".Date(), new PersonAccountUpdaterDummy());
			person.TerminatePerson("2017-02-01".Date(), new PersonAccountUpdaterDummy());

			var theEvent = ((Person)person).PopAllEvents(null).OfType<PersonTerminalDateChangedEvent>().Single();
			theEvent.TerminationDate.Value.Should().Be(new DateTime(2017, 2, 1));
		}
	}
}