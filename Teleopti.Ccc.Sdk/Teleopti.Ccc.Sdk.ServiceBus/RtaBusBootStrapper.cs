using System.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper"), 
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper")]
	public class RtaBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			//add RTA state checker
			var rtaChecker = new BusinessUnitInfoFinder(daBus);
			rtaChecker.SendMessage();

			var dbConnection = ConfigurationManager.ConnectionStrings["Queue"];
			ClearDelaySendMessages.ClearMessages(dbConnection.ToString());
		}
	}
}