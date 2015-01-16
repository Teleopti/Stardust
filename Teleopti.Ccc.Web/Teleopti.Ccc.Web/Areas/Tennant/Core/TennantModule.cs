using Autofac;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class TennantModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<ApplicationUserQuery>().As<IApplicationUserQuery>().SingleInstance();
		}
	}
}