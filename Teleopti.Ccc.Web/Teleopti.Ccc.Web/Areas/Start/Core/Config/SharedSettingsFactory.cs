using System;
using System.Configuration;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Config
{
	public class SharedSettingsFactory : ISharedSettingsFactory
	{
		private readonly IConfigReader _configReader;
		private readonly ILoadPasswordPolicyService _passwordPolicyService;

		public SharedSettingsFactory(IConfigReader configReader, ILoadPasswordPolicyService passwordPolicyService)
		{
			_configReader = configReader;
			_passwordPolicyService = passwordPolicyService;
		}

		public SharedSettings Create()
		{
			var connectionString = _configReader.ConnectionString("Queue");
			var hangfireConnection = _configReader.ConnectionString("Hangfire");
			return new SharedSettings
			{
				MessageBroker = _configReader.AppSettings_DontUse["MessageBroker"],
				MessageBrokerLongPolling = _configReader.AppSettings_DontUse["MessageBrokerLongPolling"],
				RtaPollingInterval = _configReader.AppSettings_DontUse["RtaPollingInterval"],
				Queue = connectionString == null ? string.Empty : Encryption.EncryptStringToBase64(connectionString, EncryptionConstants.Image1, EncryptionConstants.Image2),
				Hangfire = hangfireConnection == null ? string.Empty : Encryption.EncryptStringToBase64(hangfireConnection, EncryptionConstants.Image1, EncryptionConstants.Image2),
				PasswordPolicy = _passwordPolicyService.DocumentAsString,
				NumberOfDaysToShowNonPendingRequests = Convert.ToInt32(_configReader.AppSettings_DontUse["NumberOfDaysToShowNonPendingRequests"]),
				MessageBrokerMailboxPollingIntervalInSeconds = Convert.ToInt32(_configReader.AppSettings_DontUse["MessageBrokerMailboxPollingIntervalInSeconds"]),
				MessageBrokerMailboxExpirationInSeconds = Convert.ToInt32(_configReader.AppSettings_DontUse["MessageBrokerMailboxExpirationInSeconds"])
			};
		}
	}
}