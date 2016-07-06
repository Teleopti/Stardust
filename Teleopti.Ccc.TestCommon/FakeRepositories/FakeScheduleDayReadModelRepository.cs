using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleDayReadModelRepository :IScheduleDayReadModelRepository
	{
		private IList<ScheduleDayReadModel> _scheduleDayReadModels = new List<ScheduleDayReadModel>();

		public IList<ScheduleDayReadModel> ReadModelsOnPerson(DateOnly startDate, DateOnly toDate, Guid personId)
		{
			return _scheduleDayReadModels.Where(m => m.PersonId == personId && startDate <= m.BelongsToDate && toDate >= m.BelongsToDate).ToList();
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
			throw new NotImplementedException();
		}
	}
}
