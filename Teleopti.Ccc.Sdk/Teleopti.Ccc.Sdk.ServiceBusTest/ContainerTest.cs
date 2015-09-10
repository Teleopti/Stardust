using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.MessageModules;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture]
    public class ContainerTest
    {
        [Test]
        public void ShouldResolveNewAbsenceRequestConsumer()
        {

			var builder = new ContainerBuilder();
			builder.RegisterType<NewAbsenceRequestConsumer>().As<ConsumerOf<NewAbsenceRequestCreated>>();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule(SchedulePersistModule.ForOtherModules());
			builder.RegisterModule<IntraIntervalSolverServiceModule>();
		
			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<NewAbsenceRequestCreated>>().Should().Not.Be.Null();
			}
        }

		[Test]
		public void ShouldResolveMessageModule()
		{
			var appData = MockRepository.GenerateMock<IApplicationData>();
			appData.Expect(c => c.LoadPasswordPolicyService).Return(MockRepository.GenerateMock<ILoadPasswordPolicyService>());
			
			var builder = new ContainerBuilder();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterType<ApplicationLogOnMessageModule>().As<IMessageModule>().Named<IMessageModule>("1");

			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterInstance(appData);
			builder.RegisterModule<AuthenticationContainerInstaller>();
			builder.RegisterModule<AuthorizationContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IMessageModule>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveInitializeApplication()
		{
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container, new FalseToggleManager()).Configure(null);
				container.Resolve<IInitializeApplication>().Should().Not.Be.Null();
			}
		}
	}
}
