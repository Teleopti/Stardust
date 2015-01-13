using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public static class ActualAgentStateExtension
	{
		public static AgentStateReadModel DeseralizeActualAgentState(this Notification notification)
		{
			var stateBytes = Convert.FromBase64String(notification.BinaryData);
			return JsonConvert.DeserializeObject<AgentStateReadModel>(Encoding.UTF8.GetString(stateBytes));
		}

		public static void SeralizeActualAgentState(this Notification notification, AgentStateReadModel agentStateReadModel)
		{
			var domainObject = JsonConvert.SerializeObject(agentStateReadModel);
			notification.BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject));
		}

	}
}