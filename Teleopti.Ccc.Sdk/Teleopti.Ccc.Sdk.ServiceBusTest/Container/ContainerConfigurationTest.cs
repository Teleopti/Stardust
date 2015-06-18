﻿using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
{
	public class ContainerConfigurationTest
	{
		[Test]
		public void ShouldResolveServiceBusLocalEventPublisher()
		{
			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(CommonModule.ForTest());
			containerBuilder.RegisterModule<LocalServiceBusEventsPublisherModule>();
			var container = containerBuilder.Build();
			container.Resolve<IEventsPublisher>().Should().Not.Be.Null();
			container.Resolve<IEventPopulatingPublisher>().Should().Be.OfType<EventPopulatingPublisher>();
		}

		[Test]
		public void ShouldResolveEventsConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<IEvent>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveExportMultisiteSkillsToSkillConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<ExportMultisiteSkillsToSkill>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveExportMultisiteSkillToSkillConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<ExportMultisiteSkillToSkill>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveImportForecastsFileToSkillConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<ImportForecastsFileToSkill>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveImportForecastsToSkillConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<ImportForecastsToSkill>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolvePayrollExportConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<ConsumerOf<RunPayrollExport>>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveNotificationValidationCheck()
		{
			var builder = new ContainerBuilder();
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<INotificationValidationCheck>().Should().Be.OfType<NotificationValidationCheck>();
			}
		}

		[Test]
		public void ShouldResolveDatabaseConfigurationReader()
		{
			var builder = new ContainerBuilder();
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, new TrueToggleManager()).Configure();
				container.Resolve<IReadDataSourceConfiguration>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveEmailConfiguration()
		{
			var builder = new ContainerBuilder();
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
				container.Resolve<IEmailConfiguration>().Should().Be.OfType<EmailConfiguration>();
			}
		}

		private static void fakeInternalBusRegistrations(ContainerBuilder builder)
		{
			builder.Register(x => MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
			builder.RegisterType<EventsConsumer>().As<ConsumerOf<IEvent>>();
			builder.RegisterType<ExportMultisiteSkillsToSkillConsumer>().As<ConsumerOf<ExportMultisiteSkillsToSkill>>();
			builder.RegisterType<ExportMultisiteSkillToSkillConsumer>().As<ConsumerOf<ExportMultisiteSkillToSkill>>();
			builder.RegisterType<ImportForecastsFileToSkillConsumer>().As<ConsumerOf<ImportForecastsFileToSkill>>();
			builder.RegisterType<ImportForecastsToSkillConsumer>().As<ConsumerOf<ImportForecastsToSkill>>();
			builder.RegisterType<PayrollExportConsumer>().As<ConsumerOf<RunPayrollExport>>();
		}
	}
}