using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[DomainTest]
	[AddDatasourceId]
	public class OldChangesTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;
		public IIoCTestContext Context;

		[Test]
		public void ShouldPublishAWeekOldChange()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Single(x => x.PersonId == personId)
				.TeamId
				.Should().Be(team);
		}
		
		[Test]
		public void ShouldNotPublishSameChangeTwice()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == personId)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishSameChangeTwiceAfterRestart()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Context.SimulateRestart();
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == personId)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishIfNewPeriodHasSameTeam()
		{
			var person = Guid.NewGuid();
			var team = Guid.NewGuid();
			Data.WithAgent(person, "pierre")
				.WithPeriod("2017-01-01", team)
				.WithPeriod("2017-03-17", team);
			Now.Is("2017-01-01 00:00");
			Target.Handle(new TenantHourTickEvent());

			Now.Is("2017-03-17 08:00");
 			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == person)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishForEachAgentInTeam()
		{
			var team = Guid.NewGuid();
			var ashleyId = Guid.NewGuid();
			var pierreId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data
				.WithAgent(ashleyId, "ashley")
				.WithPeriod("2017-03-10", team)
				.WithAgent(pierreId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Where(x => x.PersonId == ashleyId).Should().Have.Count.EqualTo(1);
			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Where(x => x.PersonId == pierreId).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishExternalLogonChange()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Data.WithExternalLogon("usercode");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().ExternalLogons.Single()
				.UserCode.Should().Be("usercode");
		}

		[Test]
		public void ShouldPublishExternalLogonDataSourceChange()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team)
				.WithDataSource(5, "5")
				.WithExternalLogon("usercode")
				.WithPeriod("2017-03-18", team)
				.WithDataSource(6, "6")
				.WithExternalLogon("usercode")
				;

			Target.Handle(new TenantHourTickEvent());
			Now.Is("2017-03-18 08:00");
			Target.Handle(new TenantHourTickEvent());

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().ExternalLogons.Single()
				.DataSourceId.Should().Be(6);
		}

		[Test]
		public void ShouldNotPublishSameChangeOfTeam()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == personId);
			Target.Handle(new PersonTeamChangedEvent
			{
				PersonId = personId,
				CurrentTeamId = @event.TeamId,
				ExternalLogons = @event.ExternalLogons
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == personId)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishOldChangeOfTeam()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Target.Handle(new PersonTeamChangedEvent {PersonId = personId});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().TeamId
				.Should().Be(team);
		}

		[Test]
		public void ShouldNotPublishSameChangeOfPeriod()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			var @event = Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == personId);
			Target.Handle(new PersonPeriodChangedEvent
			{
				PersonId = personId,
				CurrentTeamId = @event.TeamId,
				ExternalLogons = @event.ExternalLogons
			});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>().Where(x => x.PersonId == personId)
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishOldChangeOfPeriod()
		{
			var team = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Now.Is("2017-03-17 08:00");
			Data.WithAgent(personId, "pierre")
				.WithPeriod("2017-03-10", team);

			Target.Handle(new TenantHourTickEvent());
			Target.Handle(new PersonPeriodChangedEvent {PersonId = personId});

			Publisher.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Last().TeamId
				.Should().Be(team);
		}

	}
}