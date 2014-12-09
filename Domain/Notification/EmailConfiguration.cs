using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class EmailConfiguration : IEmailConfiguration
	{
		private readonly INotificationConfigReader _configReader;

		public EmailConfiguration(INotificationConfigReader configReader)
		{
			_configReader = configReader;
		}

		public string SmtpHost { get { return _configReader.SmtpHost; } }
		public int SmtpPort { get { return _configReader.SmtpPort; } }
		public bool SmtpUseSsl { get { return _configReader.SmtpUseSsl; } }
		public string SmtpUser { get { return _configReader.SmtpUser; } }
		public string SmtpPassword { get { return _configReader.SmtpPassword; } }
		public bool SmtpUseRelay { get { return _configReader.SmtpUseRelay; } }
	}
}