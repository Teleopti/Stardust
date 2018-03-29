using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class PersonEmploymentChangedEventEmptyHandler  : IHandleEvent<PersonEmploymentChangedEvent>, IRunOnHangfire
	{
		public virtual void Handle(PersonEmploymentChangedEvent @event)
		{
			
		}
	}
}
