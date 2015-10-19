using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ResourcePlannerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{			
			builder.RegisterType<MissingForecastProvider>()
				.SingleInstance()
				.As<IMissingForecastProvider>();
			builder.RegisterType<NextPlanningPeriodProvider>()
				.SingleInstance()
				.As<INextPlanningPeriodProvider>();
			
			builder.RegisterType<CurrentUnitOfWorkScheduleRangePersister>().As<IScheduleRangePersister>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftWorkTime>().As<IWorkShiftWorkTime>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();
			builder.RegisterType<RuleSetProjectionService>().As<IRuleSetProjectionService>();
			builder.RegisterType<ShiftCreatorService>().As<IShiftCreatorService>();
			builder.RegisterType<CreateWorkShiftsFromTemplate>().As<ICreateWorkShiftsFromTemplate>();
			builder.RegisterType<RuleSetProjectionEntityService>().As<IRuleSetProjectionEntityService>();

			builder.RegisterType<BasicActionThrottler>().As<IActionThrottler>().SingleInstance();
			builder.RegisterType<ClearEvents>().As<IClearEvents>().SingleInstance();
			builder.RegisterType<ViolatedSchedulePeriodBusinessRule>();
			builder.RegisterType<DayOffBusinessRuleValidation>();
		}
	}
}