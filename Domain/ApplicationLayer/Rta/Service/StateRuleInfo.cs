using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateRuleInfo
	{
		private readonly Lazy<MappedRule> _mappedRule;
		private readonly Lazy<MappedState> _mappedState;
		private readonly Lazy<StoredStateInfo> _stored;

		public StateRuleInfo(
			Lazy<IEnumerable<Mapping>> mappings,
			Lazy<StoredStateInfo> stored, 
			string stateCode,
			Guid platformTypeId,
			Guid businessUnitId,
			ExternalUserStateInputModel input,
			ScheduleInfo schedule,
			StateMapper stateMapper
			)
		{
			_stored = stored;
			_mappedState = new Lazy<MappedState>(() => stateMapper.StateFor(mappings.Value, businessUnitId, platformTypeId, stateCode, input.StateDescription));
			_mappedRule = new Lazy<MappedRule>(() => stateMapper.RuleFor(mappings.Value, businessUnitId, platformTypeId, stateCode, schedule.CurrentActivityId()) ?? new MappedRule());
		}

		public bool StateGroupChanged()
		{
			return _mappedState.Value.StateGroupId != _stored.Value.StateGroupId();
		}

		public Guid? StateGroupId()
		{
			return _mappedState.Value.StateGroupId;
		}

		public string StateGroupName()
		{
			return _mappedState.Value.StateGroupName;
		}



		public bool HasRuleChanged()
		{
			return _mappedRule.Value.RuleId != _stored.Value.RuleId();
		}



		public Guid? RuleId()
		{
			return _mappedRule.Value.RuleId;
		}

		public string RuleName()
		{
			return _mappedRule.Value.RuleName;
		}

		public int? RuleDisplayColor()
		{
			return _mappedRule.Value.DisplayColor;
		}

		public double? StaffingEffect()
		{
			return _mappedRule.Value.StaffingEffect;
		}

		public Adherence? Adherence()
		{
			return _mappedRule.Value.Adherence;
		}



		public bool IsAlarm()
		{
			return _mappedRule.Value.IsAlarm;
		}

		public long AlarmThresholdTime()
		{
			return _mappedRule.Value.ThresholdTime;
		}

		public int? AlarmColor()
		{
			return _mappedRule.Value.AlarmColor;
		}


	}
}