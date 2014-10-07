namespace Teleopti.Ccc.Infrastructure.Util
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