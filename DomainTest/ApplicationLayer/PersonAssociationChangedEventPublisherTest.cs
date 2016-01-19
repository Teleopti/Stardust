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
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	public class PersonAssociationChangedEventPublisherTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenTerminated()
		{
			Now.Is("2016-01-15 00:00");
			Data.HasPerson("pierre", "2016-01-14");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}

		[Test]
		public void ShouldPublishWithPersonId()
		{
			Now.Is("2016-01-15 00:00");
			var personId = Guid.NewGuid();
			Data.HasPerson(personId, "pierre", "2016-01-14");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().PersonId
				.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWithTimestamp()
		{
			Now.Is("2016-01-15 00:00");
			Data.HasPerson("pierre", "2016-01-14");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().Timestamp
				.Should().Be("2016-01-15 00:00".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenNotTerminated()
		{
			Now.Is("2016-01-15 00:00");
			Data.HasPerson("pierre");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishWhenLastDayToday()
		{
			Now.Is("2016-01-15 00:00");
			Data.HasPerson("pierre", "2016-01-15");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishForAllTerminatedPersons()
		{
			Now.Is("2016-01-15 00:00");
			var ashleeyId = Guid.NewGuid();
			var pierreId = Guid.NewGuid();
			Data.HasPerson(pierreId, "pierre", "2016-01-14", Guid.NewGuid());
			Data.HasPerson(ashleeyId, "ashleey", "2016-01-14", Guid.NewGuid());

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {ashleeyId, pierreId});
		}

		[Test]
		public void ShouldPublishWithEmptyTeamIdForTerminatedPerson()
		{
			Now.Is("2016-01-15 00:00");
			var teamId = Guid.NewGuid();
			Data.HasPerson("pierre", "2016-01-14", teamId);

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().TeamId
				.Should().Be(null);
		}

		[Test]
		public void ShouldNotPublishWhenTerminated2DaysAgo()
		{
			Now.Is("2016-01-15 00:00");
			Data.HasPerson("pierre", "2016-01-12");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenTerminadedInIstanbul()
		{
			Now.Is("2016-01-14 22:00");
			Data.HasPerson("pierre", "2016-01-14", TimeZoneInfoFactory.IstanbulTimeZoneInfo());

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Not.Be.Empty();
		}
		
	}

}