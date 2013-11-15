using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IEventsPublisher
	{
		void Publish(IEnumerable<IEvent> events);
	}
}