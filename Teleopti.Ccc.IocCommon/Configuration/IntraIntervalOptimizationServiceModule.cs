using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntraIntervalOptimizationServiceModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public IntraIntervalOptimizationServiceModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntraIntervalOptimizer>().As<IIntraIntervalOptimizer>();
			builder.RegisterType<ScheduleDayIntraIntervalIssueExtractor>().As<IScheduleDayIntraIntervalIssueExtractor>().SingleInstance();
			builder.RegisterType<SkillDayIntraIntervalIssueExtractor>().As<ISkillDayIntraIntervalIssueExtractor>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodEvaluator>().As<ISkillStaffPeriodEvaluator>().SingleInstance();
			builder.RegisterType<IntraIntervalOptimizationService>().As<IIntraIntervalOptimizationService>().InstancePerDependency();
			builder.RegisterType<IntraIntervalIssueCalculator>().As<IIntraIntervalIssueCalculator>().SingleInstance();
			builder.RegisterType<ShiftProjectionCacheIntraIntervalValueCalculator>().As<IShiftProjectionCacheIntraIntervalValueCalculator>().SingleInstance();
			builder.RegisterType<ShiftProjectionIntraIntervalBestFitCalculator>().As<IShiftProjectionIntraIntervalBestFitCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodIntraIntervalPeriodFinder>().As<ISkillStaffPeriodIntraIntervalPeriodFinder>().SingleInstance();

			if (_configuration.Toggle(Toggles.ResourcePlanner_WeeklyRestSolver_35043))
			{
				builder.RegisterType<MainShiftOptimizeActivitySpecificationSetter>().As<IMainShiftOptimizeActivitySpecificationSetter>().SingleInstance();
			}
			else
			{
				builder.RegisterType<MainShiftOptimizeActivitySpecificationSetterOff>().As<IMainShiftOptimizeActivitySpecificationSetter>().SingleInstance();
			}
		}
	}
}
