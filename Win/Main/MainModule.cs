using Autofac;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main
{
	public class MainModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<RepositoryFactory>();
			builder.Register(
				(c, p) => new RaptorApplicationFunctionsSynchronizer(p.TypedAs<RepositoryFactory>(), UnitOfWorkFactory.Current));

		}
	}
}
