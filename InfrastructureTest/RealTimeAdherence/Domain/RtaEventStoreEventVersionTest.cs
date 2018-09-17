using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
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