using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public abstract class OvertimeRequestOpenPeriod : AggregateEntity, IOvertimeRequestOpenPeriod
	{
		private int _orderIndex;
		private OvertimeRequestAutoGrantType _autoGrantType;
		private bool _enableWorkRuleValidation;
		private OvertimeValidationHandleType? _workRuleValidationHandleType;
		private ISkillType _skillType;
		private IList<IOvertimeRequestOpenPeriodSkillType> _periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>();
		public abstract DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);

		public virtual OvertimeRequestAutoGrantType AutoGrantType
		{
			get => _autoGrantType;
			set => _autoGrantType = value;
		}

		public virtual bool EnableWorkRuleValidation
		{
			get => _enableWorkRuleValidation;
			set => _enableWorkRuleValidation = value;
		}

		public virtual OvertimeValidationHandleType? WorkRuleValidationHandleType
		{
			get => _workRuleValidationHandleType;
			set => _workRuleValidationHandleType = value;
		}

		public virtual IReadOnlyCollection<IOvertimeRequestOpenPeriodSkillType> PeriodSkillTypes =>
			new ReadOnlyCollection<IOvertimeRequestOpenPeriodSkillType>(_periodSkillTypes);

		public virtual int OrderIndex
		{
			get
			{
				var owner = Parent as IWorkflowControlSet;
				_orderIndex = owner?.OvertimeRequestOpenPeriods.IndexOf(this) ?? -1;
				return _orderIndex;
			}
			set => _orderIndex = value;
		}

		public virtual string DenyReason { get; set; }

		public virtual ISkillType SkillType
		{
			get => _skillType;
			set => _skillType = value;
		}

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual IOvertimeRequestOpenPeriod NoneEntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			clone.SetId(null);
			var originPeriodSkillTypes = clone.PeriodSkillTypes;

			clone.resetPeriodSkillTypes();
			foreach (var openPeriodSkillType in originPeriodSkillTypes)
			{
				var periodSkillTypeClone = openPeriodSkillType.NoneEntityClone();
				periodSkillTypeClone.SetParent(clone);
				clone.AddSkillType(periodSkillTypeClone);
			}

			return clone;
		}

		public virtual IOvertimeRequestOpenPeriod EntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			var originPeriodSkillTypes = clone.PeriodSkillTypes;

			clone.resetPeriodSkillTypes();
			foreach (var openPeriodSkillType in originPeriodSkillTypes)
			{
				var periodSkillTypeClone = openPeriodSkillType.EntityClone();
				periodSkillTypeClone.SetParent(clone);
				clone.AddSkillType(periodSkillTypeClone);
			}

			return clone;
		}

		public virtual void AddSkillType(IOvertimeRequestOpenPeriodSkillType skillType)
		{
			skillType.SetParent(this);
			_periodSkillTypes.Add(skillType);
		}

		public virtual void ClearSkillType()
		{
			while (_periodSkillTypes.Count > 0)
			{
				_periodSkillTypes.RemoveAt(0);
			}
			_periodSkillTypes = null;
			_periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>();
		}

		private void resetPeriodSkillTypes()
		{
			_periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>();
		}
	}

	public enum OvertimeRequestAutoGrantType
	{
		No,
		Yes,
		Deny
	}

	public class OvertimeRequestOpenRollingPeriod : OvertimeRequestOpenPeriod
	{
		private MinMax<int> _betweenDays;
		public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
		{
			return new DateOnlyPeriod(viewpointDateOnly.AddDays(_betweenDays.Minimum), viewpointDateOnly.AddDays(_betweenDays.Maximum));
		}

		public virtual MinMax<int> BetweenDays
		{
			get => _betweenDays;
			set => _betweenDays = value;
		}
	}

	public class OvertimeRequestOpenDatePeriod : OvertimeRequestOpenPeriod
	{
		private DateOnlyPeriod _period;
		public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
		{
			return _period;
		}

		public virtual DateOnlyPeriod Period
		{
			get => _period;
			set => _period = value;
		}
	}
}
