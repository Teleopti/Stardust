using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	[IoCTest]
	public class HangfireEventPublisherTest : IRegisterInContainer
	{
		public FakeHangfireEventClient JobClient;
		public IHangfireEventPublisher Target;
		public IJsonSerializer Serializer;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterInstance(new FakeHangfireEventClient()).AsSelf().As<IHangfireEventClient>().SingleInstance();

			builder.RegisterInstance(new TestHandler())
				.As<IHandleEvent<TestEvent>>()
				.As<IHandleEvent<Event>>()
				;
			builder.RegisterInstance(new TestMultiHandler1())
				.As<IHandleEvent<MultiHandlerTestEvent>>()
				;
			builder.RegisterInstance(new TestMultiHandler2())
				.As<IHandleEvent<MultiHandlerTestEvent>>()
				;
			builder.RegisterType<TestAspectedHandler>()
				.As<IHandleEvent<AspectedHandlerTestEvent>>()
				.SingleInstance()
				.ApplyAspects()
				;
		}

		[Test]
		public void ShouldEnqueue()
		{
			Target.Publish(new Event());

			JobClient.WasEnqueued.Should().Be.True();
		}

		[Test]
		public void ShouldSerializeTheEvent()
		{
			Target.Publish(new Event());

			JobClient.SerializedEvent.Should().Be.EqualTo(Serializer.SerializeObject(new Event()));
		}

		[Test]
		public void ShouldPassEventType()
		{
			Target.Publish(new Event());

			JobClient.EventType.Should().Be.EqualTo(typeof(Event).AssemblyQualifiedName);
		}

		[Test]
		public void ShouldPassEventTypeInDisplayName()
		{
			Target.Publish(new Event());

			JobClient.DisplayName.Should().Contain(typeof(Event).Name);
		}

		[Test]
		public void ShouldPassHandlerTypeInDisplayName()
		{
			Target.Publish(new Event());

			JobClient.DisplayName.Should().Contain(typeof(TestHandler).Name);
		}

		[Test]
		public void ShouldPassHandlerType()
		{
			Target.Publish(new TestEvent());

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
		public void ShouldEnqueueForEachHandler()
		{
			Target.Publish(new MultiHandlerTestEvent());

			JobClient.HandlerTypes.Should()
				.Have.SameValuesAs(new[] { typeof(TestMultiHandler1).AssemblyQualifiedName, typeof(TestMultiHandler2).AssemblyQualifiedName });
		}

		public class TestEvent : IEvent
		{
		}

		public class UnknownTestEvent : IEvent
		{
		}

		public class TestHandler : IHandleEvent<TestEvent>, IHandleEvent<Event>
		{
			public void Handle(TestEvent @event)
			{
			}

			public void Handle(Event @event)
			{
			}
		}

		public class MultiHandlerTestEvent : IEvent
		{
		}

		public class TestMultiHandler1 : IHandleEvent<MultiHandlerTestEvent>
		{
			public void Handle(MultiHandlerTestEvent @event)
			{
			}
		}

		public class TestMultiHandler2 : IHandleEvent<MultiHandlerTestEvent>
		{
			public void Handle(MultiHandlerTestEvent @event)
			{
			}
		}

		public class AspectedHandlerTestEvent : IEvent
		{
			
		}

		public class TestAspectedHandler : IHandleEvent<AspectedHandlerTestEvent>
		{
			public void Handle(AspectedHandlerTestEvent @event)
			{
			}
		}
	}
}
