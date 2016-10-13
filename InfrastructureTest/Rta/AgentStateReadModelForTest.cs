using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class HistoricalAdherenceReadModelForTest : HistoricalAdherenceReadModel
	{
		public HistoricalAdherenceReadModelForTest()
		{
			RandomName.Make();
		}
	}

	public class AgentStateReadModelForTest : AgentStateReadModel
	{
		public AgentStateReadModelForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			IsRuleAlarm = true;
		}
	}
}