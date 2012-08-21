using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer.SMS
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public interface ISmsLinkChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
		string SmsMobileNumber(IPerson person);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsLinkChecker : ISmsLinkChecker
	{
		public string SmsMobileNumber(IPerson person)
		{
			// get wich optional column to use

			// get a value if one
			//foreach (var optionalColumnValue in person.OptionalColumnValueCollection)
			//{
				
			//}
			//no value
			return "";
		}
	}
}