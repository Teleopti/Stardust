namespace Teleopti.Ccc.Domain.Notification
{
	public interface IEmailConfiguration
	{
		string SmtpHost { get; }
		int SmtpPort { get; }
		bool SmtpUseSsl { get; }
		string SmtpUser { get; }
		string SmtpPassword { get; }
		bool SmtpUseRelay { get; }
	}
}