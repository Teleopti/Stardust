using Autofac;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AuthenticationModule : Module
	{
		private readonly IocConfiguration _configuration;

		public AuthenticationModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TenantScopeAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<TenantFromArguments>().SingleInstance();

			builder.RegisterType<AsSystemAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<AsSystem>().SingleInstance();

			builder.RegisterType<FullPermissions>().SingleInstance();
			builder.RegisterType<FullPermissionsAspect>().As<IAspect>().SingleInstance();

			builder.RegisterType<ImpersonateSystem>().SingleInstance();
			builder.RegisterType<ImpersonateSystemAspect>().As<IAspect>().SingleInstance();

			builder.RegisterType<LogOnOff>().As<ILogOnOff>().SingleInstance();

			builder.RegisterType<AppDomainPrincipalContext>().SingleInstance();
			builder.RegisterType<WebRequestPrincipalContext>().SingleInstance();
			builder.RegisterType<ThreadPrincipalContext>().As<IThreadPrincipalContext>().SingleInstance();
			builder.RegisterType<SelectivePrincipalContext>().As<ICurrentPrincipalContext>().SingleInstance();
			builder.RegisterType<CurrentProcess>().SingleInstance();
			builder.RegisterType<TokenIdentityProvider>().As<ITokenIdentityProvider>().SingleInstance();


			if (_configuration.Args().TeleoptiPrincipalForLegacy)
			{
				builder.RegisterType<TeleoptiPrincipalForLegacyFactory>().As<IPrincipalFactory>().SingleInstance();
			}
			else
			{
				builder.RegisterType<TeleoptiPrincipalFactory>().As<IPrincipalFactory>().SingleInstance();
				builder.CacheByClassProxy<TeleoptiPrincipalInternalsFactory>()
					.As<IMakeOrganisationMembershipFromPerson>()
					.As<IRetrievePersonNameForPerson>()
					.SingleInstance();
				_configuration.Args().Cache.This<TeleoptiPrincipalInternalsFactory>(b => b
					.CacheMethod(m => m.MakeOrganisationMembership(null))
					.CacheMethod(m => m.NameForPerson(null))
				);
			}

			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>().InstancePerDependency();
			builder.RegisterType<ApplicationFunctionsForRole>().As<ApplicationFunctionsForRole>().InstancePerDependency();
			builder.RegisterType<ClaimSetForApplicationRole>().As<ClaimSetForApplicationRole>().InstancePerDependency();

			builder.RegisterType<RepositoryFactory>().As<IRepositoryFactory>().SingleInstance();
			builder.RegisterType<ScheduleStorageFactory>().As<IScheduleStorageFactory>().SingleInstance();
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
			builder.RegisterType<LoggedOnUserIsPerson>().As<ILoggedOnUserIsPerson>().SingleInstance();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>().SingleInstance();

			builder.RegisterType<CurrentIdentity>().As<ICurrentIdentity>().SingleInstance();
			builder.RegisterType<UpdatedBy>()
				.As<IUpdatedBy>()
				.As<IUpdatedByScope>()
				.SingleInstance();

			builder.RegisterType<UpdatedBySystemUser>().As<IUpdatedBySystemUser>();
		}
	}
}