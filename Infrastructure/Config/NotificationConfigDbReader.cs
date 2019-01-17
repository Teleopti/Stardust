using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Infrastructure.Config
{
	public class NotificationConfigDbReader : INotificationConfigReader
	{
		private readonly IServerConfigurationRepository _serverRepo;
		private readonly Dictionary<ServerConfigurationKey, string> _config = new Dictionary<ServerConfigurationKey, string>();

		private static readonly object LockObj = new object();

		public NotificationConfigDbReader(IServerConfigurationRepository serverRepo)
		{
			_serverRepo = serverRepo;
		}

		private void loadConfig()
		{
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderUrl);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderUser);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderPassword);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderFrom);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderClass);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderAssembly);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderApiId);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderData);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderFindSuccessOnError);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderErrorCode);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderSuccessCode);
			addConfigEntry(ServerConfigurationKey.NotificationExtProviderSkipSearch);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpEnabled);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpHost);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpPort);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpUseSsl);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpUsername);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpPassword);
			addConfigEntry(ServerConfigurationKey.NotificationSmtpUseRelay);
			addConfigEntry(ServerConfigurationKey.NotificationContentType);
			addConfigEntry(ServerConfigurationKey.NotificationEncodingName);
			HasLoadedConfig = true;
		}

		private void addConfigEntry(ServerConfigurationKey key)
		{
			var dbValue = _serverRepo.Get(key.ToString()) ?? string.Empty;
			_config.Add(key, dbValue);
		}

		public bool HasLoadedConfig { get; private set; }

		private string readConfig(ServerConfigurationKey key)
		{
			string val;
			if (_config.TryGetValue(key, out val)) return val;

			lock (LockObj)
			{
				if (!HasLoadedConfig)
				{
					loadConfig();
				}
			}

			_config.TryGetValue(key, out val);
			return val;
		}

		public Uri Url
		{
			get
			{
				var dbValue = readConfig(ServerConfigurationKey.NotificationExtProviderUrl);
				return string.IsNullOrEmpty(dbValue) ? null : new Uri(dbValue);
			}
		}

		public string User => readConfig(ServerConfigurationKey.NotificationExtProviderUser);

		public string Password => readConfig(ServerConfigurationKey.NotificationExtProviderPassword);

		public string From => readConfig(ServerConfigurationKey.NotificationExtProviderFrom);

		public string ClassName => readConfig(ServerConfigurationKey.NotificationExtProviderClass);

		public string Assembly => readConfig(ServerConfigurationKey.NotificationExtProviderAssembly);

		public string Api => readConfig(ServerConfigurationKey.NotificationExtProviderApiId);

		public string Data => readConfig(ServerConfigurationKey.NotificationExtProviderData);

		public string FindSuccessOrError => readConfig(ServerConfigurationKey.NotificationExtProviderFindSuccessOnError);

		public string ErrorCode => readConfig(ServerConfigurationKey.NotificationExtProviderErrorCode);

		public string SuccessCode => readConfig(ServerConfigurationKey.NotificationExtProviderSuccessCode);

		public bool SkipSearch => bool.TryParse(readConfig(ServerConfigurationKey.NotificationExtProviderSkipSearch), out var value) && value;

		public string SmtpHost => readConfig(ServerConfigurationKey.NotificationSmtpHost);
		
		public bool SmtpEnabled => bool.TryParse(readConfig(ServerConfigurationKey.NotificationSmtpEnabled), out var value) && value;

		public int SmtpPort => int.TryParse(readConfig(ServerConfigurationKey.NotificationSmtpPort), out var value) ? value : -1;

		public bool SmtpUseSsl => bool.TryParse(readConfig(ServerConfigurationKey.NotificationSmtpUseSsl), out var value) && value;

		public string SmtpUser => readConfig(ServerConfigurationKey.NotificationSmtpUsername);

		public string SmtpPassword => readConfig(ServerConfigurationKey.NotificationSmtpPassword);

		public bool SmtpUseRelay => bool.TryParse(readConfig(ServerConfigurationKey.NotificationSmtpUseRelay), out var value) && value;

		public string ContentType => readConfig(ServerConfigurationKey.NotificationContentType);

		public string EncodingName => readConfig(ServerConfigurationKey.NotificationEncodingName);
	}
}