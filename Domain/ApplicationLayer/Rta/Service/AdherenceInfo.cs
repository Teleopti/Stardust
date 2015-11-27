using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly PersonOrganizationData _person;
		private readonly StoredStateInfo _stored;
		private readonly StateAlarmInfo _stateAlarmInfo;
		private readonly ScheduleInfo _scheduleInfo;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly IStateMapper _stateMapper;

		public AdherenceInfo(
			ExternalUserStateInputModel input, 
			PersonOrganizationData person,
			StoredStateInfo stored,
			StateAlarmInfo stateAlarmInfo,
			ScheduleInfo scheduleInfo, 
			IAppliedAdherence appliedAdherence,
			IStateMapper stateMapper)
		{
			_input = input;
			_person = person;
			_stored = stored;
			_stateAlarmInfo = stateAlarmInfo;
			_scheduleInfo = scheduleInfo;
			_appliedAdherence = appliedAdherence;
			_stateMapper = stateMapper;
		}

		public EventAdherence Adherence()
		{
			return _appliedAdherence.ForEvent(_stateAlarmInfo.Adherence(), _stateAlarmInfo.StaffingEffect());
		}

		public EventAdherence AdherenceForStoredState()
		{
			return adherenceFor(_stored.StateCode(), _stored.PlatformTypeId(), _stored.ActivityId());
		}

		public EventAdherence AdherenceForNewStateAndPreviousActivity()
		{
			return adherenceFor(_input.StateCode, _input.ParsedPlatformTypeId(), _scheduleInfo.PreviousActivity());
		}

		public EventAdherence AdherenceForStoredStateAndCurrentActivity()
		{
			return adherenceFor(_stored.StateCode(), _stored.PlatformTypeId(), _scheduleInfo.CurrentActivity());
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, ScheduleLayer activity)
		{
			var activityId = (Guid?)null;
			if (activity != null)
				activityId = activity.PayloadId;
			return adherenceFor(stateCode, platformTypeId, activityId);
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, Guid? activityId)
		{
			var alarm = _stateMapper.AlarmFor(_person.BusinessUnitId, platformTypeId, stateCode, activityId);
			if (alarm == null)
				return _appliedAdherence.ForEvent(null, null);
			return _appliedAdherence.ForEvent(alarm.Adherence, alarm.StaffingEffect);
		}

		public static EventAdherence AggregatorAdherence(AgentStateReadModel readModel)
		{
			return new ByStaffingEffect().ForEvent(null, readModel.StaffingEffect);
		}





		public bool AdherenceChangedFromActivityChange()
		{
			EventAdherence? from = null;
			if (_stored != null)
				from = AdherenceForStoredState();
			var to = AdherenceForStoredStateAndCurrentActivity();
			return from != to;
		}

		public bool AdherenceChangedFromStateChange()
		{
			EventAdherence? from = null;
			if (_stored != null)
				from = AdherenceForStoredStateAndCurrentActivity();
			var to = Adherence();
			return from != to;
		}
		
	}
}