using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IAggregateRootWithEvents : IAggregateRoot
	{
		IEnumerable<IEvent> PopAllEvents(INow now);
	}
}