using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
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
				private ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();

						_unitOfWorkFactory = _mocker.DynamicMock<ICurrentUnitOfWorkFactory>();
        }

        [Test]
        public void ShouldResolveConsumerOfNewShiftTrade()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterType<ShiftTradeRequestSaga>().As<ConsumerOf<NewShiftTradeRequestCreated>>();

			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterModule<ShiftTradeModule>();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
					builder.RegisterModule(SchedulePersistModule.ForOtherModules());

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

			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterModule<ShiftTradeModule>();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule(SchedulePersistModule.ForOtherModules());

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<AcceptShiftTrade>>().Should().Not.Be.Null();
			}
        }
    }
}
