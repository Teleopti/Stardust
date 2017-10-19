using Autofac;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class WeeklyRestSolverModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ShiftNudgeEarlier>();
			builder.RegisterType<ContractWeeklyRestForPersonWeek>().As<IContractWeeklyRestForPersonWeek>().SingleInstance();
			builder.RegisterType<DayOffToTimeSpanExtractor>().As<IDayOffToTimeSpanExtractor>();
			builder.RegisterType<EnsureWeeklyRestRule>().As<IEnsureWeeklyRestRule>().SingleInstance();
			builder.RegisterType<ExtractDayOffFromGivenWeek>().As<IExtractDayOffFromGivenWeek>().SingleInstance();
			builder.RegisterType<ScheduleDayWorkShiftTimeExtractor>().As<IScheduleDayWorkShiftTimeExtractor>().SingleInstance();
			builder.RegisterType<WeeklyRestSolverService>();
			builder.RegisterType<DeleteScheduleDayFromUnsolvedPersonWeek>().SingleInstance();
			builder.RegisterType<IdentifyDayOffWithHighestSpan>().SingleInstance();
			builder.RegisterType<ShiftNudgeLater>();
			builder.RegisterType<ShiftNudgeManager>().As<IShiftNudgeManager>();
			builder.RegisterType<DayOffMaxFlexCalculator>().As<IDayOffMaxFlexCalculator>().SingleInstance();
			builder.RegisterType<TeamBlockScheduleCloner>().SingleInstance();
			builder.RegisterType<WeeksFromScheduleDaysExtractor>().As<IWeeksFromScheduleDaysExtractor>().SingleInstance();
			builder.RegisterType<VerifyWeeklyRestAroundDayOffSpecification>().As<IVerifyWeeklyRestAroundDayOffSpecification>().SingleInstance();
			builder.RegisterType<ScheduleDayIsLockedSpecification>().As<IScheduleDayIsLockedSpecification>().SingleInstance();
			builder.RegisterType<AllTeamMembersInSelectionSpecification>().As<IAllTeamMembersInSelectionSpecification>().SingleInstance();
			builder.RegisterType<PersonWeekViolatingWeeklyRestSpecification>().As<IPersonWeekViolatingWeeklyRestSpecification>().SingleInstance();
			builder.RegisterType<BrokenWeekCounterForAPerson>().As<IBrokenWeekCounterForAPerson>().SingleInstance();
			builder.RegisterType<WeeklyRestSolverExecuter>().InstancePerLifetimeScope();
		}
	}
}