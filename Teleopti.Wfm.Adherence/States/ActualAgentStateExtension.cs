using System;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Wfm.Adherence.States
{
	public static class ActualAgentStateExtension
	{
		public static void SeralizeActualAgentState(this Message message, IJsonSerializer serializer, AgentStateReadModel agentStateReadModel)
		{
			var domainObject = serializer.SerializeObject(agentStateReadModel);
			message.BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject));
		}

	}
}