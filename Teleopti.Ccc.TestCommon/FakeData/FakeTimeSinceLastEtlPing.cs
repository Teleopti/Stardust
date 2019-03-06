using System;
using Teleopti.Ccc.Domain.ETL;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeTimeSinceLastEtlPing : ITimeSinceLastEtlPing
	{
		private TimeSpan _timeSpan;
		
		public void SetTime(TimeSpan timespan)
		{
			_timeSpan = timespan;
		}
		
		public TimeSpan Fetch()
		{
			return _timeSpan;
		}
	}
}