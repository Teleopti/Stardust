using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
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
	public class PublishPersonPeriodChangedEventTest
	{
		public MutableNow Now;

		[Test]
		public void ShouldPublishWhenPersonPeriodIsAdded()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());

			person.AddPersonPeriod(personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenPersonPeriodIsDeleted()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			((Person) person).PopAllEvents();

			person.DeletePersonPeriod(personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenPersonPeriodStartDateIsChanged()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			((Person) person).PopAllEvents();

			person.ChangePersonPeriodStartDate("2017-01-25".Date(), personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenRemoveAllPeriodsAfter()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-25".Date());
			person.AddPersonPeriod(personPeriod);
			((Person) person).PopAllEvents();

			person.RemoveAllPeriodsAfter("2017-01-01".Date());

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenRemoveAllPersonPeriods()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			((Person) person).PopAllEvents();

			person.RemoveAllPersonPeriods();

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenAddExternalLogOn()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			((Person) person).PopAllEvents();

			person.AddExternalLogOn(new ExternalLogOn(), personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenResetExternalLogOn()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			person.AddExternalLogOn(new ExternalLogOn(), personPeriod);
			((Person) person).PopAllEvents();

			person.ResetExternalLogOn(personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWhenRemoveExternalLogOn()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-01".Date());
			person.AddPersonPeriod(personPeriod);
			var existingExternalLogon = new ExternalLogOn();
			person.AddExternalLogOn(existingExternalLogon, personPeriod);
			((Person) person).PopAllEvents();

			person.RemoveExternalLogOn(existingExternalLogon, personPeriod);

			((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Should().Have.Count.EqualTo(1);
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
			var person = PersonFactory.CreatePersonWithId(personId);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2017-01-10".Date(), organization.Team);

			person.AddPersonPeriod(personPeriod);

			var @event = ((Person) person).PopAllEvents().OfType<PersonPeriodChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.CurrentBusinessUnitId.Should().Be(businessUnitId);
			@event.CurrentSiteId.Should().Be(siteId);
			@event.CurrentSiteName.Should().Be("siteName");
			@event.CurrentTeamId.Should().Be(teamId);
			@event.CurrentTeamName.Should().Be("teamName");
		}
	}
}