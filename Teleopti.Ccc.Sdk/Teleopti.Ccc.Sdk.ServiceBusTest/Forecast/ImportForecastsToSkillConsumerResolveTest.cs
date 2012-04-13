using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
    [TestFixture]
    public class ImportForecastsToSkillConsumerResolveTest
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IServiceBus _serviceBus;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            _unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            _serviceBus = mocks.DynamicMock<IServiceBus>();
        }

        [Test]
        public void ShouldResolveImportForecastsToSkillConsumer()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_serviceBus).As<IServiceBus>();
            builder.RegisterType<ImportForecastsToSkillConsumer>().As<ConsumerOf<ImportForecastsToSkill>>();
            builder.RegisterType<SaveForecastToSkillCommand>().As<ISaveForecastToSkillCommand>();
            builder.RegisterType<OpenAndSplitSkillCommand>().As<IOpenAndSplitSkillCommand>();
            builder.RegisterModule<RepositoryContainerInstaller>();
            builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
            builder.RegisterModule<ForecastContainerInstaller>();
            
            using (var container = builder.Build())
            {
                container.Resolve<ConsumerOf<ImportForecastsToSkill>>().Should().Not.Be.Null();
            }
        }
    }
}
