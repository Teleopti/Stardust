using Autofac;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.Web.Core.Startup.VerifyLicense
{
	public class VerifyLicenseModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LicenseVerifierFactory>().As<ILicenseVerifierFactory>().SingleInstance();
			builder.RegisterType<SetLicenseActivator>().As<ISetLicenseActivator>().SingleInstance();
		}
	}
}