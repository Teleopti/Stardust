using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Ccc.Web.Core.Startup.VerifyLicense;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class BootstrapperModule : Module
	{
		private readonly IIocConfiguration _config;

		public BootstrapperModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.Where(t => typeof(IBootstrapperTask).IsAssignableFrom(t) && t.EnabledByToggle(_config))
				.SingleInstance()
				.As<IBootstrapperTask>();

			builder.RegisterModule<InitializeApplicationModule>();
			builder.RegisterModule<VerifyLicenseModule>();
			builder.RegisterType<RegisterArea>().SingleInstance().As<IRegisterArea>();
			builder.RegisterType<RegisterAreas>().SingleInstance().As<IRegisterAreas>();
			builder.RegisterType<GlobalConfigurationWrapper>().SingleInstance().As<IGlobalConfiguration>();
			builder.RegisterType<FindAreaRegistrations>().SingleInstance().As<IFindAreaRegistrations>();
		}
	}
}