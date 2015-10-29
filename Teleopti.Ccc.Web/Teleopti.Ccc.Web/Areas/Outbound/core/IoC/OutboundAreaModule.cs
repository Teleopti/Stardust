using Autofac;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.IoC
{
	public class OutboundAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			//dependences
			builder.RegisterModule(new OutboundScheduledResourcesProviderModule());

			overwriteSchedulingCommonModule(builder);

			builder.RegisterType<CampaignWarningProvider>().As<ICampaignWarningProvider>();
			builder.RegisterType<CampaignWarningConfigurationProvider>().As<ICampaignWarningConfigurationProvider>();
			builder.RegisterType<CampaignOverstaffRule>().AsSelf();
			builder.RegisterType<CampaignUnderServiceLevelRule>().AsSelf();

			builder.RegisterType<CampaignTaskManager>().As<IOutboundCampaignTaskManager>();
			builder.RegisterType<OutboundScheduledResourcesCacher>().As<IOutboundScheduledResourcesCacher>();


			builder.RegisterType<OutboundCampaignPersister>().As<IOutboundCampaignPersister>().InstancePerRequest();
			builder.RegisterType<OutboundCampaignViewModelMapper>().As<IOutboundCampaignViewModelMapper>().InstancePerRequest();
			builder.RegisterType<OutboundCampaignMapper>().As<IOutboundCampaignMapper>().InstancePerRequest();

			builder.RegisterType<ProductionReplanHelper>().As<IProductionReplanHelper>().InstancePerRequest();

			builder.RegisterType<CampaignListProvider>().As<ICampaignListProvider>().InstancePerRequest();
			builder.RegisterType<CampaignVisualizationProvider>().As<ICampaignVisualizationProvider>().InstancePerRequest();
			builder.RegisterType<OutboundThresholdSettingsPersistorAndProvider>().As<ISettingsPersisterAndProvider<OutboundThresholdSettings>>().InstancePerRequest();		
		}

		private void overwriteSchedulingCommonModule(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundPeriodMover>().As<IOutboundPeriodMover>();
			builder.RegisterType<OutboundScheduledResourcesProvider>().As<IOutboundScheduledResourcesProvider>();
			builder.RegisterType<CreateOrUpdateSkillDays>().As<ICreateOrUpdateSkillDays>();			
		}
	}
}