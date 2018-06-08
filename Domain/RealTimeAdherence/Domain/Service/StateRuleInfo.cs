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

		public bool IsLoggedIn() => StateGroupId().HasValue ? !_context.StateMapper.LoggedOutStateGroupIds().Contains(StateGroupId().Value): true;
		public bool IsLoggedOut() => !IsLoggedIn();

		public bool StateChanged() => _mappedState.Value?.StateGroupId != _context.Stored.StateGroupId;
		public Guid? StateGroupId() => _mappedState.Value?.StateGroupId;
		public string StateGroupName() => _mappedState.Value?.StateGroupName;

		public bool RuleChanged() => _mappedRule.Value?.RuleId != _context.Stored.RuleId;
		public Guid? RuleId() => _mappedRule.Value?.RuleId;
		public string RuleName() => _mappedRule.Value?.RuleName;
		public int? RuleDisplayColor() => _mappedRule.Value?.DisplayColor;
		public double? StaffingEffect() => _mappedRule.Value?.StaffingEffect;

		public bool IsAlarm() => _mappedRule.Value?.IsAlarm ?? false;
		public int AlarmThresholdTime() => _mappedRule.Value?.ThresholdTime ?? 0;
		public int? AlarmColor() => _mappedRule.Value?.AlarmColor;
	}
}