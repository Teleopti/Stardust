using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface INotify
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		void Notify(ScheduleDayReadModel readModel, DateOnly date, IPerson person);
	}
}