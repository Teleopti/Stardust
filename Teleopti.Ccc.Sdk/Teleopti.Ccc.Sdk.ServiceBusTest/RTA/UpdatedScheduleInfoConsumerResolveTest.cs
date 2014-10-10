﻿using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class UpdatedScheduleInfoConsumerResolveTest
	{
		[Test]
		public void ShouldResolvePersonScheduleDayReadModelHandlerConsumer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
			builder.RegisterModule<CommonModule>();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule<PayrollContainerInstaller>();
			builder.RegisterModule<LocalServiceBusPublisherModule>();

			builder.RegisterModule<EventHandlersModule>();

			using (var container = builder.Build())
			{
				container.Resolve<IHandleEvent<ScheduleProjectionReadOnlyChanged>>().Should().Not.Be.Null();
			}
		}
	}
}