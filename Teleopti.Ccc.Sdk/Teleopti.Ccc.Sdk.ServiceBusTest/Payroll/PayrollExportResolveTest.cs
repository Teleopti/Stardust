using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Payroll
{
    [TestFixture]
    public class PayrollExportResolveTest
    {
	    [Test]
        public void ShouldResolveNewAbsenceRequestConsumer()
        {
    		var unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
            UnitOfWorkFactoryContainer.Current = unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterType<PayrollExportConsumer>().As<ConsumerOf<RunPayrollExport>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<PayrollContainerInstaller>();

			var client = MockRepository.GenerateMock<ISignalRClient>();
			builder.Register(x => client).As<ISignalRClient>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<RunPayrollExport>>().Should().Not.Be.Null();
			}
        }
    }
}
