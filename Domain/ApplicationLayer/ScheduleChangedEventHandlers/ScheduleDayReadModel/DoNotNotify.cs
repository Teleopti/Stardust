using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class DoNotNotify : INotificationValidationCheck
	{
		public void InitiateNotify(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
		}
	}
}