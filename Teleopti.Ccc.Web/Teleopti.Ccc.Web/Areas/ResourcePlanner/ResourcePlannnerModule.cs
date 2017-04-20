using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ResourcePlannerModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{			
			builder.RegisterType<MissingForecastProvider>()
				.SingleInstance()
				.As<IMissingForecastProvider>();

			builder.RegisterType<BasicSchedulingValidator>()
				.SingleInstance()
				.As<IBasicSchedulingValidator>();

			builder.RegisterType<PersonSkillValidator>()
				.SingleInstance()
				.As<IPersonSkillValidator>();

			builder.RegisterType<PersonPeriodValidator>()
				.SingleInstance()
				.As<IPersonPeriodValidator>();

			builder.RegisterType<PersonSchedulePeriodValidator>()
				.SingleInstance()
				.As<IPersonSchedulePeriodValidator>();

			builder.RegisterType<PersonShiftBagValidator>()
				.SingleInstance()
				.As<IPersonShiftBagValidator>();

			builder.RegisterType<PersonPartTimePercentageValidator>()
				.SingleInstance()
				.As<IPersonPartTimePercentageValidator>();

			builder.RegisterType<PersonContractValidator>()
				.SingleInstance()
				.As<IPersonContractValidator>();

			builder.RegisterType<PersonContractScheduleValidator>()
				.SingleInstance()
				.As<IPersonContractScheduleValidator>();

			builder.RegisterType<NextPlanningPeriodProvider>()
				.SingleInstance()
				.As<INextPlanningPeriodProvider>();

			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<WorkShiftWorkTime>().As<IWorkShiftWorkTime>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();
			builder.RegisterType<RuleSetProjectionService>().As<IRuleSetProjectionService>();
			builder.RegisterType<ShiftCreatorService>().As<IShiftCreatorService>();
			builder.RegisterType<CreateWorkShiftsFromTemplate>().As<ICreateWorkShiftsFromTemplate>();
			builder.RegisterType<RuleSetProjectionEntityService>().As<IRuleSetProjectionEntityService>();


			builder.RegisterType<BasicActionThrottler>().As<IActionThrottler>().SingleInstance();
			builder.RegisterType<ClearScheduleEvents>().As<IClearScheduleEvents>().SingleInstance();

		}
	}
}