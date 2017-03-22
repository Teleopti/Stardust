using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeCurrentScheduleReadModelPersister : ICurrentScheduleReadModelPersister, IScheduleReader
	{
		private class data
		{
			public Guid PersonId;
			public IEnumerable<ScheduledActivity> Schedules;
			public DateTime UpdatedAt;
			public bool Valid;
		}

		private readonly INow _now;
		private IEnumerable<data> _data = Enumerable.Empty<data>();

		public FakeCurrentScheduleReadModelPersister(INow now)
		{
			_now = now;
		}

		public IEnumerable<CurrentSchedule> Read(DateTime? updatedAfter)
		{
			return _data
				.Where(x => !updatedAfter.HasValue || x.UpdatedAt >= updatedAfter.Value)
				.Select(x => new CurrentSchedule
				{
					PersonId = x.PersonId,
					Schedule = x.Schedules ?? Enumerable.Empty<ScheduledActivity>()
				})
				.ToArray();
		}
		
		public void Invalidate(Guid personId)
		{
			var data = _data.SingleOrDefault(x => x.PersonId == personId);
			if (data == null)
				_data = _data.Append(new data {PersonId = personId, Valid = false}).ToArray();
			else
				data.Valid = false;
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
				.Append(new data
				{
					PersonId = personId,
					Schedules = schedule.ToArray(),
					UpdatedAt = _now.UtcDateTime(),
					Valid = true
				})
				.ToArray();
		}

	}
}