using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	public class BlipTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WfmAdminModule2(configuration));
		}
	}

	[TestFixture]
	[BlipTest]
	public class HangfirePerformanceStatisticsTest : IExtendSystem
	{
		public HangfireClientStarter HangfireClientStarter;
		public HangfireUtilities Hangfire;
		public IEventPublisher Publisher;
		public HangfireStatisticsViewModelBuilder Target;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<TestHandler>();
		}

		[Test]
		public void ShouldGetSomething()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			HangfireClientStarter.Start();
			Publisher.Publish(new TestEvent());
			Hangfire.EmulateWorkerIteration();

			var result = Target.BuildPerformanceStatistics();

			result.Count().Should().Be(1);
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
			}
		}
	}
}