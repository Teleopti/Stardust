using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class UnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentUnitOfWork>().As<ICurrentUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();

			builder.RegisterType<CurrentDataSource>().As<ICurrentDataSource>().SingleInstance();
			builder.Register(c => c.Resolve<ICurrentDataSource>().Current())
				.As<IDataSource>()
				.ExternallyOwned();
		}
	}
}