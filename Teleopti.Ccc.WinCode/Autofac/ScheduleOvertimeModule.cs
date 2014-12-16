using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.WinCode.Autofac
{
	public class ScheduleOvertimeModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ScheduleOvertimeService>().As<IScheduleOvertimeService>();
			builder.RegisterType<CalculateBestOvertime>().As<CalculateBestOvertime>();
			builder.RegisterType<CalculateBestOvertimeBeforeOrAfter>().As<CalculateBestOvertimeBeforeOrAfter>();
			builder.Register(c => c.Resolve<IToggleManager>().IsEnabled(Toggles.Schedule_OvertimeBeforeShiftStart_30712)
				? (ICalculateBestOvertime)c.Resolve<CalculateBestOvertimeBeforeOrAfter>()
				: c.Resolve<CalculateBestOvertime>())
				.As<ICalculateBestOvertime>();

			builder.RegisterType<OvertimeDateTimePeriodExtractor>().As<IOvertimeDateTimePeriodExtractor>();
			builder.RegisterType<OvertimeRelativeDifferenceCalculator>().As<IOvertimeRelativeDifferenceCalculator>();
		}
	}
}
