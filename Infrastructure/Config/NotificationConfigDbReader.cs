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
		private readonly Dictionary<ServerConfigurationKey, string> _config;

		public NotificationConfigDbReader(IServerConfigurationRepository serverRepo)
		{
			_serverRepo = serverRepo;
			_config = new Dictionary<ServerConfigurationKey, string>();
			loadConfig();
			HasLoadedConfig = true;
		}

		private void loadConfig()
		{
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderUrl);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderUser);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderPassword);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderFrom);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderClass);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderAssembly);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderApiId);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderData);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderFindSuccessOnError);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderErrorCode);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderSuccessCode);
			AddConfigEntry(ServerConfigurationKey.NotificationExtProviderSkipSearch);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpHost);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpPort);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpUseSsl);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpUser);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpPassword);
			AddConfigEntry(ServerConfigurationKey.NotificationSmtpUseRelay);
			AddConfigEntry(ServerConfigurationKey.NotificationContentType);
			AddConfigEntry(ServerConfigurationKey.NotificationEncodingName);
		}

		private void AddConfigEntry(ServerConfigurationKey key)
		{
			var dbValue = _serverRepo.Get(key.ToString()) ?? string.Empty;
			_config.Add(key, dbValue);
		}

		public bool HasLoadedConfig { get; }

		public Uri Url
		{
			get
			{
				var dbValue = _config[ServerConfigurationKey.NotificationExtProviderUrl];
				return string.IsNullOrEmpty(dbValue) ? null : new Uri(dbValue);
			}
		}

		public string User => _config[ServerConfigurationKey.NotificationExtProviderUser];

		public string Password => _config[ServerConfigurationKey.NotificationExtProviderPassword];

		public string From => _config[ServerConfigurationKey.NotificationExtProviderFrom];

		public string ClassName => _config[ServerConfigurationKey.NotificationExtProviderClass];

		public string Assembly => _config[ServerConfigurationKey.NotificationExtProviderAssembly];

		public string Api => _config[ServerConfigurationKey.NotificationExtProviderApiId];

		public string Data => _config[ServerConfigurationKey.NotificationExtProviderData];

		public string FindSuccessOrError => _config[ServerConfigurationKey.NotificationExtProviderFindSuccessOnError];

		public string ErrorCode => _config[ServerConfigurationKey.NotificationExtProviderErrorCode];

		public string SuccessCode => _config[ServerConfigurationKey.NotificationExtProviderSuccessCode];

		public bool SkipSearch => bool.TryParse(_config[ServerConfigurationKey.NotificationExtProviderSkipSearch], out var value) ? value : false;

		public string SmtpHost => _config[ServerConfigurationKey.NotificationSmtpHost];

		public int SmtpPort => int.TryParse(_config[ServerConfigurationKey.NotificationSmtpPort], out var value) ? value : -1;

		public bool SmtpUseSsl => bool.TryParse(_config[ServerConfigurationKey.NotificationSmtpUseSsl], out var value) ? value : false;

		public string SmtpUser => _config[ServerConfigurationKey.NotificationSmtpUser];

		public string SmtpPassword => _config[ServerConfigurationKey.NotificationSmtpPassword];

		public bool SmtpUseRelay => bool.TryParse(_config[ServerConfigurationKey.NotificationSmtpUseRelay], out var value) ? value : false;

		public string ContentType => _config[ServerConfigurationKey.NotificationContentType];

		public string EncodingName => _config[ServerConfigurationKey.NotificationEncodingName];
	}
}