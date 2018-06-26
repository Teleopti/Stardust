﻿using System.IO;
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
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[TestFixture]
	[WfmAdminTest]
	[Ignore("Ignore for now until we figure how to deal with json_value in query")]
	public class HangfirePerformanceStatisticsTest : IExtendSystem
	{
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public HangfireStatisticsViewModelBuilder Target;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		[Test]
		public void ShouldRead()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildPerformanceStatistics();

			result.Count().Should().Be(1);
		}
		
		[Test]
		public void ShouldReadWithProperties()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildPerformanceStatistics();

			result.Single().Type.Should().Contain("TestEvent");
			result.Single().Type.Should().Contain("TestHandler");
			result.Single().AverageTime.Should().Be.GreaterThan(1000);
			result.Single().Count.Should().Be(1);
			result.Single().MaxTime.Should().Be.GreaterThan(1000);
			result.Single().MinTime.Should().Be.GreaterThan(1000);
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
				Thread.Sleep(1000);
			}
		}
	}
}