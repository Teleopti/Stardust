using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.Hangfire;

namespace Teleopti.Wfm.Administration.IntegrationTest.Hangfire
{
	[TestFixture]
	[WfmAdminTest]
	public class HangfireJobPerformanceTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public HangfireStatisticsViewModelBuilder Target;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		[Test]
		public void ShouldRead()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobPerformance;

			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobPerformance;

			result.Single().Type.Should().Be("HangfireJobPerformanceTest+TestHandler got TestEvent");
			result.Single().Count.Should().Be(1);
			result.Single().TotalTime.Should().Be.GreaterThan(100);
			result.Single().AverageTime.Should().Be.GreaterThan(100);
			result.Single().MaxTime.Should().Be.GreaterThan(100);
			result.Single().MinTime.Should().Be.GreaterThan(100);
		}

		[Test]
		public void ShouldReadCount()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobPerformance;

			result.Single().Count.Should().Be(2);
		}
		
		[Test]
		public void ShouldReadTotalTime()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobPerformance;

			result.Single().TotalTime.Should().Be.GreaterThan(200);
		}

		public class TestEvent : IEvent
		{
		}

		public class TestHandler :
			IHandleEvent<TestEvent>,
			IRunOnHangfire
		{
			public void Handle(TestEvent @event)
			{
				Thread.Sleep(100);
			}
		}
	}
}