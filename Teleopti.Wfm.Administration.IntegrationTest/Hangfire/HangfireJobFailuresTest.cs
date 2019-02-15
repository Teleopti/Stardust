using System;
using System.Linq;
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
	public class HangfireJobFailuresTest: IExtendSystem
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobFailures;

			result.Count().Should().Be(1);
		}
		
		[Test]
		public void ShouldReadAllProperties()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobFailures;

			result.Single().Type.Should().Be("TestHandler got TestEvent");
			result.Single().Count.Should().Be(1);
		}
		
		[Test]
		public void ShouldReadCount()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildStatistics().JobFailures;

			result.Single().Count.Should().Be(2);
		}		
			
		public class TestEvent : IEvent
		{
		}

		public class TestHandler :
			IHandleEvent<TestEvent>,
			IRunOnHangfire
		{
			[Attempts(1)]
			public void Handle(TestEvent @event)
			{
				throw new Exception("Intended fail for test");
			}
		}
		
	}
}