using System;

namespace Teleopti.Ccc.Sdk.Logic
{
	public interface IMessageBrokerEnablerFactory
	{
		IDisposable NewMessageBrokerEnabler();
	}
}