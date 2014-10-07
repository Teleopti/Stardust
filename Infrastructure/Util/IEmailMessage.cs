namespace Teleopti.Ccc.Infrastructure.Util
{
	public interface IEmailMessage
	{
		string Sender { get; set; }
		string Recipient { get; set; }
		string Subject { get; set; }
		string Message { get; set; }
	}
}