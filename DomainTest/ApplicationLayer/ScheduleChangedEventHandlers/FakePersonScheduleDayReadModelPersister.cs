using System;
using System.Collections.Generic;
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

		public void UpdateReadModels(DateOnlyPeriod period, Guid personId, Guid businessUnitId, IEnumerable<PersonScheduleDayReadModel> readModels, bool initialLoad)
		{
			Updated = readModels;
		}
	}
}