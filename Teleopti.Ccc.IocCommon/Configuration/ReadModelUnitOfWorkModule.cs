using Autofac;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReadModelUnitOfWorkFactory>()
				.AsSelf()
				.As<IReadModelUnitOfWorkConfiguration>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkState>()
				.AsSelf()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}
}