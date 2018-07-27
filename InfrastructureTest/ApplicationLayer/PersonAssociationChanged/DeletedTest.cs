using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[MultiDatabaseTest]
	public class DeletedTest : IIsolateSystem
	{
		public PersonAssociationChangedEventPublisher Target;
		public Database Database;
		public FakeEventPublisher Events;
		public IPersonRepository Persons;
		public WithUnitOfWork Uow;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldPublishForDeletedAgentHourly()
		{
			Database.WithAgent("alina");
			Target.Handle(new TenantHourTickEvent());
			var alina = Database.CurrentPersonId();
			Uow.Do(() => Persons.Remove(Persons.Get(alina)));
			Events.Clear();

			Target.Handle(new TenantHourTickEvent());

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == alina)
				.TeamId.Should().Be(null);
		}

		[Test]
		public void ShouldPublishForSoftDeletedAgent()
		{
			Database.WithAgent("alina");
			Target.Handle(new TenantHourTickEvent());
			var alina = Database.CurrentPersonId();
			Uow.Do(() => Persons.Remove(Persons.Get(alina)));
			Events.Clear();

			Target.Handle(new PersonDeletedEvent {PersonId = alina});

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == alina)
				.TeamId.Should().Be(null);
		}

		[Test]
		public void ShouldPublishForHardDeletedAgent()
		{
			Database.WithAgent("alina");
			Target.Handle(new TenantHourTickEvent());
			var alina = Database.CurrentPersonId();
			Uow.Do(() => Persons.HardRemove(Persons.Get(alina)));
			Events.Clear();

			Target.Handle(new PersonDeletedEvent {PersonId = alina});

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>().Single(x => x.PersonId == alina)
				.TeamId.Should().Be(null);
		}
	}
}