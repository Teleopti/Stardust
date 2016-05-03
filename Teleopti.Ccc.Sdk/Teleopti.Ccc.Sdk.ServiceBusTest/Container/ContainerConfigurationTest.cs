﻿//using Autofac;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Rhino.ServiceBus;
//using SharpTestsEx;
//using Teleopti.Ccc.Domain.ApplicationLayer;
//using Teleopti.Ccc.Domain.ApplicationLayer.Events;
//using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
//using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
//using Teleopti.Ccc.Domain.Notification;
//using Teleopti.Ccc.Infrastructure.ApplicationLayer;
//using Teleopti.Ccc.Infrastructure.Toggle;
//using Teleopti.Ccc.IocCommon;
//using Teleopti.Ccc.Sdk.ServiceBus;
//using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
//using Teleopti.Ccc.Sdk.ServiceBus.Container;
//using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
//using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
//using Teleopti.Interfaces.Domain;
//using Teleopti.Interfaces.Messages.General;
//using Teleopti.Interfaces.Messages.Payroll;

//namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
//{
//	public class ContainerConfigurationTest
//	{
//		[Test]
//		public void ShouldResolveEventsConsumer()
//		{
//			var builder = new ContainerBuilder();
//			fakeInternalBusRegistrations(builder);
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<ConsumerOf<IEvent>>().Should().Not.Be.Null();
//			}
//		}

//		[Test]
//		public void ShouldResolveExportMultisiteSkillsToSkillConsumer()
//		{
//			var builder = new ContainerBuilder();
//			fakeInternalBusRegistrations(builder);
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<ConsumerOf<ExportMultisiteSkillsToSkillEvent>>().Should().Not.Be.Null();
//			}
//		}

//		[Test]
//		public void ShouldResolveExportMultisiteSkillToSkillConsumer()
//		{
//			var builder = new ContainerBuilder();
//			fakeInternalBusRegistrations(builder);
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<ConsumerOf<ExportMultisiteSkillToSkill>>().Should().Not.Be.Null();
//			}
//		}




//		[Test]
//		public void ShouldResolvePayrollExportConsumer()
//		{
//			var builder = new ContainerBuilder();
//			fakeInternalBusRegistrations(builder);
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<ConsumerOf<RunPayrollExportEvent>>().Should().Not.Be.Null();
//			}
//		}

//		[Test]
//		public void ShouldResolveNotificationValidationCheck()
//		{
//			var builder = new ContainerBuilder();
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<INotificationValidationCheck>().Should().Be.OfType<NotificationValidationCheck>();
//			}
//		}

//		[Test]
//		public void ShouldResolveEmailConfiguration()
//		{
//			var builder = new ContainerBuilder();
//			using (var container = builder.Build())
//			{
//				new ContainerConfiguration(container, MockRepository.GenerateMock<IToggleManager>()).Configure();
//				container.Resolve<IEmailConfiguration>().Should().Be.OfType<EmailConfiguration>();
//			}
//		}

//		private static void fakeInternalBusRegistrations(ContainerBuilder builder)
//		{
//			builder.Register(x => MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
//			builder.RegisterType<EventsConsumer>().As<ConsumerOf<IEvent>>();
//			builder.RegisterType<ExportMultisiteSkillsToSkillHandler>().As<ConsumerOf<ExportMultisiteSkillsToSkillEvent>>();
//			builder.RegisterType<ExportMultisiteSkillProcessor>().As<ConsumerOf<ExportMultisiteSkillToSkill>>();
//			builder.RegisterType<PayrollExportHandler>().As<ConsumerOf<RunPayrollExportEvent>>();

//		}
//	}
//}