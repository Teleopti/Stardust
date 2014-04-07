using Autofac;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService
{
	public class UpdateScheduleModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>().
			        InstancePerDependency();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>();
			builder.RegisterType<ResourceOptimizationHelper>().As<IResourceOptimizationHelper>().InstancePerLifetimeScope();
			
			builder.RegisterType<SkillDayLoadHelper>().As<ISkillDayLoadHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>();
			builder.RegisterType<LoadSchedulingStateHolderForResourceCalculation>().As<ILoadSchedulingStateHolderForResourceCalculation>().InstancePerLifetimeScope();
			builder.RegisterType<ServiceLevelCalculator>().As<IServiceLevelCalculator>().SingleInstance();
		}
	}
}