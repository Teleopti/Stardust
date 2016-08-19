namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestStrategySettingsReader
	{
		int GetIntSetting(string setting, int defaultValue);
	}
}