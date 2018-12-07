using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events.Hangfire
{
	[TestFixture]
	[InfrastructureTest]
	public class HangfireRecurringEventPublisherTest : IIsolateSystem, IExtendSystem
	{
		public FakeHangfireEventClient JobClient;
		public IRecurringEventPublisher Target;
		public IJsonSerializer Serializer;
		public FakeDataSourceForTenant DataSources;
		public IDataSourceScope DataSource;
		public HandlerTypeMapper TypeMapper;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
			extend.AddService<TestMultiHandler1>();
			extend.AddService<TestMultiHandler2>();
			extend.AddService<TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId>();
			extend.AddService<TestLongNameHandlerVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryVeryLongWithLongId2>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDataSourceForTenant>().For<IDataSourceForTenant>();
		}

		[Test]
		public void ShouldAddOrUpdateDaily()
		{
			Target.PublishDaily(new HangfireTestEvent());

			JobClient.Recurring.Single().Daily.Should().Be.True();;
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

			var expected = TypeMapper.NameForPersistence(typeof(TestHandler));
			JobClient.RecurringHandlerTypeNames.Single().Should().Be(expected);
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

			JobClient.RecurringHandlerTypeNames.Should().Have.Count.EqualTo(2);
			var expected1 = TypeMapper.NameForPersistence(typeof(TestMultiHandler2));
			JobClient.RecurringHandlerTypeNames.ElementAt(0).Should().Contain(expected1);
			var expected2 = TypeMapper.NameForPersistence(typeof(TestMultiHandler1));
			JobClient.RecurringHandlerTypeNames.ElementAt(1).Should().Contain(expected2);
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

		[Test]
		public void ShouldAssignIdNoLongerThanMaxLength()
		{
			var maxLength = 100 - "recurring-job:".Length;

			Target.PublishHourly(new LongNameHandlerTestEvent());

			JobClient.RecurringIds.First().Length.Should().Be.LessThanOrEqualTo(maxLength);
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