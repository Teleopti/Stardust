namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IStaffingSettingsReader
	{
		int GetIntSetting(string setting, int defaultValue);
	}
}
