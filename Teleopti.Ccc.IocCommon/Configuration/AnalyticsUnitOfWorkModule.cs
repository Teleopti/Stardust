using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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
			builder.RegisterType<AnalyticsUnitOfWorkAspect>().As<IAspect>().SingleInstance();
			builder.RegisterType<WithAnalyticsUnitOfWork>().SingleInstance().ApplyAspects();
		}
	}
}