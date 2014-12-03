using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventHandlersModuleTest
	{

		[Test]
		public void ShouldRegisterAnalytics()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<CommonModule>();
			var container = containerBuilder.Build();
			
			container.Resolve<IIntervalLengthFetcher>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleTimeHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDateHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactSchedulePersonHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}