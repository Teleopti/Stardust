using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleHistoryRepository : IScheduleHistoryRepository
	{
		private readonly List<Tuple<IRevision, DateOnly, IPersistableScheduleData>> _internalStorage =
			new List<Tuple<IRevision, DateOnly, IPersistableScheduleData>>();

		public IEnumerable<IRevision> FindRevisions(IPerson agent, DateOnly dateOnly, int maxResult)
		{
			return _internalStorage.Where(x => x.Item2 == dateOnly).Select(x => x.Item1).Distinct().OrderByDescending(x => x.Id);
		}

		public IEnumerable<IPersistableScheduleData> FindSchedules(IRevision revision, IPerson agent, DateOnly dateOnly)
		{
			return _internalStorage.Where(x => x.Item1.Id == revision.Id && x.Item2 == dateOnly).Select(x => x.Item3);
		}

		public void SetRevision(IRevision revision, DateOnly date, IPersistableScheduleData data)
		{
			_internalStorage.Add(new Tuple<IRevision, DateOnly, IPersistableScheduleData>(revision, date, data));
		}

		public void ClearRevision()
		{
			_internalStorage.Clear();
		}
	}
}
