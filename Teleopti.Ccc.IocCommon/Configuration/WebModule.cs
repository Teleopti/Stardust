using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class WebModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
		}
	}
}