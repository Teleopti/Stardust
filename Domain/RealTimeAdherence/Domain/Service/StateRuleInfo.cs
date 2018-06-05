using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class StateRuleInfo
	{
		private readonly Lazy<MappedRule> _mappedRule;
		private readonly Lazy<MappedState> _mappedState;
		private readonly Context _context;

		public StateRuleInfo(Context context)
		{
			_context = context;
			_mappedState = new Lazy<MappedState>(() =>
					context.HasInput() ?
						context.StateMapper.StateFor(context.BusinessUnitId, context.InputStateCode(), context.InputStateDescription()) :
						context.StateMapper.StateFor(context.Stored.StateGroupId)
			);
			_mappedRule = new Lazy<MappedRule>(() => context.StateMapper.RuleFor(context.BusinessUnitId, context.State.StateGroupId(), context.Schedule.CurrentActivityId()));
		}

		public bool IsLoggedIn()
		{
			var stateGroupId = StateGroupId();
			
			//TODO: Look into. we are assuming that if there is no stategroup the person is logged in 
			if (stateGroupId == null)
				return true;
			
			//TODO: Look into. we are also assuming that if there is nothing in LoggedOutStateGroupIds the person is logged in
			return !_context.StateMapper.LoggedOutStateGroupIds().Contains(stateGroupId.Value); 
		}
		
		public bool StateChanged()
		{
			return _mappedState.Value?.StateGroupId != _context.Stored.StateGroupId;
		}

		public Guid? StateGroupId()
		{
			return _mappedState.Value?.StateGroupId;
		}

		public string StateGroupName()
		{
			return _mappedState.Value?.StateGroupName;
		}



		public bool RuleChanged()
		{
			return _mappedRule.Value?.RuleId != _context.Stored.RuleId;
		}

		public Guid? RuleId()
		{
			return _mappedRule.Value?.RuleId;
		}

		public string RuleName()
		{
			return _mappedRule.Value?.RuleName;
		}

		public int? RuleDisplayColor()
		{
			return _mappedRule.Value?.DisplayColor;
		}

		public double? StaffingEffect()
		{
			return _mappedRule.Value?.StaffingEffect;
		}

		public Adherence? Adherence()
		{
			return _mappedRule.Value?.Adherence;
		}



		public bool IsAlarm()
		{
			return _mappedRule.Value?.IsAlarm ?? false;
		}

		public int AlarmThresholdTime()
		{
			return _mappedRule.Value?.ThresholdTime ?? 0;
		}

		public int? AlarmColor()
		{
			return _mappedRule.Value?.AlarmColor;
		}


	}
}