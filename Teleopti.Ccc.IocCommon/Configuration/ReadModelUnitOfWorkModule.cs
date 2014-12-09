using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LiteTransactionSyncronization>()
				.As<ILiteTransactionSyncronization>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkState>()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.As<IReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}
}