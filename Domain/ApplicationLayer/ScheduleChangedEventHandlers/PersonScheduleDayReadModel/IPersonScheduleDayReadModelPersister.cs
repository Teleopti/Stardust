using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelPersister
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId);

		void SaveReadModel(PersonScheduleDayReadModel model, bool notifyBroker = true);
		bool IsInitialized();
	}
}