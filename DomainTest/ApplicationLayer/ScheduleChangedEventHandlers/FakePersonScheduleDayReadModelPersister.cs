using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class FakePersonScheduleDayReadModelPersister : IPersonScheduleDayReadModelPersister
	{
		public IEnumerable<PersonScheduleDayReadModel> Updated;

		public bool IsInitialized()
		{
			return true;
		}

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad, bool isDefaultScenario)
		{
			Updated = readModels;
		}

		public int SaveReadModel(PersonScheduleDayReadModel model, bool initialLoad)
		{
			var list = Updated.ToList();
			list.Add(model);
			Updated = list;
			return 1;
		}

		public void DeleteReadModel(Guid personId, DateOnly date)
		{
			Updated = Updated.Where(x => x.PersonId != personId && x.BelongsToDate != date).ToList();
		}
	}
}