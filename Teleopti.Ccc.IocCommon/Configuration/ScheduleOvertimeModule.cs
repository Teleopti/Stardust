using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Overtime;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ScheduleOvertimeModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public ScheduleOvertimeModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleOvertimeService>().As<IScheduleOvertimeService>();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>().As<ICalculateBestOvertime>();
			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			builder.RegisterType<OvertimeRelativeDifferenceCalculator>().As<IOvertimeRelativeDifferenceCalculator>();
			if (_configuration.Toggle(Toggles.ResourcePlanner_CascadingScheduleOvertimeOnPrimary_41318))
			{
				builder.RegisterType<PersonSkillsUsePrimaryForScheduleDaysOvertimeProvider>().As<IPersonSkillsForScheduleDaysOvertimeProvider>().SingleInstance();
				builder.RegisterType<PrimaryPersonSkillForNonOvertimeProvider>().As<IPersonSkillsForNonOvertimeProvider>().SingleInstance();
				builder.RegisterType<PrimaryGroupPersonSkillAggregator>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().As<IPersonSkillsForScheduleDaysOvertimeProvider>().SingleInstance();
				builder.RegisterType<AllPersonSkillForNonOvertimeProvider>().As<IPersonSkillsForNonOvertimeProvider>().SingleInstance();
			}
		}
	}
}
