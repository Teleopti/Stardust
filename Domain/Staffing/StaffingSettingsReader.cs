namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingSettingsReader : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			switch (setting)
			{
				case "StaffingReadModelNumberOfDays":
					return 14;
				case "StaffingReadModelHistoricalHours":
					return 8*24;
			}

			return defaultValue;
		}
	}

	public class StaffingSettingsReaderNoHistorical : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			switch (setting)
			{
				case "StaffingReadModelNumberOfDays":
					return 14;
				case "StaffingReadModelHistoricalHours":
					return 25;
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
				case "StaffingReadModelNumberOfDays":
					return 28;
				case "StaffingReadModelHistoricalHours":
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
				case "StaffingReadModelNumberOfDays":
					return 49;
				case "StaffingReadModelHistoricalHours":
					return 8*24;
			}

			return defaultValue;
		}
	}

}
