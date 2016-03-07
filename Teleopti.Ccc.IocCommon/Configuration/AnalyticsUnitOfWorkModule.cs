using Autofac;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class AnalyticsUnitOfWorkModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public AnalyticsUnitOfWorkModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentAnalyticsUnitOfWork>().As<ICurrentAnalyticsUnitOfWork>().SingleInstance();
			builder.RegisterType<CurrentAnalyticsUnitOfWorkFactory>().As<ICurrentAnalyticsUnitOfWorkFactory>().SingleInstance();
			builder.RegisterType<AnalyticsUnitOfWorkAspect>().As<IAnalyticsUnitOfWorkAspect>().InstancePerDependency();
		}
	}
}