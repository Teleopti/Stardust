using Autofac;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ScheduleOvertimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleOvertimeService>().As<IScheduleOvertimeService>();
			builder.RegisterType<ScheduleOvertimeServiceWithoutStateholder>().As<IScheduleOvertimeServiceWithoutStateholder>();
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().SingleInstance();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>().As<ICalculateBestOvertime>();
			builder.RegisterType<OvertimePeriodValueMapper>().As<IOvertimePeriodValueMapper>();
			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			builder.RegisterType<OvertimeRelativeDifferenceCalculator>().As<IOvertimeRelativeDifferenceCalculator>();
			builder.RegisterType<ScheduleOvertimeWithoutStateHolder>().SingleInstance();
			builder.RegisterType<AddOverTime>().As<IAddOverTime>().SingleInstance();
			builder.RegisterType<PersonForOvertimeProvider>().As<IPersonForOvertimeProvider>().SingleInstance();
			builder.RegisterType<PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryOrAllPersonSkillForNonOvertimeProvider>().SingleInstance();
			builder.RegisterType<PrimaryGroupPersonSkillAggregator>().SingleInstance();
			builder.RegisterType<PersonSkillsUseAllForScheduleDaysOvertimeProvider>().SingleInstance();
			builder.RegisterType<ScheduleOvertimeExecuteWrapper>().SingleInstance();
		}
	}
}
