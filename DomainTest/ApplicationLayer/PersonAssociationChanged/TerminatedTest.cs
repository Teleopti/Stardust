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
	[DomainTest]
	[AddDatasourceId]
	public class TerminatedTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test]
		public void ShouldPublishWhenTerminated()
		{
			Data.WithAgent("pierre", "2016-01-14");
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());

			Now.Is("2016-01-15 00:00");
			Publisher.Clear();
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}

		[Test]
		public void ShouldPublishWithPersonId()
		{
			var personId = Guid.NewGuid();
			Data.WithAgent(personId, "pierre", "2016-01-14");
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-15 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().PersonId
				.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWithTimestamp()
		{
			Data.WithAgent("pierre", "2016-01-14");
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-15 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().Timestamp
				.Should().Be("2016-01-15 00:00".Utc());
		}
		
		[Test]
		public void ShouldNotPublishWhenLastDayToday()
		{
			Data.WithAgent("pierre", "2016-01-15");
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());

			Now.Is("2016-01-15 00:00");
			Publisher.Clear();
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishForAllTerminatedPersons()
		{
			var ashleeyId = Guid.NewGuid();
			var pierreId = Guid.NewGuid();
			Data.WithAgent(pierreId, "pierre", "2016-01-14", Guid.NewGuid());
			Data.WithAgent(ashleeyId, "ashleey", "2016-01-14", Guid.NewGuid());
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-15 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {ashleeyId, pierreId});
		}

		[Test]
		public void ShouldPublishWithEmptyTeamIdForTerminatedPerson()
		{
			var teamId = Guid.NewGuid();
			Data.WithAgent("pierre", "2016-01-14", teamId);
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-15 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single().TeamId
				.Should().Be(null);
		}

		[Test]
		public void ShouldPublishWhenTerminated2DaysAgo()
		{
			Data.WithAgent("pierre", "2016-01-12");
			Now.Is("2016-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());
			Publisher.Clear();

			Now.Is("2016-01-15 00:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Should().Have.Count.EqualTo(1);
		}

	}

}