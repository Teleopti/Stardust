using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public static class ActualAgentStateExtension
	{
		public static AgentStateReadModel DeseralizeActualAgentState(this Interfaces.MessageBroker.Notification notification)
		{
			var stateBytes = Convert.FromBase64String(notification.BinaryData);
			return JsonConvert.DeserializeObject<AgentStateReadModel>(Encoding.UTF8.GetString(stateBytes));
		}

	}
}