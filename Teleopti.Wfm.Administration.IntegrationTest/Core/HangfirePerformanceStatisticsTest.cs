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
	[TestFixture]
	[WfmAdminTest]
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
		public void ShouldGetSomething()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
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