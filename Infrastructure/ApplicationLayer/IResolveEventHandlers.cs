using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IResolveEventHandlers
	{
		IEnumerable<object> ResolveHandlersForEvent(IEvent @event);
	}
}