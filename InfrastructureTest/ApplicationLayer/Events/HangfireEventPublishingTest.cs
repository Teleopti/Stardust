using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class HangfireEventPublishingTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IEventPublisher Target;
		public IJsonSerializer Serializer;
		public IJsonDeserializer Deserializer;
		public FakeApplicationData ApplicationData;
		public IDataSourceScope DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeApplicationData>().For<IApplicationData>();

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
		public void ShouldPassEventTypeShortName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.EventType.Should().Be.EqualTo(typeof(HangfireTestEvent).FullName + ", " + typeof(HangfireTestEvent).Assembly.GetName().Name);
		}

		[Test]
		[Setting("HangfireDashboardDisplayNames", true)]
		public void ShouldPassEventTypeInDisplayName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.DisplayName.Should().Contain(typeof(HangfireTestEvent).Name);
		}

		[Test]
		[Setting("HangfireDashboardDisplayNames", true)]
		public void ShouldPassHandlerTypeInDisplayName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.DisplayName.Should().Contain(typeof(TestHandler).Name);
		}

		[Test]
		public void ShouldNotPassDisplayNameByDefault()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.DisplayName.Should().Be.Null();
		}

		[Test]
		public void ShouldPassHandlerTypeShortName()
		{
			Target.Publish(new HangfireTestEvent());

			JobClient.HandlerType.Should().Be(typeof(TestHandler).FullName + ", " + typeof(TestHandler).Assembly.GetName().Name);
		}

		[Test]
		public void ShouldNotPassInterceptedHandlerTypeAsProxy()
		{
			Target.Publish(new AspectedHandlerTestEvent());

			JobClient.HandlerType.Should().Be(typeof(TestAspectedHandler).FullName + ", " + typeof(TestAspectedHandler).Assembly.GetName().Name);
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

			JobClient.HandlerTypes.Should().Have.Count.EqualTo(1);
			JobClient.HandlerTypes.ElementAt(0).Should().Contain(typeof(TestBothHangfireHandler).FullName);
		}

		[Test]
		public void ShouldEnqueueForEachHandler()
		{
			Target.Publish(new MultiHandlerTestEvent());

			JobClient.HandlerTypes.Should().Have.Count.EqualTo(2);
			JobClient.HandlerTypes.ElementAt(0).Should().Contain(typeof (TestMultiHandler2).FullName);
			JobClient.HandlerTypes.ElementAt(1).Should().Contain(typeof (TestMultiHandler1).FullName);
		}

		[Test]
		public void ShouldPassTenant()
		{
			var dataSource = new FakeDataSource {DataSourceName = RandomName.Make()};
			ApplicationData.RegisteredDataSources = new[] {dataSource };

			using (DataSource.OnThisThreadUse(dataSource))
				Target.Publish(new HangfireTestEvent());

			JobClient.Tenant.Should().Be(dataSource.DataSourceName);
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
