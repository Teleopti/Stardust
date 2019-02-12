using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public static class TimeZoneGuardForDesktop
	{
#pragma warning disable 618
		//Use ITimeZoneGuard or field in schedulingscreen instead
		public static ITimeZoneGuard Instance_DONTUSE => ServiceLocator_DONTUSE.TimeZoneGuard_DONOTCALLTHIS_JUSTHERETEMP_FOR_DESKTOP;
#pragma warning restore 618
	}
}