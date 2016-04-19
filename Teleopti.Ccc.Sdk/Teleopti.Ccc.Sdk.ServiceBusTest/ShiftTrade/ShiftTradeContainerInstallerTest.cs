using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus.Legacy;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.ShiftTrade
{
    [TestFixture]
    public class ShiftTradeContainerInstallerTest
    {
        [Test]
        public void ShouldResolveConsumerOfNewShiftTrade()
        {
			var builder = new ContainerBuilder();
			builder.RegisterType<LegacyMessageTransformer>().As<ConsumerOf<NewShiftTradeRequestCreated>>();

			builder.Register(x => MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()), null)));

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<NewShiftTradeRequestCreated>>().Should().Not.Be.Null();
			}
        }

        [Test]
        public void ShouldResolveConsumerOfAcceptedShiftTrade()
        {
			var builder = new ContainerBuilder();
			builder.RegisterType<LegacyMessageTransformer>().As<ConsumerOf<AcceptShiftTrade>>();
			builder.Register(x => MockRepository.GenerateMock<IServiceBus>()).As<IServiceBus>();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()), null)));

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<AcceptShiftTrade>>().Should().Not.Be.Null();
			}
        }
    }
}
