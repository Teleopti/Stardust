using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class HistoricalAdherenceReadModelForTest : HistoricalAdherenceReadModel
	{
		public HistoricalAdherenceReadModelForTest()
		{
			RandomName.Make();
			Date = DateOnly.Today;
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