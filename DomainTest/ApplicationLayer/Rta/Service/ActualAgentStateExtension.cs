using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	public static class ActualAgentStateExtension
	{
		public static AgentStateReadModel DeseralizeActualAgentState(this Message message)
		{
			var stateBytes = Convert.FromBase64String(message.BinaryData);
			return JsonConvert.DeserializeObject<AgentStateReadModel>(Encoding.UTF8.GetString(stateBytes));
		}

	}
}