using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class DontNotifySmsLink : IDoNotifySmsLink
	{
		public void NotifySmsLink(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
		}
	}
}