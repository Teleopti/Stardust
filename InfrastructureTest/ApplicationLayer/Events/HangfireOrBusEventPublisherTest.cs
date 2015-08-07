using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_HangfireEventProcessing_31237)]
	public class HangfireOrBusEventPublisherTest : ISetup
	{
		public FakeHangfireEventClient Hangfire;
		public FakeServiceBusSender Bus;
		public IEventPublisher Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeHangfireEventClient()).For<IHangfireEventClient>();
			system.UseTestDouble(new FakeServiceBusSender()).For<IServiceBusSender>();

			system.UseTestDouble(new HangfireEventHandler())
				.For<IHandleEvent<HangfireTestEvent>,
					IHandleEvent<PersonShiftStartEvent>,
					IHandleEvent<PersonShiftEndEvent>,
					IHandleEvent<PersonActivityStartEvent>,
					IHandleEvent<PersonStateChangedEvent>,
					IHandleEvent<PersonInAdherenceEvent>,
					IHandleEvent<PersonOutOfAdherenceEvent>>()
				;
		}

		[Test]
		public void ShouldPublishHangfireEventsToHangfire()
		{
			Target.Publish(new HangfireTestEvent());

			Hangfire.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldPublishNonHangfireEventsToTheBus()
		{
			var @event = new Event();

			Target.Publish(@event);

			Bus.SentMessages.Single().Should().Be.SameInstanceAs(@event);
		}

		[Test]
		public void ShouldPublishRtaEventsToHangfire()
		{
			Target.Publish(new PersonShiftStartEvent());
			Target.Publish(new PersonShiftEndEvent());
			Target.Publish(new PersonActivityStartEvent());
			Target.Publish(new PersonStateChangedEvent());
			Target.Publish(new PersonInAdherenceEvent());
			Target.Publish(new PersonOutOfAdherenceEvent());

			Hangfire.EventTypes.Should().Contain(typeof(PersonShiftStartEvent).AssemblyQualifiedName);
			Hangfire.EventTypes.Should().Contain(typeof(PersonShiftEndEvent).AssemblyQualifiedName);
			Hangfire.EventTypes.Should().Contain(typeof(PersonActivityStartEvent).AssemblyQualifiedName);
			Hangfire.EventTypes.Should().Contain(typeof(PersonStateChangedEvent).AssemblyQualifiedName);
			Hangfire.EventTypes.Should().Contain(typeof(PersonInAdherenceEvent).AssemblyQualifiedName);
			Hangfire.EventTypes.Should().Contain(typeof(PersonOutOfAdherenceEvent).AssemblyQualifiedName);
		}

		public class HangfireEventHandler :
			IHandleEvent<HangfireTestEvent>,
			IHandleEvent<PersonShiftStartEvent>,
			IHandleEvent<PersonShiftEndEvent>,
			IHandleEvent<PersonActivityStartEvent>,
			IHandleEvent<PersonStateChangedEvent>,
			IHandleEvent<PersonInAdherenceEvent>,
			IHandleEvent<PersonOutOfAdherenceEvent>
		{
			public void Handle(HangfireTestEvent @event)
			{
			}

			public void Handle(PersonShiftStartEvent @event)
			{
			}

			public void Handle(PersonShiftEndEvent @event)
			{
			}

			public void Handle(PersonActivityStartEvent @event)
			{
			}

			public void Handle(PersonStateChangedEvent @event)
			{
			}

			public void Handle(PersonInAdherenceEvent @event)
			{
			}

			public void Handle(PersonOutOfAdherenceEvent @event)
			{
			}
		}
	}
}