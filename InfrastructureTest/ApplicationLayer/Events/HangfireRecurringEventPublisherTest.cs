using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
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
	public class HangfireRecurringEventPublisherTest : ISetup
	{
		public FakeHangfireEventClient JobClient;
		public IRecurringEventPublisher Target;
		public IJsonSerializer Serializer;
		public FakeDataSourceForTenant DataSources;
		public IDataSourceScope DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();

			system.AddService<TestHandler>();
			system.AddService<TestMultiHandler1>();
			system.AddService<TestMultiHandler2>();
			system.AddService<TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId>();
			system.AddService<TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId2>();
		}
		
		[Test]
		public void ShouldAddOrUpdateHourly()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.Recurring.Single().Hourly.Should().Be.True();
		}

		[Test]
		public void ShouldAddOrUpdateMinutely()
		{
			Target.PublishMinutely(new HangfireTestEvent());

			JobClient.Recurring.Single().Minutely.Should().Be.True();
		}

		[Test]
		public void ShouldSerializeTheEvent()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringEvents.Single().Should().Be.EqualTo(Serializer.SerializeObject(new HangfireTestEvent()));
		}

		[Test]
		public void ShouldPassEventTypeShortName()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringEventTypes.Single().Should().Be.EqualTo(typeof(HangfireTestEvent).FullName + ", " + typeof(HangfireTestEvent).Assembly.GetName().Name);
		}

		[Test]
		public void ShouldPassEventTypeInDisplayName()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringDisplayNames.Single().Should().Contain(typeof(HangfireTestEvent).Name);
		}

		[Test]
		public void ShouldPassHandlerTypeInDisplayName()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringDisplayNames.Single().Should().Contain(typeof(TestHandler).Name);
		}

		[Test]
		public void ShouldPassHandlerTypeShortName()
		{
			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringHandlerTypes.Single().Should().Be(typeof(TestHandler).FullName + ", " + typeof(TestHandler).Assembly.GetName().Name);
		}

		[Test]
		public void ShouldNotAddIfNoHandler()
		{
			Target.PublishHourly(new UnknownTestEvent());

			JobClient.HasRecurringJobs.Should().Be.False();
		}

		[Test]
		public void ShouldAddForEachHandler()
		{
			Target.PublishHourly(new MultiHandlerTestEvent());

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
				Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringTenants.Single().Should().Be(dataSource.DataSourceName);
		}

		[Test]
		public void ShouldUpdateExistingJob()
		{
			Target.PublishHourly(new HangfireTestEvent());

			Target.PublishHourly(new HangfireTestEvent());

			JobClient.RecurringIds.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldStopPublishingAll()
		{
			Target.PublishHourly(new HangfireTestEvent());

			Target.StopPublishingAll();

			JobClient.HasRecurringJobs.Should().Be.False();
		}

		[Test]
		public void ShouldStopPublishingExceptSpecificTenant()
		{
			var dataSource1 = new FakeDataSource { DataSourceName = RandomName.Make() };
			var dataSource2 = new FakeDataSource { DataSourceName = RandomName.Make() };
			DataSources.Has(dataSource1);
			DataSources.Has(dataSource2);
			using (DataSource.OnThisThreadUse(dataSource1))
				Target.PublishHourly(new HangfireTestEvent());
			using (DataSource.OnThisThreadUse(dataSource2))
				Target.PublishHourly(new HangfireTestEvent());

			Target.StopPublishingForTenantsExcept(new[] { dataSource1.DataSourceName });

			JobClient.RecurringIds.Should().Have.Count.EqualTo(1);
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

		public class LongNameHandlerTestEvent : IEvent
		{
		}

		public class TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId :
			IRunOnHangfire,
			IHandleEvent<LongNameHandlerTestEvent>
		{
			public void Handle(LongNameHandlerTestEvent @event)
			{
			}
		}

		public class TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId2 :
			IRunOnHangfire,
			IHandleEvent<LongNameHandlerTestEvent>
		{
			public void Handle(LongNameHandlerTestEvent @event)
			{
			}
		}

	}
}