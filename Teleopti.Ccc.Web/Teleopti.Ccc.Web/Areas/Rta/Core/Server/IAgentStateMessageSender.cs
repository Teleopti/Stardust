using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public interface IAgentStateMessageSender
	{
		void Send(StateInfo state);
	}

	public class NoMessagge : IAgentStateMessageSender
	{
		public void Send(StateInfo state)
		{
		}
	}

	public class AgentStateMessageSender : IAgentStateMessageSender
	{
		private readonly IMessageSender _messageSender;

		public AgentStateMessageSender(IMessageSender messageSender)
		{
			_messageSender = messageSender;
		}

		public void Send(StateInfo state)
		{
			var actualAgentState = state.MakeActualAgentState();

			var type = typeof(IActualAgentState);
			var notification = new Notification
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
			notification.SeralizeActualAgentState(actualAgentState);

			_messageSender.Send(notification);
		}
	}

}