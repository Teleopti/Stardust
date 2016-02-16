using System;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateMessageSender
	{
		void Send(StateInfo state);
	}

	public class NoMessage : IAgentStateMessageSender
	{
		public void Send(StateInfo state)
		{
		}
	}

	public class AgentStateMessageSender : IAgentStateMessageSender
	{
		private readonly IMessageSender _messageSender;
		private readonly IJsonSerializer _jsonSerializer;

		public AgentStateMessageSender(IMessageSender messageSender, IJsonSerializer jsonSerializer)
		{
			_messageSender = messageSender;
			_jsonSerializer = jsonSerializer;
		}

		public void Send(StateInfo state)
		{
			if (!state.Send)
				return;

			var actualAgentState = state.MakeAgentStateReadModel();

			var type = typeof(AgentStateReadModel);
			var notification = new Message
			{
				StartDate = Subscription.DateToString(actualAgentState.ReceivedTime),
				EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
				DomainId = Subscription.IdToString(actualAgentState.PersonId),
				DomainType = type.Name,
				DomainQualifiedType = type.AssemblyQualifiedName,
				ModuleId = Subscription.IdToString(Guid.Empty),
				DomainUpdateType = (int)DomainUpdateType.Insert,
				BusinessUnitId = Subscription.IdToString(actualAgentState.BusinessUnitId)
			};
			notification.SeralizeActualAgentState(_jsonSerializer, actualAgentState);

			_messageSender.Send(notification);
		}
	}

}