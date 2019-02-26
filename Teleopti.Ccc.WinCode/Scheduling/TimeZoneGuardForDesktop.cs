using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public static class TimeZoneGuardForDesktop
	{
		//Use ITimeZoneGuard or field in schedulingscreen instead
		public static ITimeZoneGuard Instance_DONTUSE { get; private set; }

		public static void Set(ITimeZoneGuard timeZoneGuard)
		{
			Instance_DONTUSE = timeZoneGuard;
		}
	}
}