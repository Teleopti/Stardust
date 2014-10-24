using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class ActualAgentStateForTest : ActualAgentState
	{
		public ActualAgentStateForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			OriginalDataSourceId = "0";
		}
	}
}