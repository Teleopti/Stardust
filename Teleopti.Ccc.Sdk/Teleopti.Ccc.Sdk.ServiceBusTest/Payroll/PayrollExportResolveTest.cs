using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Payroll
{
    [TestFixture]
    public class PayrollExportResolveTest
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            _unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
        }

        [Test]
        public void ShouldResolveNewAbsenceRequestConsumer()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;

			var builder = new ContainerBuilder();
			builder.RegisterType<PayrollExportConsumer>().As<ConsumerOf<RunPayrollExport>>();

			builder.RegisterModule<RepositoryContainerInstaller>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<PayrollContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<RunPayrollExport>>().Should().Not.Be.Null();
			}
        }
    }
}
