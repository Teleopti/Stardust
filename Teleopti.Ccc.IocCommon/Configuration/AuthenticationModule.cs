using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
    public class AuthenticationModule : Module
    {
        private readonly IApplicationData _applicationData;

        public AuthenticationModule()
        {
        }

        public AuthenticationModule(IApplicationData applicationData) : this()
        {
            _applicationData = applicationData;
        }

        protected override void Load(ContainerBuilder builder)
        {
			builder.RegisterType<WindowsAppDomainPrincipalContext>()
				.As<ICurrentPrincipalContext>()
				.SingleInstance();
        	builder.RegisterType<TeleoptiPrincipalFactory>()
        		.As<IPrincipalFactory>()
        		.SingleInstance();
			builder.RegisterType<LogOnOff>()
                .As<ILogOnOff>()
                .SingleInstance();
            builder.RegisterType<RepositoryFactory>()
                .As<IRepositoryFactory>()
                .SingleInstance();
            builder.RegisterType<WindowsDataSourceProvider>()
                .As<IDataSourceProvider>();
            builder.RegisterType<ApplicationDataSourceProvider>()
                .As<IDataSourceProvider>();
            builder.RegisterType<AvailableDataSourcesProvider>()
                .As<IAvailableDataSourcesProvider>()
                .SingleInstance();
            builder.RegisterType<FindUserDetail>()
                .As<IFindUserDetail>()
                .SingleInstance();
            builder.RegisterType<FindApplicationUser>()
                .As<IFindApplicationUser>()
                .SingleInstance();
            builder.RegisterType<CheckNullUser>()
                .As<ICheckNullUser>()
                .SingleInstance();
            builder.RegisterType<CheckPassword>()
                .As<ICheckPassword>()
                .SingleInstance();
            builder.RegisterType<CheckSuperUser>()
                .As<ICheckSuperUser>()
                .SingleInstance();
            builder.RegisterType<CheckUserDetail>()
                .As<ICheckUserDetail>()
                .SingleInstance();
            builder.RegisterType<CheckPasswordChange>()
                .As<ICheckPasswordChange>()
                .SingleInstance();
            builder.RegisterType<CheckBruteForce>()
                .As<ICheckBruteForce>()
                .SingleInstance();
				builder.Register<IPasswordPolicy>(c =>
                             	{
											if(c.Resolve<IApplicationData>().LoadPasswordPolicyService==null)
												return new DummyPasswordPolicy();
                             		return new PasswordPolicy(c.Resolve<ILoadPasswordPolicyService>());
                             	})
                .As<IPasswordPolicy>()
                .SingleInstance();
            builder.RegisterType<SystemUserSpecification>()
                .As<ISystemUserSpecification>()
                .SingleInstance();
            builder.RegisterType<SystemUserPasswordSpecification>()
                .As<ISystemUserPasswordSpecification>()
                .SingleInstance();
            builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>().InstancePerDependency();
            builder.RegisterType<FunctionsForRoleProvider>().As<IFunctionsForRoleProvider>().InstancePerDependency();
            builder.RegisterType<LicensedFunctionsProvider>().As<ILicensedFunctionsProvider>().SingleInstance();
            builder.RegisterType<ExternalFunctionsProvider>().As<IExternalFunctionsProvider>().InstancePerDependency();
			builder.RegisterType<RoleToClaimSetTransformer>().As<IRoleToClaimSetTransformer>().InstancePerDependency();
			builder.RegisterType<DefinedRaptorApplicationFunctionFactory>().As<IDefinedRaptorApplicationFunctionFactory>().InstancePerDependency();
            if (_applicationData!=null)
                builder.Register(c => _applicationData)
                .As<IApplicationData>();
            else
            builder.Register(c => StateHolder.Instance.StateReader.ApplicationScopeData)
                .As<IApplicationData>().SingleInstance();
            builder.Register(c => c.Resolve<IApplicationData>().LoadPasswordPolicyService)
                .As<ILoadPasswordPolicyService>().SingleInstance();
        	builder.RegisterType<ModifyPassword>().As<IModifyPassword>().SingleInstance();
			builder.RegisterType<CurrentTeleoptiPrincipal>()
				.As<ICurrentTeleoptiPrincipal>()
				.SingleInstance();
			builder.RegisterType<PrincipalAuthorization>()
				.As<IPrincipalAuthorization>()
				.SingleInstance();
			builder.RegisterType<UserCulture>().As<IUserCulture>().SingleInstance();
			builder.RegisterType<LoggedOnUser>().As<ILoggedOnUser>().SingleInstance();
			builder.RegisterType<UserTimeZone>().As<IUserTimeZone>().SingleInstance();
			builder.RegisterType<ApplicationDataSourceProvider>().As<IApplicationDataSourceProvider>().SingleInstance();
			builder.RegisterType<OneWayEncryption>().As<IOneWayEncryption>().SingleInstance();
			builder.RegisterType<CurrentIdentity>().As<ICurrentIdentity>().SingleInstance();
			builder.RegisterType<LogonLogger>().As<ILogonLogger>().SingleInstance();
        }
    }
}
