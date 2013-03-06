using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.MessageModules;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [TestFixture]
    public class AbsenceRequestResolveTest
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
			builder.RegisterType<NewAbsenceRequestConsumer>().As<ConsumerOf<NewAbsenceRequestCreated>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<ConsumerOf<NewAbsenceRequestCreated>>().Should().Not.Be.Null();
			}
        }

		[Test]
		public void ShouldResolveMessageModule()
		{
			UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;
			var appData = MockRepository.GenerateMock<IApplicationData>();
			appData.Expect(c => c.LoadPasswordPolicyService).Return(MockRepository.GenerateMock<ILoadPasswordPolicyService>());


			var builder = new ContainerBuilder();
			builder.RegisterType<RaptorDomainMessageModule>().As<IMessageModule>().Named<IMessageModule>("1");

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ApplicationInfrastructureContainerInstaller>();
			builder.RegisterModule(new AuthenticationModule(appData));
			builder.RegisterModule<AuthenticationContainerInstaller>();
			builder.RegisterModule<DateAndTimeModule>();
			builder.RegisterModule<AuthorizationContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IMessageModule>().Should().Not.Be.Null();
			}
		}
    }
}
