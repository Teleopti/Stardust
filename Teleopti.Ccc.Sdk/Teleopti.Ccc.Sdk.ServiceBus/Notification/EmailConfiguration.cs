using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
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
		public int SmtpPortSsl { get { return _configReader.SmtpPortSsl; } }
		public bool SmtpIsSslRequired { get { return _configReader.SmtpIsSslRequired; } }
		public string SmtpUser { get { return _configReader.SmtpUser; } }
		public string SmtpPassword { get { return _configReader.SmtpPassword; } }
	}
}