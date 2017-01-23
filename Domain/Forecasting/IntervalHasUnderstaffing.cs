using log4net;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasUnderstaffing : Specification<IValidatePeriod>
    {
        private readonly ISkill _skill;
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntervalHasUnderstaffing));

		public IntervalHasUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(IValidatePeriod obj)
        {
	        var skillStaffignInterval = obj as SkillStaffingInterval;
	        if (skillStaffignInterval != null)
	        {
				logger.Info($"IsSatisfiedBy -- _skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {((SkillStaffingInterval)obj).SkillId} -- {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
			}
			return obj.RelativeDifference < _skill.StaffingThresholds.Understaffing.Value;
        }

    }
}