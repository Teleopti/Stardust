using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class StatesSender
	{
		private readonly MutableNow _now;
		private readonly TestConfiguration _testConfiguration;
		private readonly DataCreator _data;
		private readonly Http _http;
		private readonly ScheduleInvalidator _scheduleInvalidator;

		public StatesSender(
			MutableNow now,
			TestConfiguration testConfiguration,
			DataCreator data,
			Http http,
			ScheduleInvalidator scheduleInvalidator)
		{
			_now = now;
			_testConfiguration = testConfiguration;
			_data = data;
			_http = http;
			_scheduleInvalidator = scheduleInvalidator;
		}

		[TestLog]
		public virtual void SendAllAsSingles() =>
			sendAll(1);

		[TestLog]
		public virtual void SendAllAsSmallBatches() =>
			sendAll(50);

		[TestLog]
		public virtual void SendAllAsLargeBatches() =>
			sendAll(1000);

		private void sendAll(int batchSize)
		{
			StateChanges().ForEach(stateChange => { SendStateChange(batchSize, stateChange); });
			triggerRecurringJobs();
		}

		[TestLog]
		protected virtual void SendStateChange(int batchSize, StateChange stateChange)
		{
			if (stateChange.ScheduleChangesPercent > 0)
				_scheduleInvalidator.InvalidateSchedules(stateChange.ScheduleChangesPercent);

			var now = stateChange.Time.Utc();
			_now.Is(now);
			setTime(stateChange.Time);

			_data.LogonsWorking()
				.Select(logon => new ExternalUserBatchStateWebModel
				{
					UserCode = logon.UserCode,
					StateCode = stateChange.StateCode,
				})
				.Batch(batchSize)
				.Select(x => new ExternalUserBatchWebModel
				{
					AuthenticationKey = LegacyAuthenticationKey.TheKey,
					SourceId = _testConfiguration.SourceId,
					States = x
				})
				.ForEach(Send);
		}

		private void setTime(string time) =>
			_http.PostJson("/Test/SetCurrentTime", new {time = time, waitForQueue = false});

		private void triggerRecurringJobs() =>
			_http.GetJson("/Test/TriggerRecurringJobs");

		[TestLog]
		protected virtual void Send(ExternalUserBatchWebModel batch)
		{
			if (batch.States.Count() == 1)
				_http.PostJson("Rta/State/Change", new ExternalUserStateWebModel
				{
					AuthenticationKey = batch.AuthenticationKey,
					SourceId = batch.SourceId,
					StateCode = batch.States.Single().StateCode,
					UserCode = batch.States.Single().UserCode
				});
			else
				_http.PostJson("Rta/State/Batch", batch);
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

			states = states.Concat(OffChangesFor("2016-02-26 07:00")); // 1 state changes, 1 adherence change(s)
			// 08:00 phone
			states = states.Concat(PhoneChangesFor(20, 5, "2016-02-26 08:01")); // 40 state changes,  2 adherence change(s)
			// 10:00 break
			states = states.Concat(OffChangesFor("2016-02-26 10:01")); // 1 state changes,  2 adherence change(s)
			// 10:15 phone
			states = states.Concat(PhoneChangesFor(10, 5, "2016-02-26 10:16")); // 20 state changes,  2 adherence change(s)
			// 11:30 lunch
			states = states.Concat(OffChangesFor("2016-02-26 10:35")); // 1 state changes,  2 adherence change(s)
			// 12:00 phone
			states = states.Concat(PhoneChangesFor(30, 5, "2016-02-26 11:55")); // 60 state changes,  2 adherence change(s)
			// 15:00 break
			states = states.Concat(OffChangesFor("2016-02-26 14:55")); // 1 state changes,  2 adherence change(s)
			// 15:15 phone
			states = states.Concat(PhoneChangesFor(20, 5, "2016-02-26 15:16")); // 40 state changes,  2 adherence change(s)
			// 17:00 off
			states = states.Concat(OffChangesFor("2016-02-26 17:05")); // 1 state changes,  2 adherence change(s)

			return states;
		}

		public IEnumerable<StateChange> OffChangesFor(string time)
		{
			yield return new StateChange
			{
				ScheduleChangesPercent = 0,
				Time = $"{time}:00",
				StateCode = $"LoggedOff"
			};
		}

		public IEnumerable<StateChange> PhoneChangesFor(int calls, int scheduleChangesPercent, string time)
		{
			var statesPerCall = _data.PhoneStates().Count();
			return _data.PhoneStates()
				.Infinite()
				.Take(calls * statesPerCall)
				.Select((x, i) =>
					new StateChange
					{
						ScheduleChangesPercent = i == 0 ? scheduleChangesPercent : 0,
						Time = $"{time}:{i:00}",
						StateCode = x
					}
				);
		}
	}
}