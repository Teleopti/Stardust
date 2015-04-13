using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntraIntervalSolverServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntraIntervalFinder>().As<IIntraIntervalFinder>().SingleInstance();
			builder.RegisterType<SkillDayIntraIntervalFinder>().As<ISkillDayIntraIntervalFinder>().SingleInstance();
			builder.RegisterType<IntraIntervalFinderService>().SingleInstance();
			builder.RegisterType<IntraIntervalFinderServiceToggle29845Off>().SingleInstance();
			builder.RegisterType<SkillActivityCounter>().As<ISkillActivityCounter>().SingleInstance();
			builder.RegisterType<SkillActivityCountCollector>().As<ISkillActivityCountCollector>().SingleInstance();
			builder.RegisterType<FullIntervalFinder>().As<IFullIntervalFinder>().SingleInstance();

			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_IntraIntervalSolver_29845)
			   ? (IIntraIntervalFinderService)c.Resolve<IntraIntervalFinderService>()
			   : c.Resolve<IntraIntervalFinderServiceToggle29845Off>())
				   .As<IIntraIntervalFinderService>();

			//IFullIntervalFinder
		}
	}
}