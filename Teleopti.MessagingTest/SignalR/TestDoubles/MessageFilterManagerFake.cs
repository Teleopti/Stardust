using System;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class MessageFilterManagerFake : IMessageFilterManager
	{
		public bool HasType(Type type)
		{
			return true;
		}

		public string LookupTypeToSend(Type domainObjectType)
		{
			return domainObjectType.AssemblyQualifiedName;
		}

		public Type LookupType(Type domainObjectType)
		{
			return domainObjectType;
		}
	}
}