using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
    [TestFixture]
    public class ImportForecastsToSkillConsumerResolveTest
    {
        [Test]
        public void ShouldResolveImportForecastsToSkillConsumer()
		{
			var mocks = new MockRepository();
			var unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			var serviceBus = mocks.DynamicMock<IServiceBus>();
            UnitOfWorkFactoryContainer.Current = unitOfWorkFactory;

            var builder = new ContainerBuilder();
            builder.RegisterInstance(serviceBus).As<IServiceBus>();
            builder.RegisterType<ImportForecastsToSkillConsumer>().As<ConsumerOf<ImportForecastsToSkill>>();
            builder.RegisterType<SaveForecastToSkillCommand>().As<ISaveForecastToSkillCommand>();
            builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
			builder.RegisterModule<RepositoryModule>();
            builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
            builder.RegisterModule<ForecastContainerInstaller>();

			var client = MockRepository.GenerateMock<ISignalRClient>();
			builder.Register(x => client).As<ISignalRClient>();

            using (var container = builder.Build())
            {
                container.Resolve<ConsumerOf<ImportForecastsToSkill>>().Should().Not.Be.Null();
            }
        }
    }
}
