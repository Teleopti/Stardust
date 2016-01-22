using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

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
			container.Resolve<IAnalyticsFactScheduleTimeHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDateHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactSchedulePersonHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDayCountHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}