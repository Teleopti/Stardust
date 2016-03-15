using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateRuleInfo
	{
		private readonly MappedRule _mappedRule;
		private readonly MappedState _mappedState;
		private readonly StoredStateInfo _stored;

		public StateRuleInfo(
			string stateCode,
			Guid platformTypeId,
			ExternalUserStateInputModel input,
			StateContext context,
			StoredStateInfo stored,
			ScheduleInfo schedule,
			StateMapper stateMapper
			)
		{
			_mappedState = stateMapper.StateFor(context.Mappings(), context.BusinessUnitId, platformTypeId, stateCode, input.StateDescription);
			_mappedRule = stateMapper.RuleFor(context.Mappings(), context.BusinessUnitId, platformTypeId, stateCode, schedule.CurrentActivityId()) ?? new MappedRule();
			_stored = stored;
		}

		public bool StateGroupChanged()
		{
			return _mappedState.StateGroupId != _stored.StateGroupId();
		}

		public Guid? StateGroupId()
		{
			return _mappedState.StateGroupId;
		}

		public string StateGroupName()
		{
			return _mappedState.StateGroupName;
		}



		public bool HasRuleChanged()
		{
			return _mappedRule.RuleId != _stored.RuleId();
		}



		public Guid? RuleId()
		{
			return _mappedRule.RuleId;
		}

		public string RuleName()
		{
			return _mappedRule.RuleName;
		}

		public int? RuleDisplayColor()
		{
			return _mappedRule.DisplayColor;
		}

		public double? StaffingEffect()
		{
			return _mappedRule.StaffingEffect;
		}

		public Adherence? Adherence()
		{
			return _mappedRule.Adherence;
		}



		public bool IsAlarm()
		{
			return _mappedRule.IsAlarm;
		}

		public long AlarmThresholdTime()
		{
			return _mappedRule.ThresholdTime;
		}

		public int? AlarmColor()
		{
			return _mappedRule.AlarmColor;
		}


	}
}