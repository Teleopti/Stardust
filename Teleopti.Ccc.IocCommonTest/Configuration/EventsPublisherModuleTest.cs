using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class EventsPublisherModuleTest
	{
		[Test]
		public void ShouldResolveLocalEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<LocalEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<EventPublisher>();
		}

		[Test]
		public void ShouldResolveServiceBusEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<IServiceBusSender>()).As<IServiceBusSender>();
			containerBuilder.RegisterModule<ServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPublisher>().Should().Be.OfType<ServiceBusEventPublisher>();
		}

		[Test]
		public void ShouldResolveDenormalizationQueueEventsPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(MockRepository.GenerateMock<ISendDenormalizeNotification>()).As<ISendDenormalizeNotification>();
			containerBuilder.RegisterModule<UnitOfWorkModule>();
			containerBuilder.RegisterModule<DenormalizationQueueEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Be.OfType<DenormalizationQueueEventsPublisher>();
		}
	}
}
