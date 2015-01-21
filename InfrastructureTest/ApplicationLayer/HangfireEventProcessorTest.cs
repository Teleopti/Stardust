using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterInstance(new NonConcurrenctSafeHandler()).AsSelf().As<IHandleEvent<AnEvent>>().SingleInstance();

			builder.RegisterInstance(new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
				}
			}).As<IConfigReader>().AsSelf().SingleInstance();
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

		[Test]
		public void ShouldNotPublishToSameHandlerInParallel()
		{
			Action job = () =>
			{
				10.Times(() =>
				{
					Target.Process(null, typeof (AnEvent).AssemblyQualifiedName, "{}", typeof (NonConcurrenctSafeHandler).AssemblyQualifiedName);
				});
			};
			var worker1 = Task.Factory.StartNew(job);
			var worker2 = Task.Factory.StartNew(job);

			Exceptions.Ignore(() => Task.WaitAll(worker1, worker2));

			worker1.Exception.Should().Be.Null();
			worker2.Exception.Should().Be.Null();
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

		public class NonConcurrenctSafeHandler : IHandleEvent<AnEvent>
		{
			private bool _isHandling = false;

			public void Handle(AnEvent @event)
			{
				if (_isHandling)
					throw new Exception("Please dont execute me in parallel! >:-[");
				_isHandling = true;
				Thread.Sleep(10); // do some work
				_isHandling = false;
			}
		}

	}
}