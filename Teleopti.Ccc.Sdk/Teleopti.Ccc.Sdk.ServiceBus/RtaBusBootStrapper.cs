using Teleopti.Ccc.Sdk.ServiceBus.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class RtaBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			//add RTA state checker
			var rtaChecker = new BusinessUnitInfoFinder(daBus);
			rtaChecker.SendMessage();
		}
	}
}