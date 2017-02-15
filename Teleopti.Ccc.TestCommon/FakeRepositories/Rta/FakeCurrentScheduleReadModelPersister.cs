using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeCurrentScheduleReadModelPersister : ICurrentScheduleReadModelPersister, IScheduleReader
	{
		private class data
		{
			public Guid PersonId;
			public IEnumerable<ScheduledActivity> Schedules;
			public bool Valid;

		}

		private IEnumerable<data> _data = Enumerable.Empty<data>();

		public IEnumerable<ScheduledActivity> Read()
		{
			return _data
				.SelectMany(x => x.Schedules ?? Enumerable.Empty<ScheduledActivity>())
				.ToArray();
		}
		
		public void Invalidate(Guid personId)
		{
			var data = _data.SingleOrDefault(x => x.PersonId == personId);
			if (data == null)
				_data = _data.Append(new data {PersonId = personId});
			_data.Single(x => x.PersonId == personId).Valid = false;
		}

		public IEnumerable<Guid> GetInvalid()
		{
			return _data
				.Where(x => !x.Valid)
				.Select(x => x.PersonId)
				.ToArray();
		}

		public void Persist(Guid personId, IEnumerable<ScheduledActivity> schedule)
		{
			_data = _data
				.Where(x => x.PersonId != personId)
				.ToArray();
			if (schedule.Any())
			{
				_data = _data
					.Append(new data
					{
						PersonId = personId,
						Schedules = schedule.ToArray(),
						Valid = true
					})
					.ToArray();
			}
		}

	}
}