using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class PersonEmploymentChangedEventEmptyHandler  : IHandleEvent<PersonEmployementChangedEvent>, IRunOnHangfire
	{
		public virtual void Handle(PersonEmployementChangedEvent @event)
		{
			
		}
	}
}
