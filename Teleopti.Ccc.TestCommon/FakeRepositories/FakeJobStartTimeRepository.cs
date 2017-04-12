using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeJobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly INow _now;
		public Dictionary<Guid,DateTime> Records = new Dictionary<Guid, DateTime>();

		public FakeJobStartTimeRepository(INow now)
		{
			_now = now;
		}

		public bool CheckAndUpdate(int thresholdMinutes, Guid bu)
		{
			if (Records.ContainsKey(bu) && _now.UtcDateTime() <= Records[bu].AddMinutes(thresholdMinutes))
				return false;

			if (Records.ContainsKey(bu))
				Records.Remove(bu);
			Records.Add(bu, _now.UtcDateTime());
			return true;
		}

		public void UpdateLockTimestamp(Guid bu)
		{
			
		}

		public void ResetLockTimestamp(Guid bu)
		{
			
		}
	}
}