using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private readonly IMessageBrokerComposite _messaging;

		public InitializeApplication(IMessageBrokerComposite messaging)
		{
			_messaging = messaging;
		}

		public void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings)
		{
			StateHolder.Initialize(clientCache, _messaging);
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