using System;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Intraday.PerformanceTest
{
	public class TimeSetter
	{
		private readonly Http _http;
		private readonly MutableNow _now;
		public TimeSetter(Http http, MutableNow now)
		{
			_http = http;
			_now = now;
		}

		public void SetDateTime(string time)
		{
			_now.Is(time);
			_http.Get("/Test/SetCurrentTime?ticks=" + time.Utc().Ticks);
		}
	}
}
