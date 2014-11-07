using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public static class ActualAgentStateExtension
	{
		public static ActualAgentState DeseralizeActualAgentState(this Notification notification)
		{
			var stateBytes = Convert.FromBase64String(notification.BinaryData);
			return JsonConvert.DeserializeObject<ActualAgentState>(Encoding.UTF8.GetString(stateBytes));
		}

		public static void SeralizeActualAgentState(this Notification notification, IActualAgentState actualAgentState)
		{
			var domainObject = JsonConvert.SerializeObject(actualAgentState);
			notification.BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject));
		}

	}
}