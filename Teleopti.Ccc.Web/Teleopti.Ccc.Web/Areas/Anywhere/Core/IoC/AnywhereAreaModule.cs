using Autofac;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC
{
	public class AnywhereAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ExceptionHandlerPipelineModule>().As<IHubPipelineModule>();

			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelMapper>().As<ITeamScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelFactory>().As<IPersonScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<PersonScheduleViewModelMapper>().As<IPersonScheduleViewModelMapper>().SingleInstance();
			builder.RegisterType<DailyStaffingMetricsViewModelFactory>().As<IDailyStaffingMetricsViewModelFactory>().InstancePerLifetimeScope();
			builder.RegisterType<ResourceCalculateSkillCommand>().As<IResourceCalculateSkillCommand>().InstancePerLifetimeScope();
			builder.RegisterType<SkillLoaderDecider>().As<ISkillLoaderDecider>().SingleInstance();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>();
			builder.RegisterType<NonBlendSkillImpactOnPeriodForProjection>().As<INonBlendSkillImpactOnPeriodForProjection>();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<SkillVisualLayerCollectionDictionaryCreator>().As<ISkillVisualLayerCollectionDictionaryCreator>();
			builder.RegisterType<SeatImpactOnPeriodForProjection>().As<ISeatImpactOnPeriodForProjection>();
			builder.RegisterType<SingleSkillDictionary>().As<ISingleSkillDictionary>().InstancePerLifetimeScope();
		}
	}
}