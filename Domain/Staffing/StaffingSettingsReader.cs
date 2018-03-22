using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Staffing
{
	public static class KeyNames
	{
		public const string StaffingReadModelNumberOfDays = "StaffingReadModelNumberOfDays";
		public const string StaffingReadModelHistoricalHours = "StaffingReadModelHistoricalHours";
	}

	public class FakeStaffingSettingsReader : IStaffingSettingsReader
	{
		public Dictionary<string, int> StaffingSettings = new Dictionary<string, int>();

		public int GetIntSetting(string setting, int defaultValue)
		{
			if (!StaffingSettings.TryGetValue(setting, out var value))
				value = defaultValue;

			return value;
		}
	}

	public class StaffingSettingsReader : IStaffingSettingsReader
	{
		
		public int GetIntSetting(string setting, int defaultValue)
		{
			switch (setting)
			{
				case KeyNames.StaffingReadModelNumberOfDays:
					return 14;
				case KeyNames.StaffingReadModelHistoricalHours:
					return 8*24;
			}

			return defaultValue;
		}
	}

	public class StaffingSettingsReader28Days : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			switch (setting)
			{
				case KeyNames.StaffingReadModelNumberOfDays:
					return 28;
				case KeyNames.StaffingReadModelHistoricalHours:
					return 8*24;
			}

			return defaultValue;
		}
	}

	public class StaffingSettingsReader49Days : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			switch (setting)
			{
				case KeyNames.StaffingReadModelNumberOfDays:
					return 49;
				case KeyNames.StaffingReadModelHistoricalHours:
					return 8*24;
			}

			return defaultValue;
		}
	}

}
