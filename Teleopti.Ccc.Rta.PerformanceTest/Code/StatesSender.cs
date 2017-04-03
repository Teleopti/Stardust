using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
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
		private readonly TestConfiguration _testConfiguration;
		private readonly DataCreator _data;
		private readonly Http _http;

		public StatesSender(
			MutableNow now, 
			TestConfiguration testConfiguration,
			DataCreator data,
			Http http)
		{
			_now = now;
			_testConfiguration = testConfiguration;
			_data = data;
			_http = http;
		}

		[TestLog]
		public virtual void SendAllAsSingles()
		{
			sendAll(1);
		}

		[TestLog]
		public virtual void SendAllAsSmallBatches()
		{
			sendAll(50);
		}

		[TestLog]
		public virtual void SendAllAsLargeBatches()
		{
			sendAll(1000);
		}

		private void sendAll(int batchSize)
		{
			StateChanges().ForEach(stateChange =>
			{
				SendStateChange(batchSize, stateChange);
			});
		}
		
		[TestLog]
		protected virtual void SendStateChange(int batchSize, StateChange stateChange)
		{
			var now = stateChange.Time.Utc();
			_now.Is(now);
			_http.PostJson("/Test/SetCurrentTime", new {time = stateChange.Time});


			_data.LogonsWorking()
				.Select(logon => new ExternalUserStateWebModel
				{
					AuthenticationKey = LegacyAuthenticationKey.TheKey,
					UserCode = logon.UserCode,
					StateCode = stateChange.StateCode,
					SourceId = _testConfiguration.SourceId,
				})
				.Batch(batchSize)
				.ForEach(x => Send(x.ToArray()));
		}

		[TestLog]
		protected virtual void Send(IEnumerable<ExternalUserStateWebModel> states)
		{
			if (states.Count() == 1)
				_http.PostJson("Rta/State/Change", states.Single());
			else
				_http.PostJson("Rta/State/Batch", states);
		}

		// ~10% adherence changes
		// 10 calls per hour
		// 2 state changes per call
		// 160+5 state changes
		// 17 adherence changes
		[TestLog]
		public virtual IEnumerable<StateChange> StateChanges()
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