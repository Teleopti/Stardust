using Autofac;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
	public class MainModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RaptorApplicationFunctionsSynchronizer>().As<IRaptorApplicationFunctionsSynchronizer>();
			// builder.RegisterType<ICurrentUnitOfWorkFactory>();
		}
	}
}