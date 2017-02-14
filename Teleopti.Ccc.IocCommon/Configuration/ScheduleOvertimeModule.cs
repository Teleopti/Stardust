using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;

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
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().SingleInstance();
			builder.RegisterType<AddOverTime>().As<IAddOverTime>().SingleInstance();
			builder.RegisterType<CalculateOvertimeSuggestionProvider>().As<CalculateOvertimeSuggestionProvider>().SingleInstance();
			builder.RegisterType<PersonForOvertimeProvider>().As<IPersonForOvertimeProvider>().SingleInstance();

			if (_configuration.Toggle(Toggles.ResourcePlanner_CascadingScheduleOvertimeOnPrimary_41318))
			{
				builder.RegisterType<PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider>().As<IPersonSkillsForScheduleDaysOvertimeProvider>().SingleInstance();
				builder.RegisterType<PrimaryOrAllPersonSkillForNonOvertimeProvider>().As<IPersonSkillsForNonOvertimeProvider>().SingleInstance();
				builder.RegisterType<PrimaryGroupPersonSkillAggregator>().SingleInstance();
				builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().As<IPersonSkillsForScheduleDaysOvertimeProvider>().SingleInstance();
				builder.RegisterType<AllPersonSkillForNonOvertimeProvider>().As<IPersonSkillsForNonOvertimeProvider>().SingleInstance();
			}
		}
	}
}
