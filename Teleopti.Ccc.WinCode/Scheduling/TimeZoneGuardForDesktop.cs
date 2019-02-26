using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public static class TimeZoneGuardForDesktop
	{
#pragma warning disable 618
		//Use ITimeZoneGuard or field in schedulingscreen instead
		public static ITimeZoneGuard Instance_DONTUSE { get; private set; }
#pragma warning restore 618

		public static void Set(ITimeZoneGuard timeZoneGuard)
		{
			Instance_DONTUSE = timeZoneGuard;
		}
	}
}