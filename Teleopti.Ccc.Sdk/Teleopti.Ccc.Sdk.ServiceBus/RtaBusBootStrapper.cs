using System.Configuration;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper"), 
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper")]
	public class RtaBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			var dbConnection = ConfigurationManager.ConnectionStrings["Queue"];
			QueueClearMessages.ClearMessages(dbConnection.ConnectionString, "rta");
			
			//add RTA state checker
			var rtaChecker = new BusinessUnitStarter(() => Container.Resolve<IServiceBus>());
			rtaChecker.SendMessage();
		}
	}
}