using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
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