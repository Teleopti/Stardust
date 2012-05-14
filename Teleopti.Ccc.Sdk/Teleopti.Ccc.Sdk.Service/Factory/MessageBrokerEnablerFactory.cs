using System;
using Teleopti.Ccc.Sdk.Logic;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class MessageBrokerEnablerFactory : IMessageBrokerEnablerFactory
	{
		public IDisposable NewMessageBrokerEnabler()
		{
			return new MessageBrokerSendEnabler();
		}
	}
}