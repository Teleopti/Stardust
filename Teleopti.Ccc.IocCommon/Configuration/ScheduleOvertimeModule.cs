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
			builder.RegisterType<ScheduleOvertimeService>();
			builder.RegisterType<ScheduleOvertimeServiceWithoutStateholder>();
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().SingleInstance();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>();
			builder.RegisterType<OvertimePeriodValueMapper>().As<IOvertimePeriodValueMapper>();
			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			if (_configuration.Toggle(Toggles.ResourcePlanner_OvertimeNightShifts_44311))
			{
				builder.RegisterType<OvertimeRelativeDifferenceCalculator>().SingleInstance();
			}
			else
			{
				builder.RegisterType<OvertimeRelativeDifferenceCalculatorOLD>().As<OvertimeRelativeDifferenceCalculator>().SingleInstance();
			}
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().SingleInstance();
			builder.RegisterType<AddOverTime>();
			builder.RegisterType<PersonForOvertimeProvider>().As<IPersonForOvertimeProvider>().SingleInstance();
			builder.RegisterType<PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryOrAllPersonSkillForNonOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryGroupPersonSkillAggregator>().SingleInstance();
			builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<ScheduleOvertimeExecuteWrapper>();
		}
	}
}
