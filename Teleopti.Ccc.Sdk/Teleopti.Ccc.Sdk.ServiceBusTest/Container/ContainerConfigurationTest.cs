using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.Messages.General;
using Teleopti.Interfaces.Messages.Payroll;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
{
	public class ContainerConfigurationTest
	{
		[Test]
		public void ShouldResolveMessageSender()
		{
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container).Configure();
				container.Resolve<IMessageSender>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveMessageBrokerCompositeClient()
		{
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container).Configure();
				container.Resolve<IMessageBrokerComposite>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveSignalRSender()
		{
			using (var container = containerWithToggle(Toggles.Messaging_HttpSender_29205, false))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.SameInstanceAs(container.Resolve<SignalRSender>());
			}
		}

		[Test]
		public void ShouldResolveHttpSender()
		{
			using (var container = containerWithToggle(Toggles.Messaging_HttpSender_29205, true))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.SameInstanceAs(container.Resolve<HttpSender>());
			}
		}

		private static IContainer containerWithToggle(Toggles toggle, bool value)
		{
			var container = new ContainerBuilder().Build();
			new ContainerConfiguration(container).Configure();

			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			var builder = new ContainerBuilder();
			builder.Register(c => toggleManager).As<IToggleManager>();
			builder.Update(container);

			return container;
		}

		[Test]
		public void ShouldResolveEventsConsumer()
		{
			var builder = new ContainerBuilder();
			fakeInternalBusRegistrations(builder);
			using (var container = builder.Build())
			{
				new ContainerConfiguration(container).Configure();
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
				new ContainerConfiguration(container).Configure();
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
				new ContainerConfiguration(container).Configure();
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
				new ContainerConfiguration(container).Configure();
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
				new ContainerConfiguration(container).Configure();
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
				new ContainerConfiguration(container).Configure();
				container.Resolve<ConsumerOf<RunPayrollExport>>().Should().Not.Be.Null();
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