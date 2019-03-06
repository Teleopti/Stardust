using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Infrastructure.Requests
{
	public static class StaffingInfoAvailableDaysProvider
	{
		public static int GetDays(IToggleManager toggleManager)
		{
			var staffingAvailableDays = 48;
			return staffingAvailableDays;
		}
	}
}