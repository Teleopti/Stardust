using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class TestableNow : INow
	{
		private DateTime _time;

		public TestableNow(DateTime time)
		{
			_time = time;
		}

		public TestableNow()
		{
			_time = DateTime.UtcNow;
		}

		public DateTime UtcDateTime()
		{
			return _time;
		}

		public void Change(DateTime utc)
		{
			_time = utc;
		}
	}
}