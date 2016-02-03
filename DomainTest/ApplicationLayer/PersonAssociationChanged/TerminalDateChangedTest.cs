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
	public class TerminalDateChangedTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		
		[Test]
		public void ShouldPublishWhenChangedFromTheFutureToThePast()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", "2016-12-31");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-12-31".Utc(),
				TerminationDate = "2016-01-01".Utc()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenChangedFromTodayToThePast()
		{
			Now.Is("2016-01-14 09:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", "2016-01-14");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-01-14".Utc(),
				TerminationDate = "2016-01-05".Utc()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishPropertiesWhenChangedFromTheFutureToThePast()
		{
			Now.Is("2016-01-18 08:15");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", "2016-12-31");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-12-31".Utc(),
				TerminationDate = "2016-01-01".Utc()
			});

			var result = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single();
			result.PersonId.Should().Be(personId);
			result.TeamId.Should().Be(null);
			result.Timestamp.Should().Be("2016-01-18 08:15".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenPushedForward()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-06-30");

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
			Data.WithAgent(personId, "Pierre", "2016-06-30");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-06-30".Utc(),
				TerminationDate = null
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenReactivated()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-01-01", teamId);

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-01-01".Utc(),
				TerminationDate = "2016-12-31".Utc()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenReactivatedIndefinetely()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-01-01", teamId);

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-01-01".Utc(),
				TerminationDate = null
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishCurrentTeamWhenReactivated()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-01-01", teamId);

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				TeamId = teamId,
				PreviousTerminationDate = "2016-01-01".Utc(),
				TerminationDate = null
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldNotPublishWhenPushedForwardInThePast()
		{
			Now.Is("2016-01-18 00:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "Pierre", "2016-01-02");

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				PreviousTerminationDate = "2016-01-02".Utc(),
				TerminationDate = "2016-01-03".Utc()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenChangedFromTheFutureToThePastInIstanbul()
		{
			Now.Is("2016-01-18 22:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", TimeZoneInfoFactory.IstanbulTimeZoneInfo());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				TimeZoneInfoId = TimeZoneInfoFactory.IstanbulTimeZoneInfo().Id,
				PreviousTerminationDate = "2016-01-31".Unspecified(),
				TerminationDate = "2016-01-18".Unspecified()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}


		[Test]
		public void ShouldNotPublishWhenChangedFromTheFutureToThePastInIstanbul()
		{
			Now.Is("2016-01-18 21:00");
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", TimeZoneInfoFactory.IstanbulTimeZoneInfo());

			Target.Handle(new PersonTerminalDateChangedEvent
			{
				PersonId = personId,
				TimeZoneInfoId = TimeZoneInfoFactory.IstanbulTimeZoneInfo().Id,
				PreviousTerminationDate = "2016-01-31".Unspecified(),
				TerminationDate = "2016-01-18".Unspecified()
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Be.Empty();
		}
	}

}