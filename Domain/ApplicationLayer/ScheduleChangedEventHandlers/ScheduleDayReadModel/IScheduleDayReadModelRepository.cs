using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public interface IScheduleDayReadModelRepository
	{
		ScheduleDayReadModel ForPerson(DateOnly date, Guid personId);

		void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId);

		void SaveReadModel(ScheduleDayReadModel model);
		bool IsInitialized();
	}
}