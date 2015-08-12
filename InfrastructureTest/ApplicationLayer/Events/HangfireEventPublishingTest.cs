using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_HangfireEventProcessing_31237)]
	public class HangfireEventPublishingTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IEventPublisher Target;
		public IJsonSerializer Serializer;
		public IJsonDeserializer Deserializer;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();

			system.AddService<TestHandler>();
			system.AddService<TestMultiHandler1>();
			system.AddService<TestMultiHandler2>();
			system.AddService<TestAspectedHandler>();
			system.AddService<TestBothBusHandler>();
			system.AddService<TestBothHangfireHandler>();
		}

		[Test]
		public void ShouldEnqueue()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldSerializeTheEvent()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.SerializedEvent.Should().Be.EqualTo(Serializer.SerializeObject(new HangfireTestEvent()));
		}

		[Test]
		public void ShouldPassEventType()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.EventType.Should().Be.EqualTo(typeof(HangfireTestEvent).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldPassEventTypeInDisplayName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.DisplayName.Should().Contain(typeof(HangfireTestEvent).Name);
		}

		[Test]
		public void ShouldPassHandlerTypeInDisplayName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.DisplayName.Should().Contain(typeof(TestHandler).Name);
		}

		[Test]
		public void ShouldPassHandlerType()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.HandlerType.Should().Be(typeof(TestHandler).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotPassInterceptedHandlerTypeAsProxy()
		{
			Target.Publish(new AspectedHandlerTestEvent());

			JobClient.HandlerType.Should().Be(typeof(TestAspectedHandler).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldNotEnqueueIfNoHandler()
		{
			Target.Publish(new UnknownTestEvent());

			JobClient.WasEnqueued.Should().Be.False();
		}

		[Test]
		public void ShouldNotEnqueueToBusHandler()
		{
			Target.Publish(new BothTestEvent());

			JobClient.HandlerTypes.Should()
				.Have.SameValuesAs(new[] { typeof(TestBothHangfireHandler).AssemblyQualifiedName });
		}

		[Test]
		public void ShouldEnqueueForEachHandler()
		{
			Target.Publish(new MultiHandlerTestEvent());

			JobClient.HandlerTypes.Should()
				.Have.SameValuesAs(new[] { typeof(TestMultiHandler1).AssemblyQualifiedName, typeof(TestMultiHandler2).AssemblyQualifiedName });
		}

		public class UnknownTestEvent : IEvent
		{
		}

		public class HangfireTestEvent : IEvent
		{
		}

		public class TestHandler : 
			IRunOnHangfire, 
			IHandleEvent<HangfireTestEvent>, 
			IHandleEvent<Event>
		{
			public void Handle(HangfireTestEvent @event)
			{
			}

			public void Handle(Event @event)
			{
			}
		}

		public class MultiHandlerTestEvent : IEvent
		{
		}

		public class TestMultiHandler1 : 
			IRunOnHangfire, 
			IHandleEvent<MultiHandlerTestEvent>
		{
			public void Handle(MultiHandlerTestEvent @event)
			{
			}
		}

		public class TestMultiHandler2 : 
			IRunOnHangfire, 
			IHandleEvent<MultiHandlerTestEvent>
		{
			public void Handle(MultiHandlerTestEvent @event)
			{
			}
		}

		public class AspectedHandlerTestEvent : IEvent
		{
			
		}

		public class TestAspectedHandler : 
			IRunOnHangfire, 
			IHandleEvent<AspectedHandlerTestEvent>
		{
			public void Handle(AspectedHandlerTestEvent @event)
			{
			}
		}

		public class BothTestEvent : IEvent
		{
		}

		public class TestBothBusHandler :
			IHandleEvent<BothTestEvent>
		{
			public void Handle(BothTestEvent @event)
			{
			}
		}

		public class TestBothHangfireHandler :
			IRunOnHangfire,
			IHandleEvent<BothTestEvent>
		{
			public void Handle(BothTestEvent @event)
			{
			}
		}

	}
}
