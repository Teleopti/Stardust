using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReadModelUnitOfWorkState>()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.As<IReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}
}