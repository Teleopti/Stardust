using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class EmailConfiguration : IEmailConfiguration
	{
		private readonly INotificationConfigReader _configReader;

		public EmailConfiguration(INotificationConfigReader configReader)
		{
			_configReader = configReader;
		}

		public string SmtpHost => _configReader.SmtpHost;
		public int SmtpPort => _configReader.SmtpPort;
		public bool SmtpUseSsl => _configReader.SmtpUseSsl;
		public string SmtpUser => _configReader.SmtpUser;
		public string SmtpPassword => _configReader.SmtpPassword;
		public bool SmtpUseRelay => _configReader.SmtpUseRelay;
	}
}