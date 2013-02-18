using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.RTA;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper", Justification = "As the base class is named as it is, this will remain like this."), 
	 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper", Justification = "As the base class is named as it is, this will remain like this.")]
	public class DenormalizeBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			var initialLoad = new InitialLoadOfScheduleProjectionReadModel(daBus);
			initialLoad.Check();
            //add RTA state checker
            var rtaChecker = new RTAStateChecker(daBus);
            rtaChecker.Check();
		}
	}
}