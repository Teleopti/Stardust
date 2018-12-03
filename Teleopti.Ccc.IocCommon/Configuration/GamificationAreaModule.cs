using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Gamification;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class GamificationAreaModule: Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LineExtractorValidator>().As<ILineExtractorValidator>().SingleInstance();
			builder.RegisterType<ExternalPerformanceInfoFileProcessor>().As<IExternalPerformanceInfoFileProcessor>().SingleInstance();
			builder.RegisterType<ImportExternalPerformanceInfoService>().As<IImportExternalPerformanceInfoService>();
			builder.RegisterType<ExternalPerformancePersister>().As<IExternalPerformancePersister>().SingleInstance();
			builder.RegisterType<TenantLogonPersonProvider>().As<ITenantLogonPersonProvider>().ApplyAspects();
			builder.RegisterType<TenantPersonLogonQuerier>().As<ITenantPersonLogonQuerier>().ApplyAspects();
			builder.RegisterType<RecalculateBadgeJobService>().As<IRecalculateBadgeJobService>();
		}
	}
}
