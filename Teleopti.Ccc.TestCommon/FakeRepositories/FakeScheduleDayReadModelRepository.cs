using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleDayReadModelRepository :IScheduleDayReadModelRepository
	{
		private IList<ScheduleDayReadModel> _scheduleDayReadModels = new List<ScheduleDayReadModel>();

		public ScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			return _scheduleDayReadModels.FirstOrDefault(m => m.PersonId == personId && date == m.BelongsToDate);
		}

		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid personId)
		{
			_scheduleDayReadModels =
				_scheduleDayReadModels.Where(
					r => !(r.PersonId == personId && r.BelongsToDate >= period.StartDate && r.BelongsToDate <= period.EndDate)).ToList();
		}

		public void SaveReadModel(ScheduleDayReadModel model)
		{
			_scheduleDayReadModels.Add(model);
		}

		public bool IsInitialized()
		{
			return _scheduleDayReadModels.Any();
		}
	}
}
