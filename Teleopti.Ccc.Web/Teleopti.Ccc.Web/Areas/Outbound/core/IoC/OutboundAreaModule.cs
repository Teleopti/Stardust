using Autofac;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;
using OutboundRuleConfigurationProvider = Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider.OutboundRuleConfigurationProvider;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.IoC
{
	public class OutboundAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			//dependences
			builder.RegisterModule(new SchedulingCommonModule());
			builder.RegisterModule(new OutboundScheduledResourcesProviderModule());

			builder.RegisterType<OutboundCampaignPersister>().As<IOutboundCampaignPersister>().SingleInstance();
			builder.RegisterType<OutboundCampaignViewModelMapper>().As<IOutboundCampaignViewModelMapper>().SingleInstance();
			builder.RegisterType<OutboundCampaignMapper>().As<IOutboundCampaignMapper>().SingleInstance();
			builder.RegisterType<ActivityProvider>().As<IActivityProvider>().SingleInstance();
			builder.RegisterType<OutboundSkillCreator>().As<IOutboundSkillCreator>().SingleInstance();
			builder.RegisterType<OutboundSkillTypeProvider>().As<IOutboundSkillTypeProvider>().SingleInstance();
			builder.RegisterType<OutboundSkillPersister>().As<IOutboundSkillPersister>().SingleInstance();
			builder.RegisterType<ProductionReplanHelper>().As<IProductionReplanHelper>().SingleInstance();
			builder.RegisterType<OutboundPeriodMover>().As<IOutboundPeriodMover>().SingleInstance();
			builder.RegisterType<CampaignListOrderProvider>().As<ICampaignListOrderProvider>().SingleInstance();
			builder.RegisterType<CampaignListProvider>().As<ICampaignListProvider>().InstancePerLifetimeScope();
			builder.RegisterType<CampaignSummaryViewModelFactory>().As<ICampaignSummaryViewModelFactory>().InstancePerLifetimeScope();
			builder.RegisterType<CampaignVisualizationProvider>().As<ICampaignVisualizationProvider>().SingleInstance();			
			builder.RegisterType<OutboundThresholdSettingsPersistorAndProvider>().As<ISettingsPersisterAndProvider<OutboundThresholdSettings>>().SingleInstance();

			builder.RegisterType<OutboundRuleConfigurationProvider>().As<IOutboundRuleConfigurationProvider>().InstancePerLifetimeScope();
		}
	}
}