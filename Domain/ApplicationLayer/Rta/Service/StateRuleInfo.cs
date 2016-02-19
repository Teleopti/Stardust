using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateRuleInfo
	{
		private readonly StateMapping _stateMapping;
		private readonly RuleMapping _ruleMapping;
		private readonly StoredStateInfo _stored;

		public StateRuleInfo(
			string stateCode,
			Guid platformTypeId,
			ExternalUserStateInputModel input,
			PersonOrganizationData person,
			StoredStateInfo stored,
			ScheduleInfo schedule,
			IStateMapper stateMapper
			)
		{
			_stateMapping = stateMapper.StateFor(person.BusinessUnitId, platformTypeId, stateCode, input.StateDescription);
			_ruleMapping = stateMapper.RuleFor(person.BusinessUnitId, platformTypeId, stateCode, schedule.CurrentActivityId()) ?? new RuleMapping();
			_stored = stored;
		}

		public bool StateGroupChanged()
		{
			return _stateMapping.StateGroupId != _stored.StateGroupId();
		}

		public Guid? StateGroupId()
		{
			return _stateMapping.StateGroupId;
		}

		public string StateGroupName()
		{
			return _stateMapping.StateGroupName;
		}



		public bool HasRuleChanged()
		{
			return _ruleMapping.RuleId != _stored.RuleId();
		}



		public Guid? RuleId()
		{
			return _ruleMapping.RuleId;
		}

		public string RuleName()
		{
			return _ruleMapping.RuleName;
		}

		public int? RuleDisplayColor()
		{
			return _ruleMapping.DisplayColor;
		}

		public double? StaffingEffect()
		{
			return _ruleMapping.StaffingEffect;
		}

		public Adherence? Adherence()
		{
			return _ruleMapping.Adherence;
		}



		public bool IsAlarm()
		{
			return _ruleMapping.IsAlarm;
		}

		public long AlarmThresholdTime()
		{
			return _ruleMapping.ThresholdTime;
		}

		public int? AlarmColor()
		{
			return _ruleMapping.AlarmColor;
		}


	}
}