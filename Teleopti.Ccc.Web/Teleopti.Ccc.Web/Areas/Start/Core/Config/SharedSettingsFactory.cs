using System;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Config
{
	public class SharedSettingsFactory : ISharedSettingsFactory
	{
		private readonly IConfigReader _configReader;
		private readonly ILoadPasswordPolicyService _passwordPolicyService;
		private readonly IApplicationInsightsConfigurationReader _applicationInsightsConfigurationReader;

		public SharedSettingsFactory(IConfigReader configReader, ILoadPasswordPolicyService passwordPolicyService,
			IApplicationInsightsConfigurationReader applicationInsightsConfigurationReader)
		{
			_configReader = configReader;
			_passwordPolicyService = passwordPolicyService;
			_applicationInsightsConfigurationReader = applicationInsightsConfigurationReader;
		}

		public SharedSettings Create()
		{
			var hangfireConnection = _configReader.ConnectionString("Hangfire");
			var instrumentationKey = _applicationInsightsConfigurationReader.InstrumentationKey();
			return new SharedSettings
			{
				MessageBroker = _configReader.AppConfig("MessageBroker"),
				MessageBrokerLongPolling = _configReader.AppConfig("MessageBrokerLongPolling"),
				RtaPollingInterval = _configReader.AppConfig("RtaPollingInterval"),
				Hangfire = hangfireConnection == null
					? string.Empty
					: Encryption.EncryptStringToBase64(hangfireConnection, EncryptionConstants.Image1, EncryptionConstants.Image2),
				PasswordPolicy = _passwordPolicyService.DocumentAsString,
				NumberOfDaysToShowNonPendingRequests =
					Convert.ToInt32(_configReader.AppConfig("NumberOfDaysToShowNonPendingRequests")),
				MessageBrokerMailboxPollingIntervalInSeconds =
					Convert.ToInt32(_configReader.AppConfig("MessageBrokerMailboxPollingIntervalInSeconds")),
				MessageBrokerMailboxExpirationInSeconds =
					Convert.ToInt32(_configReader.AppConfig("MessageBrokerMailboxExpirationInSeconds")),
				InstrumentationKey = instrumentationKey
					//Encryption.EncryptStringToBase64(instrumentationKey, EncryptionConstants.Image1, EncryptionConstants.Image2)
			};
		}
	}
}