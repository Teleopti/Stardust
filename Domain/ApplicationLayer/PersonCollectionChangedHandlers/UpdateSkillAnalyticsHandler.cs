using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class UpdateSkillAnalyticsHandler :
		IHandleEvent<SkillChangedEvent>,
		IRunOnServiceBus
	{
		public void Handle(SkillChangedEvent @event)
		{
			
		}
	}
}