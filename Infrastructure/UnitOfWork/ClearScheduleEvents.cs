using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ClearScheduleEvents : IClearScheduleEvents
	{
		public void Execute(IDifferenceCollection<IPersistableScheduleData> scheduleDifference)
		{
			var aggregateRoots = scheduleDifference.Select(d => d.CurrentItem);
      aggregateRoots.OfType<IAggregateRootWithEvents>().ForEach(a => a.PopAllEvents());
		}
	}
}