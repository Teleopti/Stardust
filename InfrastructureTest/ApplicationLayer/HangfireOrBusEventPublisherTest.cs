using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	[IoCTest]
	[Toggle(Toggles.RTA_HangfireEventProcessing_31237)]
	public class HangfireOrBusEventPublisherTest : IRegisterInContainer
	{
		public FakeHangfireEventClient hangfire;
		public FakeServiceBusSender bus;
		public IEventPublisher target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterInstance(new FakeHangfireEventClient()).AsSelf().As<IHangfireEventClient>();
			builder.RegisterInstance(new FakeServiceBusSender()).AsSelf().As<IServiceBusSender>();
			builder.RegisterInstance(new HangfireEventHandler()).AsSelf()
				.As<IHandleEvent<HangfireEvent>>()
				.As<IHandleEvent<PersonShiftStartEvent>>()
				.As<IHandleEvent<PersonShiftEndEvent>>()
				.As<IHandleEvent<PersonActivityStartEvent>>()
				.As<IHandleEvent<PersonStateChangedEvent>>()
				.As<IHandleEvent<PersonInAdherenceEvent>>()
				.As<IHandleEvent<PersonOutOfAdherenceEvent>>()
				;
		}

		[Test]
		public void ShouldPublishHangfireEventsToHangfire()
		{
			target.Publish(new HangfireEvent());

			hangfire.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldPublishNonHangfireEventsToTheBus()
		{
			var @event = new Event();

			target.Publish(@event);

			bus.SentMessages.Single().Should().Be(@event);
		}

		[Test]
		public void ShouldPublishRtaEventsToHangfire()
		{
			target.Publish(new PersonShiftStartEvent());
			target.Publish(new PersonShiftEndEvent());
			target.Publish(new PersonActivityStartEvent());
			target.Publish(new PersonStateChangedEvent());
			target.Publish(new PersonInAdherenceEvent());
			target.Publish(new PersonOutOfAdherenceEvent());

			hangfire.EventTypes.Should().Contain(typeof(PersonShiftStartEvent).AssemblyQualifiedName);
			hangfire.EventTypes.Should().Contain(typeof(PersonShiftEndEvent).AssemblyQualifiedName);
			hangfire.EventTypes.Should().Contain(typeof(PersonActivityStartEvent).AssemblyQualifiedName);
			hangfire.EventTypes.Should().Contain(typeof(PersonStateChangedEvent).AssemblyQualifiedName);
			hangfire.EventTypes.Should().Contain(typeof(PersonInAdherenceEvent).AssemblyQualifiedName);
			hangfire.EventTypes.Should().Contain(typeof(PersonOutOfAdherenceEvent).AssemblyQualifiedName);
		}

		public class HangfireEvent : IEvent, IGoToHangfire
		{
		}

		public class HangfireEventHandler :
			IHandleEvent<HangfireEvent>,
			IHandleEvent<PersonShiftStartEvent>,
			IHandleEvent<PersonShiftEndEvent>,
			IHandleEvent<PersonActivityStartEvent>,
			IHandleEvent<PersonStateChangedEvent>,
			IHandleEvent<PersonInAdherenceEvent>,
			IHandleEvent<PersonOutOfAdherenceEvent>
		{
			public void Handle(HangfireEvent @event)
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