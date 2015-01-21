using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	[IoCTestAttribute]
	public class HangfireEventProcessorTest : IRegisterInContainer
	{
		public AHandler Handler;
		public AnotherHandler Another;
		public InterceptedHandler Intercepted;
		public IHangfireEventProcessor Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterInstance(new AHandler()).AsSelf().As<IHandleEvent<AnEvent>>().SingleInstance();
			builder.RegisterInstance(new AnotherHandler()).AsSelf().As<IHandleEvent<AnEvent>>().SingleInstance();
			builder.RegisterType<InterceptedHandler>().AsSelf().As<IHandleEvent<AnEvent>>().EnableClassInterceptors().SingleInstance();
		}

		[Test]
		public void ShouldPublish()
		{
			Target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldDeserialize()
		{
			Target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{ Data: 'Hello' }", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.OfType<AnEvent>().Single().Data.Should().Be("Hello");
		}

		[Test]
		public void ShouldPublishToSpecifiedHandlerOnly()
		{
			Target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AnotherHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Should().Have.Count.EqualTo(0);
			Another.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldPublishToInterceptedHandler()
		{
			Target.Process(null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(InterceptedHandler).AssemblyQualifiedName);

			Intercepted.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		public class AnEvent : Event
		{
			public string Data { get; set; }
		}

		public class AHandler : IHandleEvent<AnEvent>
		{
			public List<IEvent> HandledEvents = new List<IEvent>();

			public void Handle(AnEvent @event)
			{
				HandledEvents.Add(@event);
			}
		}

		public class AnotherHandler : IHandleEvent<AnEvent>
		{
			public List<IEvent> HandledEvents = new List<IEvent>();

			public void Handle(AnEvent @event)
			{
				HandledEvents.Add(@event);
			}
		}

		public class InterceptedHandler : IHandleEvent<AnEvent>
		{
			public List<IEvent> HandledEvents = new List<IEvent>();

			public void Handle(AnEvent @event)
			{
				HandledEvents.Add(@event);
			}
		}
	}
}