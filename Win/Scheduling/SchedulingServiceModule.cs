using Autofac;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class SchedulingServiceModule : Module
    {
	    protected override void Load(ContainerBuilder builder)
	    {
			builder.RegisterType<VirtualSkillHelper>().As<IVirtualSkillHelper>().InstancePerLifetimeScope();
			builder.RegisterType<ClassicScheduleCommand>().As<ClassicScheduleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleCommand>().As<ScheduleCommand>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizationCommand>().As<OptimizationCommand>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffOptimizationDecisionMakerFactory>().As<IDayOffOptimizationDecisionMakerFactory>();
			builder.RegisterType<ScheduleOvertimeCommand>().As<IScheduleOvertimeCommand>();
			builder.RegisterType<TeamBlockMoveTimeBetweenDaysCommand>().As<ITeamBlockMoveTimeBetweenDaysCommand>();
			builder.RegisterType<GroupPersonBuilderForOptimizationFactory>().As<IGroupPersonBuilderForOptimizationFactory>();
			builder.RegisterType<MatrixListFactory>().As<IMatrixListFactory>();
			builder.RegisterType<TeamBlockScheduleCommand>().As<ITeamBlockScheduleCommand>();
			builder.RegisterType<TeamBlockOptimizationCommand>().As<ITeamBlockOptimizationCommand>();
			builder.RegisterType<WeeklyRestSolverCommand>().As<IWeeklyRestSolverCommand>();
			builder.RegisterType<BackToLegalShiftCommand>().As<BackToLegalShiftCommand>();
		    builder.RegisterType<IntraIntervalOptimizationCommand>().As<IIntraIntervalOptimizationCommand>();
	    }
	}
}
