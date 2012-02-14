using Autofac;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class AuthenticationContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PrincipalManager>().As<IPrincipalManager>();
			builder.RegisterType<LogOnOff>().As<ILogOnOff>();
			builder.RegisterType<AvailableDataSourcesProvider>().As<IAvailableDataSourcesProvider>().SingleInstance();
			builder.RegisterType<ApplicationDataSourceProvider>().As<IApplicationDataSourceProvider>().SingleInstance();
			builder.RegisterType<FindApplicationUser>().As<IFindApplicationUser>().SingleInstance();
			builder.RegisterType<FindUserDetail>().As<IFindUserDetail>().SingleInstance();
			builder.RegisterType<CheckNullUser>().As<ICheckNullUser>().SingleInstance();
			builder.RegisterType<CheckPassword>().As<ICheckPassword>().SingleInstance();
			builder.RegisterType<CheckSuperUser>().As<ICheckSuperUser>().SingleInstance();
			builder.RegisterType<CheckUserDetail>().As<ICheckUserDetail>().SingleInstance();
			builder.RegisterType<CheckBruteForce>().As<ICheckBruteForce>().SingleInstance();
			builder.RegisterType<CheckPasswordChange>().As<ICheckPasswordChange>().SingleInstance();
			builder.RegisterType<SystemUserSpecification>().As<ISystemUserSpecification>().SingleInstance();
			builder.RegisterType<SystemUserPasswordSpecification>().As<ISystemUserPasswordSpecification>().SingleInstance();
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<DummyPasswordPolicy>().As<IPasswordPolicy>().SingleInstance();
		}
    }
}