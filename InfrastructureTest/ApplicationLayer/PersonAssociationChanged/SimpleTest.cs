using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[MultiDatabaseTest]
	public class SimpleTest : IIsolateSystem
	{
		public PersonAssociationChangedEventPublisher Target;
		public Database Database;
		public FakeEventPublisher Events;
		public INow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldNotPublishWithoutAgents()
		{
			Target.Handle(new TenantHourTickEvent());
			Events.Clear();

			Target.Handle(new TenantHourTickEvent());

			Events.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishForAnAgentAlinaHourly()
		{
			Target.Handle(new TenantHourTickEvent());
			Events.Clear();
			Database.WithAgent("alina");

			Target.Handle(new TenantHourTickEvent());

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishForAlinaOnTeamChange()
		{
			Database.WithAgent("alina");

			Target.Handle(new PersonTeamChangedEvent
			{
				Timestamp = Now.UtcDateTime(),
				PersonId = Database.CurrentPersonId(),
			});

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Should().Have.Count.EqualTo(1);
		}
	}
}