using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface ISignificantChangeChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		INotificationMessage SignificantChangeNotificationMessage(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel);
	}
}