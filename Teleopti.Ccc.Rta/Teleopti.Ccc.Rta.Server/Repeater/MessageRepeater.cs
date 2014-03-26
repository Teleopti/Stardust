using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public class MessageRepeater
	{
		private readonly IMessageSender _messageSender;
		private readonly ICreateNotification _createNotification;
		private readonly IDictionary<Guid, IActualAgentState> _actualAgentStates;

		public MessageRepeater(IMessageSender messageSender, 
			IMessageRepeaterTrigger messageRepeaterTrigger,
			ICreateNotification createNotification)
		{
			_actualAgentStates = new ConcurrentDictionary<Guid, IActualAgentState>();
			_messageSender = messageSender;
			_createNotification = createNotification;
			
			if (messageRepeaterTrigger != null)
			{
				messageRepeaterTrigger.Initialize(flush);
			}
		}

		public void Invoke(IActualAgentState message)
		{
			_actualAgentStates[message.PersonId] = message;
		}

		private void flush()
		{
			foreach (var actualAgentState in _actualAgentStates)
			{
				_messageSender.SendNotification(_createNotification.FromActualAgentState(actualAgentState.Value));
			}
			_actualAgentStates.Clear();
		}
	}
}