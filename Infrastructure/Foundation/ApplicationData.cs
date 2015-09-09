using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationData : IApplicationData
	{
		public ApplicationData(IDictionary<string, string> appSettings,
			IMessageBrokerComposite messageBroker,
			ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			AppSettings = appSettings;
			Messaging = messageBroker;
			LoadPasswordPolicyService = loadPasswordPolicyService;
		}

		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; private set; }

		public IMessageBrokerComposite Messaging { get; private set; }

		public IDictionary<string, string> AppSettings { get; private set; }

		public void Dispose()
		{
			if (Messaging != null)
			{
				Messaging.Dispose();
			}
		}
	}
}