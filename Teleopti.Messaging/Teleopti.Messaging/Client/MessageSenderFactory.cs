﻿using System;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Messaging.Client
{
	public static class MessageSenderFactory
	{
		public static IMessageSender CreateMessageSender(string connectionString)
		{
			Uri serverUrl;
			if (Uri.TryCreate(connectionString,UriKind.Absolute,out serverUrl))
			{
				return new SignalSender(connectionString);
			}
			return new MessageSender(connectionString);
		}
	}
}