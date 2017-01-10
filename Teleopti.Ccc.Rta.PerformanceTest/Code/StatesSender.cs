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
		private readonly DataCreator _data;
		private readonly Http _http;

		public StatesSender(
			MutableNow now, 
			TestConfiguration stateHolder,
			DataCreator data,
			Http http)
		{
			_now = now;
			_stateHolder = stateHolder;
			_data = data;
			_http = http;
		}

		[TestLogTime]
		public virtual void Send()
		{
			StateChanges().ForEach(stateChange =>
			{
				setTime(stateChange);
				Enumerable.Range(0, _stateHolder.NumberOfAgentsWorking)
					.ForEach(roger =>
					{
						_http.PostJson(
							"Rta/State/Change",
							new ExternalUserStateWebModel
							{
								AuthenticationKey = LegacyAuthenticationKey.TheKey,
								UserCode = $"roger{roger}",
								StateCode = stateChange.StateCode,
								PlatformTypeId = Guid.Empty.ToString(),
								SourceId = _stateHolder.SourceId,
							});
					});
			});
		}

		public void SendBatches()
		{
			StateChanges().ForEach(stateChange =>
			{
				setTime(stateChange);
				Enumerable.Range(0, _stateHolder.NumberOfAgentsWorking)
					.Select(agent => new ExternalUserStateWebModel
					{
						AuthenticationKey = LegacyAuthenticationKey.TheKey,
						UserCode = $"roger{agent}",
						StateCode = stateChange.StateCode,
						PlatformTypeId = Guid.Empty.ToString(),
						SourceId = _stateHolder.SourceId,
					})
					.Batch(1000)
					.ForEach(state => _http.PostJson("Rta/State/Batch", state));
			});
		}

		private void setTime(StateChange stateChange)
		{
			var now = stateChange.Time.Utc();
			_now.Is(now);
			_http.Get("/Test/SetCurrentTime?ticks=" + now.Ticks);
		}

		// ~10% adherence changes
		// 10 calls per hour
		// 2 state changes per call
		// 200+5 state changes
		// 17 adherence changes
		public IEnumerable<StateChange> StateChanges()
		{
			var states = Enumerable.Empty<StateChange>();

			states = states.Concat(_data.OffChangesFor("2016-02-26 07:00")); // 1 state changes, 1 adherence change(s)
			// 08:00 phone
			states = states.Concat(_data.PhoneChangesFor(20, "2016-02-26 08:01")); // 40 state changes,  2 adherence change(s)
			// 10:00 break
			states = states.Concat(_data.OffChangesFor("2016-02-26 10:01")); // 1 state changes,  2 adherence change(s)
			// 10:15 phone
			states = states.Concat(_data.PhoneChangesFor(10, "2016-02-26 10:16")); // 20 state changes,  2 adherence change(s)
			// 11:30 lunch
			states = states.Concat(_data.OffChangesFor("2016-02-26 10:35")); // 1 state changes,  2 adherence change(s)
			// 12:00 phone
			states = states.Concat(_data.PhoneChangesFor(30, "2016-02-26 11:55")); // 60 state changes,  2 adherence change(s)
			// 15:00 break
			states = states.Concat(_data.OffChangesFor("2016-02-26 14:55")); // 1 state changes,  2 adherence change(s)
			// 15:15 phone
			states = states.Concat(_data.PhoneChangesFor(20, "2016-02-26 15:16")); // 40 state changes,  2 adherence change(s)
			// 17:00 off
			states = states.Concat(_data.OffChangesFor("2016-02-26 17:05")); // 1 state changes,  2 adherence change(s)

			return states;
		}
		
	}
}