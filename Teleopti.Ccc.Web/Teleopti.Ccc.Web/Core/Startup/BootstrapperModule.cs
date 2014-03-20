using Autofac;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Ccc.Web.Core.Startup.VerifyLicense;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class BootstrapperModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.Where(t => typeof(IBootstrapperTask).IsAssignableFrom(t))
				.SingleInstance()
				.As<IBootstrapperTask>();

			builder.RegisterModule<InitializeApplicationModule>();
			builder.RegisterModule<VerifyLicenseModule>();
			builder.RegisterType<RegisterArea>().SingleInstance().As<IRegisterArea>();
			builder.RegisterType<RegisterAreas>().SingleInstance().As<IRegisterAreas>();
			builder.RegisterType<FindAreaRegistrations>().SingleInstance().As<IFindAreaRegistrations>();
		}
	}
}