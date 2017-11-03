using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Infrastructure.Requests
{
	public static class StaffingInfoAvailableDaysProvider
	{
		public static int GetDays(IToggleManager toggleManager)
		{
			int staffingAvailableDays;

			if (toggleManager.IsEnabled(Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109))
				staffingAvailableDays = 48;
			else if (toggleManager.IsEnabled(Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109))
				staffingAvailableDays = 27;
			else
				staffingAvailableDays = 13;

			return staffingAvailableDays;
		}
	}
}