using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class RtaStates
	{
		private readonly MutableNow _now;
		private readonly TestConfiguration _stateHolder;

		public RtaStates(MutableNow now, TestConfiguration stateHolder)
		{
			_now = now;
			_stateHolder = stateHolder;
		}

		[LogTime]
		public virtual void Send()
		{
			var changes = new[]
			{
				new TimeStateInfo {Time = "2016-02-26 08:01", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 09:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 09:05", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 10:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 10:15", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 11:35", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 11:55", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 14:55", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 15:15", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 17:05", StateCode = "LoggedOff"},
			};

			changes.ForEach(i =>
			{
				_now.Is(i.Time.Utc());
				Http.Get("/Test/SetCurrentTime?ticks=" + _now.UtcDateTime().Ticks);
				Enumerable.Range(0, _stateHolder.NumberOfAgents)
					.ForEach(roger =>
					{
						Http.PostJson(
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
	}
}