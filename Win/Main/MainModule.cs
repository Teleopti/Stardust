using Autofac;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Win.Main
{
	public class MainModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RaptorApplicationFunctionsSynchronizer>().As<IRaptorApplicationFunctionsSynchronizer>();
		}
	}
}