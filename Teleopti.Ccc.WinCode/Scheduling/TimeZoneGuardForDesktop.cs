using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public static class TimeZoneGuardForDesktop
	{
		public static ITimeZoneGuard Instance => ServiceLocator_DONTUSE.TimeZoneGuard;
	}
}