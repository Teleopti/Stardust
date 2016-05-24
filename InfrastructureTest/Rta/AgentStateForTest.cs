using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class AgentStateForTest : AgentState
	{
		public AgentStateForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			SourceId = "0";
		}
	}
}