using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	public class HangfireEventProcessorTest : ISetup
	{
		public AHandler Handler;
		public AnotherHandler Another;
		public AspectedHandler Aspected;
		public IHangfireEventProcessor Target;
		public FakeApplicationDataWithTestDatasource ApplicationData;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeApplicationDataWithTestDatasource>().For<IApplicationData, IDataSourceForTenant>();

			system.AddService<AHandler>();
			system.AddService<AnotherHandler>();
			system.AddService<AspectedHandler>();
			system.AddService<NonConcurrenctSafeHandler>();
		}

		[Test]
		public void ShouldPublish()
		{
			Target.Process(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldDeserialize()
		{
			Target.Process(null, null, typeof(AnEvent).AssemblyQualifiedName, "{ Data: 'Hello' }", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.OfType<AnEvent>().Single().Data.Should().Be("Hello");
		}

		[Test]
		public void ShouldPublishToSpecifiedHandlerOnly()
		{
			Target.Process(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AnotherHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Should().Have.Count.EqualTo(0);
			Another.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldPublishToInterceptedHandler()
		{
			Target.Process(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AspectedHandler).AssemblyQualifiedName);

			Aspected.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldNotPublishToSameHandlerInParallel()
		{
			Action job = () =>
			{
				10.Times(() =>
				{
					Target.Process(null, null, typeof (AnEvent).AssemblyQualifiedName, "{}", typeof (NonConcurrenctSafeHandler).AssemblyQualifiedName);
				});
			};
			var worker1 = Task.Factory.StartNew(job);
			var worker2 = Task.Factory.StartNew(job);

			Exceptions.Ignore(async () => await Task.WhenAll(worker1, worker2));

			worker1.Exception.Should().Be.Null();
			worker2.Exception.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptTypeNameWithoutVersionCultureAndPublicKeyToken()
		{
			var handlerType = typeof(AHandler).FullName + ", " + typeof(AHandler).Assembly.GetName().Name;
			var eventType = typeof(AnEvent).FullName + ", " + typeof(AnEvent).Assembly.GetName().Name;
			// I know... testing the test...
			handlerType.Should().Not.Contain("Version");
			handlerType.Should().Not.Contain("Culture");
			handlerType.Should().Not.Contain("PublicKeyToken");

			Target.Process(null, null, eventType, "{}", handlerType);

			Handler.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldSetCurrentDataSourceFromJob()
		{
			ApplicationData.RegisteredDataSources = new[] {new FakeDataSource {DataSourceName = "tenant" } };

			Target.Process(null, "tenant", typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledOnDataSource.Should().Be("tenant");
		}

		public class AnEvent : Event
		{
			public string Data { get; set; }
		}

		public class AHandler : IHandleEvent<AnEvent>
		{
			private readonly ICurrentDataSource _dataSource;
			public List<IEvent> HandledEvents = new List<IEvent>();
			public string HandledOnDataSource;

			public AHandler(ICurrentDataSource dataSource)
			{
				_dataSource = dataSource;
			}

			public void Handle(AnEvent @event)
			{
				HandledOnDataSource = _dataSource.Current().DataSourceName;
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

		public class AspectedHandler : IHandleEvent<AnEvent>
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