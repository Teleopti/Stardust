using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IEventsPublisher
	{
		void Publish(IEnumerable<IEvent> events);
	}
}