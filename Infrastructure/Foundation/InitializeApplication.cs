using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		public InitializeApplication(IMessageBrokerComposite messaging)
		{
			Messaging = messaging;
		}

		public IMessageBrokerComposite Messaging { get; private set; }

		public void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings)
		{
			StateHolder.Initialize(clientCache, Messaging);
			StateHolder
				.Instance
				.State
				.SetApplicationData(
					new ApplicationData(
						appSettings,
						loadPasswordPolicyService
						)
				);
		}
	}
}