using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class DoNotNotifySmsLink : INotify
	{
		public void Notify(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
		}
	}
}