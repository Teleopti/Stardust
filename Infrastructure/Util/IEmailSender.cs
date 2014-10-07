namespace Teleopti.Ccc.Infrastructure.Util
{
	public interface IEmailSender
	{
		void Send(EmailMessage emailMessage);
	}
}