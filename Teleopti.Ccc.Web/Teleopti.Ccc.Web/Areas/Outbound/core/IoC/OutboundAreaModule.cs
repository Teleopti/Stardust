﻿using Autofac;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.IoC
{
	public class OutboundAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundCampaignPersister>().As<IOutboundCampaignPersister>().SingleInstance();
			builder.RegisterType<OutboundCampaignViewModelMapper>().As<IOutboundCampaignViewModelMapper>().SingleInstance();
			builder.RegisterType<OutboundCampaignMapper>().As<IOutboundCampaignMapper>().SingleInstance();
			builder.RegisterType<OutboundActivityProvider>().As<IOutboundActivityProvider>().SingleInstance();
			builder.RegisterType<OutboundSkillCreator>().As<IOutboundSkillCreator>().SingleInstance();
			builder.RegisterType<OutboundSkillTypeProvider>().As<IOutboundSkillTypeProvider>().SingleInstance();
			builder.RegisterType<OutboundSkillPersister>().As<IOutboundSkillPersister>().SingleInstance();
			builder.RegisterType<ProductionReplanHelper>().As<IProductionReplanHelper>().SingleInstance();

			builder.RegisterType<IncomingTaskFactory>().As<IncomingTaskFactory>().SingleInstance();
			builder.RegisterType<FlatDistributionSetter>().As<FlatDistributionSetter>().SingleInstance();
			builder.RegisterType<OutboundProductionPlanFactory>().As<OutboundProductionPlanFactory>().SingleInstance();
			builder.RegisterType<CreateOrUpdateSkillDays>().As<ICreateOrUpdateSkillDays>().SingleInstance();

			//builder.RegisterType<BacklogScheduledProvider>().As<BacklogScheduledProvider>().SingleInstance();
			//builder.RegisterType<FetchAndFillSkillDays>().As<IFetchAndFillSkillDays>().SingleInstance();
			//builder.RegisterType<ForecastingTargetMerger>().As<IForecastingTargetMerger>().SingleInstance();
			//builder.RegisterType<SkillDayRepository>().As<ISkillDayRepository>().SingleInstance();
		}
	}
}