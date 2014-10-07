namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface IEmailConfiguration
	{
		string SmtpHost { get; }
		int SmtpPort { get; }
		int SmtpPortSsl { get; }
		bool SmtpIsSslRequired { get; }
		string SmtpUser { get; }
		string SmtpPassword { get; }
	}
}