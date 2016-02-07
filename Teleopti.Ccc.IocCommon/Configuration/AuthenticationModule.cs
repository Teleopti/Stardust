using Autofac;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AuthenticationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WindowsAppDomainPrincipalContext>().SingleInstance();
			builder.RegisterType<WebRequestPrincipalContext>().SingleInstance();
			builder.RegisterType<ThreadPrincipalContext>().SingleInstance();
			builder.RegisterType<SelectivePrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();

			builder.RegisterType<TokenIdentityProvider>().As<ITokenIdentityProvider>().SingleInstance();
			builder.RegisterType<TeleoptiPrincipalFactory>().As<IPrincipalFactory>().SingleInstance();
			builder.RegisterType<AsSuperUser>().SingleInstance();
			builder.RegisterType<LogOnOff>().As<ILogOnOff>().SingleInstance();
			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<AvailableBusinessUnitsProvider>().As<IAvailableBusinessUnitsProvider>().SingleInstance();

			builder.Register<IPasswordPolicy>(c =>
			{
				if (c.Resolve<IApplicationData>().LoadPasswordPolicyService == null)
					return new PasswordPolicyFake();
				return new PasswordPolicy(c.Resolve<ILoadPasswordPolicyService>());
			})
				.As<IPasswordPolicy>()
				.SingleInstance();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>().InstancePerDependency();
			builder.RegisterType<ApplicationFunctionsForRole>().As<ApplicationFunctionsForRole>().InstancePerDependency();
			builder.RegisterType<LicensedFunctionsProvider>().As<ILicensedFunctionsProvider>().SingleInstance();
			builder.RegisterType<ApplicationFunctionsProvider>().As<IApplicationFunctionsProvider>().SingleInstance();
			builder.RegisterType<ApplicationFunctionsToggleFilter>().As<IApplicationFunctionsToggleFilter>().SingleInstance();
			builder.RegisterType<ClaimSetForApplicationRole>().As<ClaimSetForApplicationRole>().InstancePerDependency();
			builder.RegisterType<DefinedRaptorApplicationFunctionFactory>().As<IDefinedRaptorApplicationFunctionFactory>().InstancePerDependency();

			builder.RegisterType<CurrentApplicationData>().As<ICurrentApplicationData>().SingleInstance();

			builder.Register(c => StateHolder.Instance.StateReader.ApplicationScopeData)
				.As<IApplicationData>().SingleInstance()
				.ExternallyOwned();

			builder.RegisterType<DataSourceForTenant>().As<IDataSourceForTenant>().SingleInstance();
			//by default, don't load tenats on demand. Will be changed client-by-client in the future
			builder.RegisterType<FindTenantByNameWithEnsuredTransactionFake>().As<IFindTenantByNameWithEnsuredTransaction>().SingleInstance();
			
			builder.Register(c =>
			{
				var passwordPolicyService = c.Resolve<IApplicationData>().LoadPasswordPolicyService;
				return passwordPolicyService ?? new ThrowingLoadPasswordPolicyService();
			}).As<ILoadPasswordPolicyService>().SingleInstance();

			builder.RegisterType<CurrentTeleoptiPrincipal>()
				.As<ICurrentTeleoptiPrincipal>()
				.SingleInstance();
			builder.RegisterType<PrincipalAuthorization>()
				.As<IPrincipalAuthorization>()
				.SingleInstance();
			builder.RegisterType<UserCulture>().As<IUserCulture>().SingleInstance();
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>().SingleInstance();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>().SingleInstance();
			
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<CurrentIdentity>().As<ICurrentIdentity>().SingleInstance();
		}
	}
}
