using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonScheduleDayReadModelPersister : IPersonScheduleDayReadModelPersister
	{
		public IEnumerable<PersonScheduleDayReadModel> Updated = new List<PersonScheduleDayReadModel>();
		public int NumberOfUpdatedCalls;

		public bool IsInitialized()
		{
			return true;
		}

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad)
		{
			Updated = readModels;
			NumberOfUpdatedCalls++;
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