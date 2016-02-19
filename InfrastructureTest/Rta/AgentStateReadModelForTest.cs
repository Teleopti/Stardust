using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class AgentStateReadModelForTest : AgentStateReadModel
	{
		public AgentStateReadModelForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			OriginalDataSourceId = "0";
			IsAlarm = true;
		}
	}
}