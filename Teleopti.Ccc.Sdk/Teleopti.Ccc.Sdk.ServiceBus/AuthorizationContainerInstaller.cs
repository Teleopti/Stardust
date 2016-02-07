using System;
using Autofac;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class AuthorizationContainerInstaller : Module
    {
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationFunctionsForRole>().As<ApplicationFunctionsForRole>();
			builder.RegisterType<RoleToPrincipalCommand>().As<IRoleToPrincipalCommand>();
			builder.RegisterType<LicensedFunctionsProvider>().As<ILicensedFunctionsProvider>();
			builder.RegisterType<ClaimSetForApplicationRole>().As<ClaimSetForApplicationRole>().InstancePerDependency();
			builder.RegisterType<DefinedRaptorApplicationFunctionFactory>().As<IDefinedRaptorApplicationFunctionFactory>();
		}
    }
}