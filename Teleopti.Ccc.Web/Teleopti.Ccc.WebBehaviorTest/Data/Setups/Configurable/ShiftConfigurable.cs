﻿using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class ShiftConfigurable : TestCommon.TestData.Setups.Configurable.ShiftConfigurable
	{
		public ShiftConfigurable()
		    var timeZone = user.PermissionInformation.DefaultTimeZone();
		    var startTimeUtc = timeZone.SafeConvertTimeToUtc(StartTime);
			var endTimeUtc = timeZone.SafeConvertTimeToUtc(EndTime);
				var lunchStartTimeUtc = timeZone.SafeConvertTimeToUtc(LunchStartTime);
				var lunchEndTimeUtc = timeZone.SafeConvertTimeToUtc(LunchEndTime);
		{
			Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario.Description.Name;
			Activity = TestData.ActivityPhone.Name;
			LunchActivity = TestData.ActivityLunch.Name;
		}
	}
}