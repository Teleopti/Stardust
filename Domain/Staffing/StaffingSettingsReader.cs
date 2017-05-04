namespace Teleopti.Ccc.Domain.Staffing
{
	public class StaffingSettingsReader : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			return setting == "StaffingReadModelNumberOfDays" ? 14 : defaultValue;
		}
	}

	public class StaffingSettingsReader1Day : IStaffingSettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			return setting == "StaffingReadModelNumberOfDays" ? 1 : defaultValue;
		}
	}
}
