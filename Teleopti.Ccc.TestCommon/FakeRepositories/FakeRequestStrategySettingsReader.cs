using Teleopti.Ccc.Domain.AgentInfo.Requests;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeRequestStrategySettingsReader : IRequestStrategySettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			return defaultValue;
		}
	}
}