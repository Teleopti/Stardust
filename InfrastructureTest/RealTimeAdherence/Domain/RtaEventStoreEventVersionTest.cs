using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[Toggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreEventVersionTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreTestReader Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldNotIncludeVersionWithEventType()
		{
			var @event = new PersonStateChangedEvent();
			var eventType = @event.GetType();
			Publisher.Publish(@event);

			var actual = WithUnitOfWork.Get(uow => Events.LoadAllEventTypes());

			actual.Single().Should().Be($"{eventType.FullName}, {eventType.Assembly.GetName().Name}");
		}
	}
}