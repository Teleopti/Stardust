using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public interface IPersonScheduleDayReadModelPersister
	{
		bool IsInitialized();
		void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad);
		int SaveReadModel(PersonScheduleDayReadModel model, bool initialLoad);
		void DeleteReadModel(Guid personId, DateOnly date);
	}
}