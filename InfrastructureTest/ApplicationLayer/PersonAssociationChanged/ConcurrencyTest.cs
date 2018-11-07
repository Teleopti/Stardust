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
	public class ConcurrencyTest : IIsolateSystem
	{
		public PersonAssociationChangedEventPublisher Target;
		public Database Database;
		public FakeEventPublisher Events;
		public ConcurrencyRunner Run;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldNotPublishDuplicates()
		{
			Target.Handle(new TenantHourTickEvent());
			Events.Clear();
			10.Times(() => Database.WithAgent());
			10.Times(() => { Run.InParallel(() => { Target.Handle(new TenantHourTickEvent()); }); });
			Run.Wait();

			Events.PublishedEvents.OfType<PersonAssociationChangedEvent>()
				.Should().Have.Count.EqualTo(10);
		}
	}
}