using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
#pragma warning disable 618

	[TestFixture]
	[InfrastructureTest]
	public class HangfireEventServerTest : IIsolateSystem, IExtendSystem
	{
		public AHandler Handler;
		public AnotherHandler Another;
		public AspectedHandler Aspected;
		public HangfireEventServerForTest Target;
		public FakeDataSourceForTenant DataSources;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<HangfireEventServerForTest>();
			extend.AddService<AHandler>();
			extend.AddService<AnotherHandler>();
			extend.AddService<AspectedHandler>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
		}

		[Test]
		public void ShouldPublish()
		{
			Target.ProcessForTest(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldDeserialize()
		{
			Target.ProcessForTest(null, null, typeof(AnEvent).AssemblyQualifiedName, "{ Data: 'Hello' }", typeof(AHandler).AssemblyQualifiedName);

			Handler.HandledEvents.OfType<AnEvent>().Single().Data.Should().Be("Hello");
		}

		[Test]
		public void ShouldPublishToSpecifiedHandlerOnly()
		{
			Target.ProcessForTest(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AnotherHandler).AssemblyQualifiedName);

			Handler.HandledEvents.Should().Have.Count.EqualTo(0);
			Another.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldPublishToInterceptedHandler()
		{
			Target.ProcessForTest(null, null, typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AspectedHandler).AssemblyQualifiedName);

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

			Target.ProcessForTest(null, null, eventType, "{}", handlerType);

			Handler.HandledEvents.Single().Should().Be.OfType<AnEvent>();
		}

		[Test]
		public void ShouldSetCurrentDataSourceFromJob()
		{
			DataSources.Has(new FakeDataSource {DataSourceName = "tenant"});

			Target.ProcessForTest(null, "tenant", typeof(AnEvent).AssemblyQualifiedName, "{}", typeof(AHandler).AssemblyQualifiedName);

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