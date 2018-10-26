using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
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
	public class RtaEventStoreEventTypeIdTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreTester Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldStoreWithEventId()
		{
			var @event = new PersonStateChangedEvent();
			var eventType = @event.GetType();
			Publisher.Publish(@event);

			var actual = WithUnitOfWork.Get(uow => Events.LoadAllEventTypeIds());

			actual.Single().Should().Be(eventType.GetCustomAttribute<JsonObjectAttribute>().Id);
		}
	}
}