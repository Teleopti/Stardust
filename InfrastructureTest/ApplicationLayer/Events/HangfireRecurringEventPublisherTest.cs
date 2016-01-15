using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
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
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	public class HangfireRecurringEventPublisherTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IRecurringEventPublisher Target;
		public IJsonSerializer Serializer;
		public FakeDataSourceForTenant DataSources;
		public IDataSourceScope DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();

			system.AddService<TestHandler>();
			system.AddService<TestMultiHandler1>();
			system.AddService<TestMultiHandler2>();
		}

		[Test]
		public void ShouldAddOrUpdate()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.HasRecurringJobs.Should().Be.True();
		}

		[Test]
		public void ShouldSerializeTheEvent()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringEvents.Single().Should().Be.EqualTo(Serializer.SerializeObject(new HangfireTestEvent()));
		}

		[Test]
		public void ShouldPassEventTypeShortName()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringEventTypes.Single().Should().Be.EqualTo(typeof(HangfireTestEvent).FullName + ", " + typeof(HangfireTestEvent).Assembly.GetName().Name);
		}

		[Test]
		[Setting("HangfireDashboardDisplayNames", true)]
		public void ShouldPassEventTypeInDisplayName()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringDisplayNames.Single().Should().Contain(typeof(HangfireTestEvent).Name);
		}

		[Test]
		[Setting("HangfireDashboardDisplayNames", true)]
		public void ShouldPassHandlerTypeInDisplayName()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringDisplayNames.Single().Should().Contain(typeof(TestHandler).Name);
		}

		[Test]
		public void ShouldNotPassDisplayNameByDefault()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringDisplayNames.Single().Should().Be.Null();
		}

		[Test]
		public void ShouldPassHandlerTypeShortName()
		{
			Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringHandlerTypes.Single().Should().Be(typeof(TestHandler).FullName + ", " + typeof(TestHandler).Assembly.GetName().Name);
		}
		
		[Test]
		public void ShouldNotAddIfNoHandler()
		{
			Target.PublishHourly("id", new UnknownTestEvent());

			JobClient.HasRecurringJobs.Should().Be.False();
		}

		[Test]
		public void ShouldAddForEachHandler()
		{
			Target.PublishHourly("id", new MultiHandlerTestEvent());

			JobClient.RecurringHandlerTypes.Should().Have.Count.EqualTo(2);
			JobClient.RecurringHandlerTypes.ElementAt(0).Should().Contain(typeof(TestMultiHandler2).FullName);
			JobClient.RecurringHandlerTypes.ElementAt(1).Should().Contain(typeof(TestMultiHandler1).FullName);
		}

		[Test]
		public void ShouldPassTenant()
		{
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			DataSources.Has(dataSource);

			using (DataSource.OnThisThreadUse(dataSource))
				Target.PublishHourly("id", new HangfireTestEvent());

			JobClient.RecurringTenants.Single().Should().Be(dataSource.DataSourceName);
		}

		[Test]
		public void ShouldReturnAllPublishingsById()
		{
			Target.PublishHourly("id", new MultiHandlerTestEvent());

			Target.RecurringPublishingIds().Single().Should().Be("id");
		}

		[Test]
		public void ShouldStopPublishing()
		{
			Target.PublishHourly("1", new HangfireTestEvent());
			Target.PublishHourly("2", new HangfireTestEvent());

			Target.StopPublishing("1");

			Target.RecurringPublishingIds().Single().Should().Be("2");
		}

		public class UnknownTestEvent : IEvent
		{
		}

		public class HangfireTestEvent : IEvent
		{
		}

		public class TestHandler :
			IRunOnHangfire,
			IHandleEvent<HangfireTestEvent>
		{
			public void Handle(HangfireTestEvent @event)
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

	}
}