using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Clickatell"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class ClickatellSmsSender : ISmsSender
	{
		private string _smsString =
			@"<clickAPI>
				<sendMsg><api_id>3388480</api_id><user>{0}</user>
				<password>{1}</password><to>{2}</to><text>{3}</text>
				</sendMsg>
			</clickAPI>";
		public void SendSms(DateOnlyPeriod dateOnlyPeriod, string mobileNumber)
		{
			//do it do it
		}
	}
}