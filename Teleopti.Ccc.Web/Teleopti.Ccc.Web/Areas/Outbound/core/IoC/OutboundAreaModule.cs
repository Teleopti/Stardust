using System;
using Autofac;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			builder.RegisterType<OutboundPeriodMover>().As<IOutboundPeriodMover>().SingleInstance();

			builder.RegisterType<IncomingTaskFactory>().As<IncomingTaskFactory>().SingleInstance();
			builder.RegisterType<FlatDistributionSetter>().As<FlatDistributionSetter>().SingleInstance();
			builder.RegisterType<OutboundProductionPlanFactory>().As<OutboundProductionPlanFactory>().SingleInstance();
			builder.RegisterType<CreateOrUpdateSkillDays>().As<ICreateOrUpdateSkillDays>().SingleInstance();
			builder.RegisterType<OutboundScheduledResourcesProvider>().As<OutboundScheduledResourcesProvider>().SingleInstance();

			//OutboundScheduledResourcesProvider's dependences
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>().SingleInstance();
			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().SingleInstance();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>().SingleInstance();
			builder.RegisterType<IntraIntervalFinderService>().As<IIntraIntervalFinderService>().SingleInstance();
			builder.RegisterType<SkillDayIntraIntervalFinder>().As<ISkillDayIntraIntervalFinder>().SingleInstance();
			builder.RegisterType<IntraIntervalFinder>().As<IIntraIntervalFinder>().SingleInstance();
			builder.RegisterType<SkillActivityCountCollector>().As<ISkillActivityCountCollector>().SingleInstance();
			builder.RegisterType<FullIntervalFinder>().As<IFullIntervalFinder>().SingleInstance();
			builder.RegisterType<SkillActivityCounter>().As<ISkillActivityCounter>().SingleInstance();
			builder.RegisterType<PairMatrixService<Guid>>().As<IPairMatrixService<Guid>>().SingleInstance();
			builder.RegisterType<PairDictionaryFactory<Guid>>().As<IPairDictionaryFactory<Guid>>().SingleInstance();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().SingleInstance();
			builder.RegisterType<SchedulerStateHolder>().As<ISchedulerStateHolder>().SingleInstance();
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().SingleInstance();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().SingleInstance();
			builder.RegisterType<CurrentUnitOfWorkFactory>().As<ICurrentUnitOfWorkFactory>().SingleInstance();
			builder.RegisterType<OutboundAssignedStaffProvider>().As<OutboundAssignedStaffProvider>().SingleInstance();
		}
	}
}