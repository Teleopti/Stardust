namespace Teleopti.Ccc.Infrastructure.Util
{
	public struct EmailMessage
	{
		public string Sender { get; set; }
		public string Recipient { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
	}
}