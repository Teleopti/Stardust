﻿using Autofac;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Logon.Aspects;
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
			builder.RegisterType<TenantScopeAspect>().SingleInstance();
			builder.RegisterType<TenantFromArguments>().SingleInstance();

			builder.RegisterType<AsSystemAspect>().SingleInstance();
			builder.RegisterType<AsSystem>().SingleInstance();

			builder.RegisterType<ImpersonateSystem>().SingleInstance();
			builder.RegisterType<ImpersonateSystemAspect>().SingleInstance();

			builder.RegisterType<LogOnOff>().As<ILogOnOff>().SingleInstance();

			builder.RegisterType<WindowsAppDomainPrincipalContext>().SingleInstance();
			builder.RegisterType<WebRequestPrincipalContext>().SingleInstance();
			builder.RegisterType<ThreadPrincipalContext>().As<IThreadPrincipalContext>().SingleInstance();
			builder.RegisterType<SelectivePrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();
			builder.RegisterType<CurrentProcess>().SingleInstance();
			builder.RegisterType<TokenIdentityProvider>().As<ITokenIdentityProvider>().SingleInstance();
			builder.RegisterType<TeleoptiPrincipalFactory>().As<IPrincipalFactory>().SingleInstance();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>().InstancePerDependency();
			builder.RegisterType<ApplicationFunctionsForRole>().As<ApplicationFunctionsForRole>().InstancePerDependency();
			builder.RegisterType<ClaimSetForApplicationRole>().As<ClaimSetForApplicationRole>().InstancePerDependency();

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
			builder.RegisterType<LicensedFunctionsProvider>().As<ILicensedFunctionsProvider>().SingleInstance();
			builder.RegisterType<ApplicationFunctionsProvider>().As<IApplicationFunctionsProvider>().SingleInstance();
			builder.RegisterType<ApplicationFunctionsToggleFilter>().As<IApplicationFunctionsToggleFilter>().SingleInstance();
			builder.RegisterType<DefinedRaptorApplicationFunctionFactory>().As<IDefinedRaptorApplicationFunctionFactory>().InstancePerDependency();
			builder.RegisterType<PermissionProvider>().As<IPermissionProvider>().SingleInstance();

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

			builder.RegisterType<CurrentTeleoptiPrincipal>().As<ICurrentTeleoptiPrincipal>().SingleInstance();
			builder.RegisterType<PrincipalAuthorization>().As<IAuthorization>().SingleInstance();
			builder.RegisterType<CurrentAuthorization>()
				.As<ICurrentAuthorization>()
				.As<IAuthorizationScope>()
				.SingleInstance();
			builder.RegisterType<UserCulture>().As<IUserCulture>().SingleInstance();
			builder.RegisterType<UserUiCulture>().As<IUserUiCulture>().SingleInstance();
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>().SingleInstance();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>().SingleInstance();
			
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<CurrentIdentity>().As<ICurrentIdentity>().SingleInstance();
			builder.RegisterType<UpdatedBy>()
				.As<IUpdatedBy>()
				.As<IUpdatedByScope>()
				.SingleInstance();
		}
	}
}
