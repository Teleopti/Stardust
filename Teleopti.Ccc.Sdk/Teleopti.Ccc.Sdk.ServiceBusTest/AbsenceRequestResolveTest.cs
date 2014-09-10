﻿using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.MessageModules;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
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
        [Test]
        public void ShouldResolveNewAbsenceRequestConsumer()
        {
			var builder = new ContainerBuilder();
			builder.RegisterType<NewAbsenceRequestConsumer>().As<ConsumerOf<NewAbsenceRequestCreated>>();

			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule<ForecastContainerInstaller>();
			builder.RegisterModule<RequestContainerInstaller>();
			builder.RegisterModule<SchedulingContainerInstaller>();
			builder.RegisterModule<EventHandlersModule>();
					builder.RegisterModule(SchedulePersistModule.ForOtherModules());

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
			builder.RegisterType<RaptorDomainMessageModule>().As<IMessageModule>().Named<IMessageModule>("1");

			builder.RegisterModule<GodModule>();
			builder.RegisterModule<RepositoryModule>();
			builder.RegisterModule<ServiceBusCommonModule>();
			builder.RegisterModule(new AuthenticationModule(appData));
			builder.RegisterModule<AuthenticationContainerInstaller>();
			builder.RegisterModule<AuthorizationContainerInstaller>();

			using (var container = builder.Build())
			{
				container.Resolve<IMessageModule>().Should().Not.Be.Null();
			}
		}
    }
}
