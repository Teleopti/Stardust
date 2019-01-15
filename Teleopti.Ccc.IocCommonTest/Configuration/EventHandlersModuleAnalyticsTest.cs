using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventHandlersModuleAnalyticsTest
	{
		[Test]
		public void ShouldResolveAnalytics()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			var container = builder.Build();
			
			container.Resolve<IIntervalLengthFetcher>().Should().Not.Be.Null();
			container.Resolve<AnalyticsFactScheduleTimeMapper>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDateMapper>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactSchedulePersonMapper>().Should().Not.Be.Null();
			container.Resolve<AnalyticsFactScheduleMapper>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDayCountMapper>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}