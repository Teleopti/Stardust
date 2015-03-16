using System;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public static class ActualAgentStateExtension
	{
		public static void SeralizeActualAgentState(this Interfaces.MessageBroker.Notification notification, IJsonSerializer serializer, AgentStateReadModel agentStateReadModel)
		{
			var domainObject = serializer.SerializeObject(agentStateReadModel);
			notification.BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject));
		}

	}
}