using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	public class AgentStateReadModelForTest : AgentStateReadModel
	{
		public AgentStateReadModelForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			IsRuleAlarm = true;
		}
	}
}