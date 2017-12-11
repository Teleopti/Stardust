using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
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
		private readonly bool _supportPerRequest;

		public OutboundAreaModule(bool supportPerRequest)
		{
			_supportPerRequest = supportPerRequest;
		}

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

			builder.RegisterType<OutboundCampaignViewModelMapper>().As<IOutboundCampaignViewModelMapper>().SingleInstance();
			registerType<OutboundCampaignPersister, IOutboundCampaignPersister>(builder);
			registerType<OutboundCampaignMapper, IOutboundCampaignMapper>(builder);
			registerType<ProductionReplanHelper, IProductionReplanHelper>(builder);
			registerType<CampaignListProvider, ICampaignListProvider>(builder);
			registerType<CampaignVisualizationProvider, ICampaignVisualizationProvider>(builder);
			registerType<OutboundThresholdSettingsPersistorAndProvider, ISettingsPersisterAndProvider<OutboundThresholdSettings>>(builder);
		}

		private void overwriteSchedulingCommonModule(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundPeriodMover>().As<IOutboundPeriodMover>();
			builder.RegisterType<OutboundScheduledResourcesProvider>().As<IOutboundScheduledResourcesProvider>();
			builder.RegisterType<CreateOrUpdateSkillDays>().As<ICreateOrUpdateSkillDays>();			
		}

		private void registerType<TImplementer, TService>(ContainerBuilder builder)
		{
			var reg = builder.RegisterType<TImplementer>().As<TService>();
			if (_supportPerRequest)
				reg.InstancePerRequest();
		}
	}
}