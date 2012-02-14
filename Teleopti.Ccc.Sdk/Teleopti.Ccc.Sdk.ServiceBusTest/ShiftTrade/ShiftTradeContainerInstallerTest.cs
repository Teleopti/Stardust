using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.ShiftTrade;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.ShiftTrade
{
    [TestFixture]
    public class ShiftTradeContainerInstallerTest
    {
        private MockRepository _mocker;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();

            _unitOfWorkFactory = _mocker.DynamicMock<IUnitOfWorkFactory>();
        }

        [Test]
        public void ShouldResolveConsumerOfNewShiftTrade()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterType<ShiftTradeRequestSaga>().As<ConsumerOf<NewShiftTradeRequestCreated>>();

			builder.RegisterModule<ShiftTradeContainerInstaller>();
			builder.RegisterModule<RepositoryContainerInstaller>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<NewShiftTradeRequestCreated>>().Should().Not.Be.Null();
			}
        }

        [Test]
        public void ShouldResolveConsumerOfAcceptedShiftTrade()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterType<ShiftTradeRequestSaga>().As<ConsumerOf<AcceptShiftTrade>>();

			builder.RegisterModule<ShiftTradeContainerInstaller>();
			builder.RegisterModule<RepositoryContainerInstaller>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<AcceptShiftTrade>>().Should().Not.Be.Null();
			}
        }
    }
}
