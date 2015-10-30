using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class InitializeApplication : IInitializeApplication
	{
		private readonly IMessageBrokerComposite _messageBroker;

		public InitializeApplication(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings)
		{
			StateHolder.Initialize(clientCache);

			var appData = new ApplicationData(appSettings, _messageBroker, loadPasswordPolicyService);
			StateHolder.Instance.State.SetApplicationData(appData);
		}
	}
}