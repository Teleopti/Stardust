using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Rta.Service;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	public class AgentStateReadModelForTest : AgentStateReadModel
	{
		public AgentStateReadModelForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			IsRuleAlarm = true;
			BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value;
		}
	}
}