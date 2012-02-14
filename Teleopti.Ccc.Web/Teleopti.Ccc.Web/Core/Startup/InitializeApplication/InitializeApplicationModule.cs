using Autofac;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class InitializeApplicationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PhysicalApplicationPath>().As<IPhysicalApplicationPath>();
			builder.RegisterType<AppConfigSettings>().As<ISettings>();
		}
	}
}