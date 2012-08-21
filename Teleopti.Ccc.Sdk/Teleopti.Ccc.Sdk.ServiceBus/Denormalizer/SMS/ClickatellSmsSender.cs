using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellSmsSender : ISmsSender
	{
		public void SendSms(DateOnlyPeriod dateOnlyPeriod, string mobileNumber)
		{
			//do it do it
		}
	}
}