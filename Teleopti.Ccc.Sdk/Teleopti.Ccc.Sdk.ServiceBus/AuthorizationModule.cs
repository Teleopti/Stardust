using Autofac;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class AuthorizationModule : Module
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