using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesSender
	{
		private readonly MutableNow _now;
		private readonly TestConfiguration _stateHolder;
		private readonly Http _http;
		private TimeStateInfo[] changes;

		public StatesSender(
			MutableNow now, 
			TestConfiguration stateHolder,
			Http http)
		{
			_now = now;
			_stateHolder = stateHolder;
			_http = http;
		}

			
		[LogTime]
		public virtual void Send()
		{
			changes = new[]
			{
				new TimeStateInfo {Time = "2016-02-26 07:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 07:05", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 07:06", StateCode = "LoggedOff"},

				// 08:00 phone
				new TimeStateInfo {Time = "2016-02-26 08:01", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 08:30", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 08:32", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 09:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 09:05", StateCode = "Ready"},

				// 10:00 break
				new TimeStateInfo {Time = "2016-02-26 10:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 10:02", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 10:03", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 10:15", StateCode = "Ready"},

				// 11:30 lunch
				new TimeStateInfo {Time = "2016-02-26 11:35", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 11:36", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 11:37", StateCode = "LoggedOff"},

				// 12:00 phone
				new TimeStateInfo {Time = "2016-02-26 11:55", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 12:20", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 12:22", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 12:30", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 12:21", StateCode = "Ready"},

				// 15:00 break
				new TimeStateInfo {Time = "2016-02-26 14:55", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 15:02", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 15:03", StateCode = "LoggedOff"},

				// 15:15 phone
				new TimeStateInfo {Time = "2016-02-26 15:15", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 15:45", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 15:47", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 16:10", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 16:15", StateCode = "Ready"},

				// 17:00 off
				new TimeStateInfo {Time = "2016-02-26 17:05", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 17:10", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 17:11", StateCode = "LoggedOff"},
			};

			changes.ForEach(i =>
			{
				_now.Is(i.Time.Utc());
				_http.Get("/Test/SetCurrentTime?ticks=" + _now.UtcDateTime().Ticks);
				Enumerable.Range(0, _stateHolder.NumberOfAgents)
					.ForEach(roger =>
					{
						_http.PostJson(
							"Rta/State/Change",
							new ExternalUserStateWebModel
							{
								AuthenticationKey = LegacyAuthenticationKey.TheKey,
								UserCode = "roger" + roger,
								StateCode = i.StateCode,
								IsLoggedOn = true,
								PlatformTypeId = Guid.Empty.ToString(),
								SourceId = _stateHolder.SourceId,
								IsSnapshot = false
							});
					});
			});
		}

		public IEnumerable<TimeStateInfo> SentSates()
		{
			return changes;
		}
	}
}