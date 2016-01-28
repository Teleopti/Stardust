using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	public class ServiceBusEventPublishingTest : ISetup
	{
		public FakeServiceBusSender Bus;
		public IEventPublisher Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeServiceBusSender>().For<IServiceBusSender>();

			system.AddService<EventHandler>();
		}

		[Test]
		public void ShouldSendToBus()
		{
			var @event = new Event();

			Target.Publish(@event);

			Bus.SentMessages.Single().Should().Be(@event);
		}

		public class EventHandler : IHandleEvent<Event>, IRunOnServiceBus
		{
			public void Handle(Event @event)
			{
			}
		}
	}
}