using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Events the rooted aggregate
	/// </summary>
	public interface IAggregateRootWithEvents : IAggregateRoot
	{
		/// <summary>
		/// Pops all events in this aggregate.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IEvent> PopAllEvents();
	}
}