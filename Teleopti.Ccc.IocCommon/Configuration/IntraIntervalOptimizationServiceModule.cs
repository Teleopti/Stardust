using Autofac;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class IntraIntervalOptimizationServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntraIntervalOptimizer>();
			builder.RegisterType<ScheduleDayIntraIntervalIssueExtractor>().SingleInstance();
			builder.RegisterType<SkillDayIntraIntervalIssueExtractor>().As<ISkillDayIntraIntervalIssueExtractor>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodEvaluator>().SingleInstance();
			builder.RegisterType<IntraIntervalOptimizationService>().InstancePerDependency();
			builder.RegisterType<IntraIntervalIssueCalculator>().SingleInstance();
			builder.RegisterType<ShiftProjectionCacheIntraIntervalValueCalculator>().As<IShiftProjectionCacheIntraIntervalValueCalculator>().SingleInstance();
			builder.RegisterType<ShiftProjectionIntraIntervalBestFitCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffPeriodIntraIntervalPeriodFinder>().As<ISkillStaffPeriodIntraIntervalPeriodFinder>().SingleInstance();
			builder.RegisterType<MainShiftOptimizeActivitySpecificationSetter>().As<IMainShiftOptimizeActivitySpecificationSetter>().SingleInstance();
		}
	}
}
