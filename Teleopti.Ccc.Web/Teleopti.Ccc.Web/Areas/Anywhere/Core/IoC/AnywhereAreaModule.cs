﻿using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC
{
	public class AnywhereAreaModule : Module
	{
		private readonly IIocConfiguration _config;

		public AnywhereAreaModule(IIocConfiguration config)
		{
			_config = config;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();

			builder.RegisterType<GroupScheduleViewModelFactory>().As<IGroupScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<GroupScheduleViewModelMapper>().As<IGroupScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleDayViewModelFactory>().As<IPersonScheduleDayViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<IPersonScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<PersonScheduleDayViewModelMapper>().As<IPersonScheduleDayViewModelMapper>().SingleInstance();
			builder.RegisterType<DailyStaffingMetricsViewModelFactory>().As<IDailyStaffingMetricsViewModelFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ReportItemsProvider>().As<IReportItemsProvider>().SingleInstance();

			if (_config.Toggle(Toggles.MultiTenantSSOSupport_StandardReports_15093))
			{
				builder.Register(c => new ReportUrlConstructor(_config.Args().ReportServer, c.Resolve<IConfigReader>()))
					.As<IReportUrl>()
					.SingleInstance();
			}
			else
			{
				builder.RegisterType<ReportUrl>().As<IReportUrl>().SingleInstance();
			}
			
			builder.RegisterType<ResourceCalculateSkillCommand>().As<IResourceCalculateSkillCommand>().InstancePerLifetimeScope();
			builder.RegisterType<SkillLoaderDecider>().As<ISkillLoaderDecider>().SingleInstance();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();
			builder.RegisterType<GetSiteAdherence>().As<IGetSiteAdherence>().InstancePerLifetimeScope();
			builder.RegisterType<GetTeamAdherence>().As<IGetTeamAdherence>().InstancePerLifetimeScope();
			builder.RegisterType<GetBusinessUnitId>().As<IGetBusinessUnitId>().InstancePerLifetimeScope();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>();
			builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();

			builder.RegisterType<IntraIntervalFinderServiceToggle29845Off>().As<IIntraIntervalFinderService>();	
		}
	}
}