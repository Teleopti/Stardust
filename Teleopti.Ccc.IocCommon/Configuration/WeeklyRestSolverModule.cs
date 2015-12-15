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
			builder.RegisterType<ShiftNudgeEarlier>().As<IShiftNudgeEarlier>();
			builder.RegisterType<ContractWeeklyRestForPersonWeek>().As<IContractWeeklyRestForPersonWeek>();
			builder.RegisterType<DayOffToTimeSpanExtractor>().As<IDayOffToTimeSpanExtractor>();
			builder.RegisterType<EnsureWeeklyRestRule>().As<IEnsureWeeklyRestRule>();
			builder.RegisterType<ExtractDayOffFromGivenWeek>().As<IExtractDayOffFromGivenWeek>();
			builder.RegisterType<ScheduleDayWorkShiftTimeExtractor>().As<IScheduleDayWorkShiftTimeExtractor>();
			builder.RegisterType<WeeklyRestSolverService>().As<IWeeklyRestSolverService>();
			builder.RegisterType<DeleteScheduleDayFromUnsolvedPersonWeek>().As<IDeleteScheduleDayFromUnsolvedPersonWeek>();
			builder.RegisterType<IdentifyDayOffWithHighestSpan>();
			builder.RegisterType<ShiftNudgeLater>().As<IShiftNudgeLater>();
			builder.RegisterType<ShiftNudgeManager>().As<IShiftNudgeManager>();
			builder.RegisterType<DayOffMaxFlexCalculator>().As<IDayOffMaxFlexCalculator>();
			builder.RegisterType<EnsureWeeklyRestRule>().As<IEnsureWeeklyRestRule>();
			builder.RegisterType<ContractWeeklyRestForPersonWeek>().As<IContractWeeklyRestForPersonWeek>();
			builder.RegisterType<TeamBlockScheduleCloner>().As<ITeamBlockScheduleCloner>();
			builder.RegisterType<WeeksFromScheduleDaysExtractor>().As<IWeeksFromScheduleDaysExtractor>();
			builder.RegisterType<DayOffMaxFlexCalculator>().As<IDayOffMaxFlexCalculator>();
			builder.RegisterType<VerifyWeeklyRestAroundDayOffSpecification>().As<IVerifyWeeklyRestAroundDayOffSpecification>();
			builder.RegisterType<ScheduleDayIsLockedSpecification>().As<IScheduleDayIsLockedSpecification>();
			builder.RegisterType<AllTeamMembersInSelectionSpecification>().As<IAllTeamMembersInSelectionSpecification>();
			builder.RegisterType<PersonWeekViolatingWeeklyRestSpecification>().As<IPersonWeekViolatingWeeklyRestSpecification>();
			builder.RegisterType<BrokenWeekCounterForAPerson>().As<IBrokenWeekCounterForAPerson>();
			builder.RegisterType<WeeklyRestSolverExecuter>().InstancePerLifetimeScope();
		}
	}
}