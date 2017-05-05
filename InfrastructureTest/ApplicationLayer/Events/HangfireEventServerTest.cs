using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
#pragma warning disable 618

	[TestFixture]
	[InfrastructureTest]
	public class HangfireEventServerTest : ISetup
	{
		public AHandler Handler;
		public AnotherHandler Another;
		public AspectedHandler Aspected;
		public HangfireEventServer Target;
		public FakeDataSourceForTenant DataSources;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();

			system.AddService<AHandler>();
			system.AddService<AnotherHandler>();
			system.AddService<AspectedHandler>();
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
			DataSources.Has(new FakeDataSource { DataSourceName = "tenant" });

			Target.Process(null, "tenant", typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledOnDataSource.Should().Be("tenant");
		}

		public class AnEvent : Event
		{
			public string Data { get; set; }
		}

		public class AHandler : IHandleEvent<AnEvent>, IRunOnHangfire
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

		public class AnotherHandler : IHandleEvent<AnEvent>, IRunOnHangfire
		{
			public List<IEvent> HandledEvents = new List<IEvent>();

			public void Handle(AnEvent @event)
			{
				HandledEvents.Add(@event);
			}
		}

		public class AspectedHandler : IHandleEvent<AnEvent>, IRunOnHangfire
		{
			public List<IEvent> HandledEvents = new List<IEvent>();

			public void Handle(AnEvent @event)
			{
				HandledEvents.Add(@event);
			}
		}
		
	}
#pragma warning restore 618

}