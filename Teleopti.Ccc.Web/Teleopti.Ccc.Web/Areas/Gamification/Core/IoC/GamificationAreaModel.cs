using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Global.Core;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.IoC
{
	public class GamificationAreaModel : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<GamificationSettingMapper>().As<IGamificationSettingMapper>().SingleInstance();
			builder.RegisterType<GamificationSettingPersister>().As<IGamificationSettingPersister>().SingleInstance();
			builder.RegisterType<GamificationSettingProvider>().As<IGamificationSettingProvider>().SingleInstance();
			builder.RegisterType<TeamGamificationSettingProviderAndPersister>().As<ITeamGamificationSettingProviderAndPersister>().SingleInstance();
			builder.RegisterType<ImportExternalPerformanceInfoService>().As<IImportExternalPerformanceInfoService>().SingleInstance();
			builder.RegisterType<MultipartHttpContentExtractor>().As<IMultipartHttpContentExtractor>().SingleInstance();
		}
	}
}