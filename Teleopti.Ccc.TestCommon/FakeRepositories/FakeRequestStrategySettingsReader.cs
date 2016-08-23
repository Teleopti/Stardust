using Teleopti.Ccc.Domain.AgentInfo.Requests;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	public class FakeRequestStrategySettingsReader : IRequestStrategySettingsReader
	{
		public int GetIntSetting(string setting, int defaultValue)
		{
			return defaultValue;
		}
	}
}