using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationData : IApplicationData
	{
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;

		public ApplicationData(IDictionary<string, string> appSettings,
			IMessageBrokerComposite messageBroker,
			ILoadPasswordPolicyService loadPasswordPolicyService,
			IDataSourceForTenant dataSourceForTenant)
		{
			AppSettings = appSettings;
			_messageBroker = messageBroker;
			_loadPasswordPolicyService = loadPasswordPolicyService;
			DataSourceForTenant = dataSourceForTenant;
		}

		public ILoadPasswordPolicyService LoadPasswordPolicyService
		{
			get { return _loadPasswordPolicyService; }
		}

		public IDataSourceForTenant DataSourceForTenant { get; }

		public IMessageBrokerComposite Messaging
		{
			get { return _messageBroker; }
		}

		public IDictionary<string, string> AppSettings { get; private set; }

		public void Dispose()
		{
			DataSourceForTenant.Dispose();
			if (Messaging != null)
			{
				Messaging.Dispose();
			}
		}
	}
}