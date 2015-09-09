using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Data shared by hole application
	/// </summary>
	public interface IApplicationData : IDisposable
	{
		IMessageBrokerComposite Messaging { get; }

		//Do not use this one - should be removed!
		IDictionary<string, string> AppSettings { get; }

		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }
	}
}