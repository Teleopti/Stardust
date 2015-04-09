using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class IntraIntervalOptimizationServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<IntraIntervalOptimizer>().As<IIntraIntervalOptimizer>();
			builder.RegisterType<ScheduleDayIntraIntervalIssueExtractor>().As<IScheduleDayIntraIntervalIssueExtractor>();
			builder.RegisterType<SkillDayIntraIntervalIssueExtractor>().As<ISkillDayIntraIntervalIssueExtractor>();
			builder.RegisterType<SkillStaffPeriodEvaluator>().As<ISkillStaffPeriodEvaluator>();
			builder.RegisterType<IntraIntervalOptimizationService>().As<IntraIntervalOptimizationService>();
			builder.RegisterType<IntraIntervalOptimizationServiceToggle29846Off>().As<IntraIntervalOptimizationServiceToggle29846Off>();
			builder.RegisterType<IntraIntervalIssueCalculator>().As<IIntraIntervalIssueCalculator>();
			builder.RegisterType<ShiftProjectionCacheIntraIntervalValueCalculator>().As<IShiftProjectionCacheIntraIntervalValueCalculator>();
			builder.RegisterType<ShiftProjectionIntraIntervalBestFitCalculator>().As<IShiftProjectionIntraIntervalBestFitCalculator>();
			builder.RegisterType<SkillStaffPeriodIntraIntervalPeriodFinder>().As<ISkillStaffPeriodIntraIntervalPeriodFinder>();

			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Schedule_IntraIntervalOptimizer_29846)
			   ? (IIntraIntervalOptimizationService)c.Resolve<IntraIntervalOptimizationService>()
			   : c.Resolve<IntraIntervalOptimizationServiceToggle29846Off>())
				   .As<IIntraIntervalOptimizationService>();
		}
	}
}
