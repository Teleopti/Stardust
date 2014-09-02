using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
	[TestFixture]
	public class ExportMultisiteSkillsToSkillResolveTest
	{
		[Test]
		public void ShouldResolveNewAbsenceRequestConsumer()
		{
			var unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var serviceBus = MockRepository.GenerateMock<IServiceBus>();
			UnitOfWorkFactoryContainer.Current = unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterInstance(serviceBus).As<IServiceBus>();
			builder.RegisterType<ExportMultisiteSkillsToSkillConsumer>().As<ConsumerOf<ExportMultisiteSkillsToSkill>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<ExportForecastContainerInstaller>();

			var client = MockRepository.GenerateMock<ISignalRClient>();
			builder.Register(x => client).As<ISignalRClient>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<ExportMultisiteSkillsToSkill>>().Should().Not.Be.Null();
			}
		}
	}
}
