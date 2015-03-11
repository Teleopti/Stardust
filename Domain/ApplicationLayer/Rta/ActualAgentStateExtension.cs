using System;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public static class ActualAgentStateExtension
	{
		public static void SeralizeActualAgentState(this Notification notification, IJsonSerializer serializer, AgentStateReadModel agentStateReadModel)
		{
			var domainObject = serializer.SerializeObject(agentStateReadModel);
			notification.BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject));
		}

	}
}