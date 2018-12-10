using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeCurrentScheduleReadModelPersister : ICurrentScheduleReadModelPersister, IScheduleReader
	{
		private class data
		{
			public Guid PersonId;
			public IEnumerable<ScheduledActivity> Schedules;
			public int Revision;
			public bool Valid;
		}
		
		private IEnumerable<data> _data = Enumerable.Empty<data>();

		public void Has(Guid personId, int revision, IEnumerable<ScheduledActivity> schedule)
		{
			_data = _data
				.Append(new data
				{
					PersonId = personId,
					Schedules = schedule.ToArray(),
					Revision = revision,
					Valid = true
				})
				.ToArray();
		}
		
		public IEnumerable<CurrentSchedule> Read(int fromRevision)
		{
			return _data
				.Where(x => x.Revision > fromRevision)
				.Select(x => new CurrentSchedule
				{
					PersonId = x.PersonId,
					Schedule = x.Schedules ?? Enumerable.Empty<ScheduledActivity>()
				})
				.ToArray();
		}

		public IEnumerable<CurrentSchedule> Read()
		{
			return _data
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

		public void Persist(Guid personId, int revision, IEnumerable<ScheduledActivity> schedule)
		{
			_data = _data
				.Where(x => x.PersonId != personId)
				.Append(new data
				{
					PersonId = personId,
					Schedules = schedule.ToArray(),
					Revision = revision,
					Valid = true
				})
				.ToArray();
		}
	}
}