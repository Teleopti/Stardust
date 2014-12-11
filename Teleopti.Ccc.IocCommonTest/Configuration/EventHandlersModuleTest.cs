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
		//[Test]
		//public void ShouldResolveAllEventHandlers()
		//{
		//	var handlers = (
		//		from type in typeof (IHandleEvent<>).Assembly.GetTypes()
		//		let handlerInterfaces =
		//			from i in type.GetInterfaces()
		//			let isHandlerInterface = i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IHandleEvent<>)
		//			where isHandlerInterface
		//			select i
		//		where handlerInterfaces.Any()
		//		select new
		//		{
		//			type,
		//			handlerInterfaces = handlerInterfaces.ToArray()
		//		}
		//		).ToArray();
		//	handlers.Should().Have.Count.GreaterThan(10);
		//	var builder = new ContainerBuilder();
		//	builder.RegisterModule(CommonModule.ForTest());
		//	var container = builder.Build();
		//	handlers.ForEach(x =>
		//	{
		//		container.Resolve(x.handlerInterfaces.First()).Should().Not.Be.Null();
		//	});
		//}

		[Test]
		public void ShouldRegisterAnalytics()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			var container = builder.Build();
			
			container.Resolve<IIntervalLengthFetcher>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleTimeHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactScheduleDateHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsFactSchedulePersonHandler>().Should().Not.Be.Null();
			container.Resolve<IAnalyticsScheduleRepository>().Should().Not.Be.Null();
			
		}
	}
}