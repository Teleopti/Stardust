using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	public class FakeNow : INow
	{
		private DateTime _now;

		public FakeNow(DateTime now)
		{
			_now = now;
		}

		public void SetFakeNow(DateTime now)
		{
			_now = now;
		}

		public DateTime UtcDateTime()
		{
			return _now;
		}
	}
}
