using Autofac;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ResourcePlannerModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public ResourcePlannerModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MissingForecastProvider>()
				.SingleInstance()
				.As<IMissingForecastProvider>();
			builder.RegisterType<NextPlanningPeriodProvider>()
				.SingleInstance()
				.As<INextPlanningPeriodProvider>();

			builder.RegisterModule(new SchedulingCommonModule());
			builder.RegisterModule(SchedulePersistModule.ForOtherModules());

			builder.RegisterType<CurrentUnitOfWorkScheduleRangePersister>().As<IScheduleRangePersister>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();
		}
	}
}