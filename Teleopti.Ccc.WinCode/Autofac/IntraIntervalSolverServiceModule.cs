using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.WinCode.Autofac
{
	public class IntraIntervalSolverServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntraIntervalFinder>().As<IIntraIntervalFinder>();
			builder.RegisterType<SkillDayIntraIntervalFinder>().As<ISkillDayIntraIntervalFinder>();
			builder.RegisterType<IntraIntervalFinderService>().As<IntraIntervalFinderService>();
			builder.RegisterType<IntraIntervalFinderServiceToggle29845Off>().As<IntraIntervalFinderServiceToggle29845Off>();

			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Scheduler_IntraIntervalSolver_29845)
			   ? (IIntraIntervalFinderService)c.Resolve<IntraIntervalFinderService>()
			   : c.Resolve<IntraIntervalFinderServiceToggle29845Off>())
				   .As<IIntraIntervalFinderService>();

			//IIntraIntervalFinderService
		}
	}
}