using System;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.MessagingTest.SignalR
{
	public class MessageFilterManagerFake : IMessageFilterManager
	{
		public bool HasType(Type type)
		{
			return true;
		}

		public string LookupTypeToSend(Type domainObjectType)
		{
			return "string";
		}

		public Type LookupType(Type domainObjectType)
		{
			return typeof(string);
		}
	}
}