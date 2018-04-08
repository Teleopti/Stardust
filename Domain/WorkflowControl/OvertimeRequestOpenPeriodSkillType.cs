using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class OvertimeRequestOpenPeriodSkillType : AggregateEntity, IOvertimeRequestOpenPeriodSkillType
	{
		private ISkillType _skillType;

		public OvertimeRequestOpenPeriodSkillType()
		{
		}

		public OvertimeRequestOpenPeriodSkillType(ISkillType skillType)
		{
			_skillType = skillType;
		}

		public virtual ISkillType SkillType
		{
			get => _skillType;
			set => _skillType = value;
		}

		public virtual IOvertimeRequestOpenPeriodSkillType EntityClone()
		{
			return (IOvertimeRequestOpenPeriodSkillType) MemberwiseClone();
		}

		public virtual IOvertimeRequestOpenPeriodSkillType NoneEntityClone()
		{
			var clone = (IOvertimeRequestOpenPeriodSkillType) MemberwiseClone();
			clone.SetId(null);
			return clone;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IOvertimeRequestOpenPeriodSkillType periodSKillType))
				return false;
			return periodSKillType.Parent.Equals(Parent)
				   && periodSKillType.SkillType.Equals(SkillType);
		}

		public override int GetHashCode()
		{
			return Parent.GetHashCode() & SkillType.GetHashCode();
		}
	}
}