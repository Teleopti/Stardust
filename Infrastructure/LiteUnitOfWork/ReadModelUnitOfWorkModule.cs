using Autofac;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkModule : Module
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