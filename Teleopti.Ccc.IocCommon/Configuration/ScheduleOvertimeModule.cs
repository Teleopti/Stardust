using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class ScheduleOvertimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleOvertimeService>().As<IScheduleOvertimeService>();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>().As<ICalculateBestOvertime>();
			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			builder.RegisterType<OvertimeRelativeDifferenceCalculator>().As<IOvertimeRelativeDifferenceCalculator>();
		}
	}
}
