using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleHistoryRepository :IScheduleHistoryRepository
	{
		private readonly List<Tuple<IRevision, IPersistableScheduleData>> _internalStorage = 
			new List<Tuple<IRevision, IPersistableScheduleData>>(); 

		public IEnumerable<IRevision> FindRevisions(IPerson agent, DateOnly dateOnly, int maxResult)
		{
			return _internalStorage.Select(x => x.Item1).OrderByDescending(x => x.Id);
		}

		public IEnumerable<IPersistableScheduleData> FindSchedules(IRevision revision, IPerson agent, DateOnly dateOnly)
		{
			return _internalStorage.Where(x => x.Item1.Id == revision.Id).Select(x => x.Item2);
		}

		public void SetRevision(IRevision revision, IPersistableScheduleData data)
		{
			_internalStorage.Add(new Tuple<IRevision, IPersistableScheduleData>(revision, data));
		}

		public void ClearRevision()
		{
			_internalStorage.Clear();
		}
	}
}
