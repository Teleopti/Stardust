using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeJobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly INow _now;
		public Dictionary<Guid,LockRecord> Records = new Dictionary<Guid, LockRecord>();

		public FakeJobStartTimeRepository(INow now)
		{
			_now = now;
		}

		public bool CheckAndUpdate(int thresholdMinutes, Guid bu)
		{
			if (Records.ContainsKey(bu) && _now.UtcDateTime() <= Records[bu].StartTime.AddMinutes(thresholdMinutes))
				return false;

			if (Records.ContainsKey(bu))
				Records.Remove(bu);
			Records.Add(bu, new LockRecord{StartTime = _now.UtcDateTime() }); 
			return true;
		}

		public void UpdateLockTimestamp(Guid bu)
		{
			if (Records.ContainsKey(bu))
				Records[bu].LockTimestamp = _now.UtcDateTime().AddMinutes(5);
		}

		public void ResetLockTimestamp(Guid bu)
		{
			if (Records.ContainsKey(bu))
				Records[bu].LockTimestamp = null;
		}

		public void RemoveLock(Guid bu)
		{
			if (Records.ContainsKey(bu))
				Records.Remove(bu);
		}
	}

	public class LockRecord
	{
		public DateTime StartTime { get; set; }
		public DateTime? LockTimestamp { get; set; }
	}
}