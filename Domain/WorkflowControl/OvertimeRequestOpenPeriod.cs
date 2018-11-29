using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public abstract class OvertimeRequestOpenPeriod : AggregateEntity, IOvertimeRequestOpenPeriod
	{
		private int _orderIndex;
		private OvertimeRequestAutoGrantType _autoGrantType;
		private bool _enableWorkRuleValidation;
		private OvertimeValidationHandleType? _workRuleValidationHandleType;
		private IList<IOvertimeRequestOpenPeriodSkillType> _periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>();

		protected OvertimeRequestOpenPeriod()
		{
		}

		protected OvertimeRequestOpenPeriod(IEnumerable<ISkillType> skillTypes)
		{
			foreach (var skillType in skillTypes)
			{
				AddPeriodSkillType(new OvertimeRequestOpenPeriodSkillType(skillType));
			}
		}

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

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual IOvertimeRequestOpenPeriod NoneEntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			clone.SetId(null);
			clone._periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>(_periodSkillTypes);
			return clone;
		}

		public virtual IOvertimeRequestOpenPeriod EntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			clone._periodSkillTypes = new List<IOvertimeRequestOpenPeriodSkillType>(_periodSkillTypes);
			return clone;
		}

		public virtual void AddPeriodSkillType(IOvertimeRequestOpenPeriodSkillType periodSkillType)
		{
			periodSkillType.SetParent(this);
			_periodSkillTypes.Add(periodSkillType);
		}

		public virtual void ClearPeriodSkillType()
		{
			while(_periodSkillTypes.Count > 0)
			{
				_periodSkillTypes.Remove(_periodSkillTypes.First());
			}
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

		public OvertimeRequestOpenRollingPeriod()
		{
		}

		public OvertimeRequestOpenRollingPeriod(IEnumerable<ISkillType> skillTypes) : base(skillTypes)
		{
		}

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

		public OvertimeRequestOpenDatePeriod()
		{
		}

		public OvertimeRequestOpenDatePeriod(IEnumerable<ISkillType> skillTypes) : base(skillTypes)
		{
		}

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
