using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface IDoNotifySmsLink
	{
		void NotifySmsLink(ScheduleDayReadModel readModel, DateOnly date, IPerson person);
	}
}