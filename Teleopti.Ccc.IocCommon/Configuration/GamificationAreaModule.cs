using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class GamificationAreaModule: Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TenantUserPersister>().As<ITenantUserPersister>().SingleInstance().ApplyAspects();
			builder.RegisterType<LineExtractor>().As<ILineExtractor>().SingleInstance();
			builder.RegisterType<ExternalPerformanceInfoFileProcessor>().As<IExternalPerformanceInfoFileProcessor>().SingleInstance();
			builder.RegisterType<ImportExternalPerformanceInfoService>().As<IImportExternalPerformanceInfoService>();
			builder.RegisterType<ImportJobArtifactValidator>().As<IImportJobArtifactValidator>().ApplyAspects();
			builder.RegisterType<ExternalPerformancePersister>().As<IExternalPerformancePersister>().SingleInstance();
		}
	}
}
