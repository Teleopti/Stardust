using System;
using Autofac;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class AuthorizationContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FunctionsForRoleProvider>().As<IFunctionsForRoleProvider>();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LicensedFunctionsProvider>().As<ILicensedFunctionsProvider>();
			builder.RegisterType<ExternalFunctionsProvider>().As<IExternalFunctionsProvider>();
			builder.RegisterType<RoleToClaimSetTransformer>().As<IRoleToClaimSetTransformer>().InstancePerDependency();
			builder.RegisterType<DefinedRaptorApplicationFunctionFactory>().As<IDefinedRaptorApplicationFunctionFactory>();
		}
    }
}