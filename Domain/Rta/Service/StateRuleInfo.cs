using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
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