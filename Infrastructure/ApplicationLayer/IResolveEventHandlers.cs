using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IResolveEventHandlers
	{
		IEnumerable ResolveHandlersForEvent(IEvent @event);
	}
}