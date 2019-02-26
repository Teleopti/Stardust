using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public static class TimeZoneGuardForDesktop
	{
		//Use SchedulingScreen._timeZoneGuard or inject ITimeZoneGuard instead!
		public static ITimeZoneGuard Instance_DONTUSE { get; private set; }

		public static void Set(ITimeZoneGuard timeZoneGuard)
		{
			Instance_DONTUSE = timeZoneGuard;
		}
	}
}