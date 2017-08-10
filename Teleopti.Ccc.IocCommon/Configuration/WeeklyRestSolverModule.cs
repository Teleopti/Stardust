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
			builder.RegisterType<ContractWeeklyRestForPersonWeek>().As<IContractWeeklyRestForPersonWeek>();
			builder.RegisterType<DayOffToTimeSpanExtractor>().As<IDayOffToTimeSpanExtractor>();
			builder.RegisterType<EnsureWeeklyRestRule>().As<IEnsureWeeklyRestRule>();
			builder.RegisterType<ExtractDayOffFromGivenWeek>().As<IExtractDayOffFromGivenWeek>();
			builder.RegisterType<ScheduleDayWorkShiftTimeExtractor>().As<IScheduleDayWorkShiftTimeExtractor>();
			builder.RegisterType<WeeklyRestSolverService>();
			builder.RegisterType<DeleteScheduleDayFromUnsolvedPersonWeek>().SingleInstance();
			builder.RegisterType<IdentifyDayOffWithHighestSpan>();
			builder.RegisterType<ShiftNudgeLater>();
			builder.RegisterType<ShiftNudgeManager>().As<IShiftNudgeManager>();
			builder.RegisterType<DayOffMaxFlexCalculator>().As<IDayOffMaxFlexCalculator>();
			builder.RegisterType<EnsureWeeklyRestRule>().As<IEnsureWeeklyRestRule>();
			builder.RegisterType<TeamBlockScheduleCloner>().SingleInstance();
			builder.RegisterType<WeeksFromScheduleDaysExtractor>().As<IWeeksFromScheduleDaysExtractor>().SingleInstance();
			builder.RegisterType<DayOffMaxFlexCalculator>().As<IDayOffMaxFlexCalculator>();
			builder.RegisterType<VerifyWeeklyRestAroundDayOffSpecification>().As<IVerifyWeeklyRestAroundDayOffSpecification>();
			builder.RegisterType<ScheduleDayIsLockedSpecification>().As<IScheduleDayIsLockedSpecification>().SingleInstance();
			builder.RegisterType<AllTeamMembersInSelectionSpecification>().As<IAllTeamMembersInSelectionSpecification>().SingleInstance();
			builder.RegisterType<PersonWeekViolatingWeeklyRestSpecification>().As<IPersonWeekViolatingWeeklyRestSpecification>();
			builder.RegisterType<BrokenWeekCounterForAPerson>().As<IBrokenWeekCounterForAPerson>();
			builder.RegisterType<WeeklyRestSolverExecuter>().InstancePerLifetimeScope();
		}
	}
}