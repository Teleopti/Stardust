using Autofac;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReadModelTransactionSyncronization>()
				.As<IReadModelTransactionSyncronization>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkState>()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.As<IAspect>()
				.SingleInstance();
			builder.RegisterType<WithReadModelUnitOfWork>()
				.SingleInstance()
				.ApplyAspects();
		}
	}
}