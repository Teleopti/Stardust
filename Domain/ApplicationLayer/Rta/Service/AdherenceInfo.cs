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
		private readonly StateRuleInfo _stateRuleInfo;
		private readonly ScheduleInfo _scheduleInfo;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly IStateMapper _stateMapper;

		public AdherenceInfo(
			ExternalUserStateInputModel input, 
			PersonOrganizationData person,
			StoredStateInfo stored,
			StateRuleInfo stateRuleInfo,
			ScheduleInfo scheduleInfo, 
			IAppliedAdherence appliedAdherence,
			IStateMapper stateMapper)
		{
			_input = input;
			_person = person;
			_stored = stored;
			_stateRuleInfo = stateRuleInfo;
			_scheduleInfo = scheduleInfo;
			_appliedAdherence = appliedAdherence;
			_stateMapper = stateMapper;
		}

		public EventAdherence AdherenceForNewStateAndCurrentActivity()
		{
			return _appliedAdherence.ForEvent(_stateRuleInfo.Adherence(), _stateRuleInfo.StaffingEffect());
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
			var rule = _stateMapper.RuleFor(_person.BusinessUnitId, platformTypeId, stateCode, activityId);
			if (rule == null)
				return _appliedAdherence.ForEvent(null, null);
			return _appliedAdherence.ForEvent(rule.Adherence, rule.StaffingEffect);
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
			var to = AdherenceForNewStateAndCurrentActivity();
			return from != to;
		}
		
	}
}