namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public interface ISmsSenderFactory
	{
		ISmsSender Sender { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsSenderFactory : ISmsSenderFactory
	{
		//create via reflection of the class from config.file
		public ISmsSender Sender
		{
			get { return new ClickatellSmsSender();}
		}
	}
}