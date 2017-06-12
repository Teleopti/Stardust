
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;

namespace Teleopti.Ccc.Web.Areas.Options.IoC
{
	public class OptionsAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SiteOpenHoursPersister>().As<ISiteOpenHoursPersister>().InstancePerLifetimeScope();
			builder.RegisterType<SiteWithOpenHourProvider>().As<ISiteWithOpenHourProvider>().InstancePerLifetimeScope();
		}
	}
}