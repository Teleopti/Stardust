using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceInfo
	{
		private readonly InputInfo _input;
		private readonly AgentState _stored;
		private readonly MappingsState _mappings;
		private readonly Guid _businessUnitId;
		private readonly StateRuleInfo _stateRuleInfo;
		private readonly ScheduleInfo _scheduleInfo;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly StateMapper _stateMapper;

		public AdherenceInfo(
			InputInfo input,
			AgentState stored,
			MappingsState mappings,
			Guid businessUnitId,
			StateRuleInfo stateRuleInfo,
			ScheduleInfo scheduleInfo,
			AppliedAdherence appliedAdherence,
			StateMapper stateMapper)
		{
			_input = input;
			_stored = stored;
			_mappings = mappings;
			_businessUnitId = businessUnitId;
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
			return adherenceFor(_stored?.StateCode, _stored.PlatformTypeId(), _stored?.ActivityId);
		}

		public EventAdherence AdherenceForNewStateAndPreviousActivity()
		{
			return adherenceFor(_input.StateCode, _input.ParsedPlatformTypeId(), _scheduleInfo.PreviousActivity());
		}

		public EventAdherence AdherenceForStoredStateAndCurrentActivity()
		{
			return adherenceFor(_stored?.StateCode, _stored.PlatformTypeId(), _scheduleInfo.CurrentActivity());
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, ScheduledActivity activity)
		{
			var activityId = (Guid?)null;
			if (activity != null)
				activityId = activity.PayloadId;
			return adherenceFor(stateCode, platformTypeId, activityId);
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, Guid? activityId)
		{
			var rule = _stateMapper.RuleFor(_mappings, _businessUnitId, platformTypeId, stateCode, activityId);
			if (rule == null)
				return _appliedAdherence.ForEvent(null, null);
			return _appliedAdherence.ForEvent(rule.Adherence, rule.StaffingEffect);
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