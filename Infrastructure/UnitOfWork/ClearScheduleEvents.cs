using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ClearScheduleEvents : IClearScheduleEvents
	{
		public void Execute(IDifferenceCollection<IPersistableScheduleData> scheduleDifference)
		{
			var aggregateRoots = scheduleDifference.Select(d => d.CurrentItem);
			aggregateRoots.OfType<IAggregateRoot_Events>().ForEach(a => a.PopAllEvents(null));
		}
	}
}