using System;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.MessagingTest.SignalR.ImplementationDetailsTests.TestDoubles
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