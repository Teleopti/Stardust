using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
	public class UpdateScheduleModule : Module
	{
		private readonly IocConfiguration _configuration;

		public UpdateScheduleModule(IocConfiguration configuration)
		{
			_configuration = configuration;
		}
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleDayForPerson>().As<IScheduleDayForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleRangeForPerson>().As<IScheduleRangeForPerson>().InstancePerLifetimeScope();
			builder.RegisterType<SchedulerStateHolder>().As<ISchedulerStateHolder>().InstancePerLifetimeScope();
			builder.RegisterType<TimeZoneGuard>().As<ITimeZoneGuard>().SingleInstance();
			builder.RegisterType<DisableDeletedFilter>().As<IDisableDeletedFilter>().SingleInstance();

			builder.RegisterType<NonBlendSkillCalculator>().As<INonBlendSkillCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<OccupiedSeatCalculator>().As<IOccupiedSeatCalculator>();
			builder.RegisterType<PersonSkillProvider>().As<IPersonSkillProvider>().InstancePerLifetimeScope();
			builder.RegisterType<PeriodDistributionService>().As<IPeriodDistributionService>();
			builder.RegisterType<IntraIntervalFinderService>().As<IIntraIntervalFinderService>();	
			builder.RegisterType<SkillDayLoadHelper>().InstancePerLifetimeScope();
			
			builder.RegisterType<PeopleAndSkillLoaderDecider>().As<IPeopleAndSkillLoaderDecider>().InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(PairMatrixService<>)).As(typeof(IPairMatrixService<>)).InstancePerLifetimeScope();
			builder.RegisterGeneric(typeof(PairDictionaryFactory<>)).As(typeof(IPairDictionaryFactory<>)).InstancePerLifetimeScope();
			builder.RegisterType<LoadSchedulingStateHolderForResourceCalculation>().As<ILoadSchedulingStateHolderForResourceCalculation>().InstancePerLifetimeScope();
		}
	}
}