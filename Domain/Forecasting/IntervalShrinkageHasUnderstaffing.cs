using log4net;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class IntervalShrinkageHasUnderstaffing : Specification<IValidatePeriod>
	{
		private readonly ISkill _skill;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntervalShrinkageHasUnderstaffing));

		public IntervalShrinkageHasUnderstaffing(ISkill skill)
		{
			_skill = skill;
		}

		public override bool IsSatisfiedBy(IValidatePeriod obj)
		{
			var skillStaffPeriod = obj as SkillStaffPeriod;
			if (skillStaffPeriod != null)
			{
				logger.Info($"IsSatisfiedBy -- _skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {((SkillStaffPeriod)obj).SkillDay.Skill.Id} -- {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
			}

			var skillStaffignInterval = obj as SkillStaffingInterval;
			if (skillStaffignInterval != null)
			{
				logger.Info($"IsSatisfiedBy -- _skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {((SkillStaffingInterval)obj).SkillId} -- {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
			}
			return obj.RelativeDifferenceWithShrinkage < _skill.StaffingThresholds.Understaffing.Value;
		}

	}
}