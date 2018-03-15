using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasUnderstaffing : Specification<IValidatePeriod>
    {
        private readonly ISkill _skill;
		private static readonly ILog requestLogger = LogManager.GetLogger("Teleopti.Requests");

		public IntervalHasUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(IValidatePeriod obj)
        {
			var skillStaffPeriod = obj as SkillStaffPeriod;
	        if (skillStaffPeriod != null)
	        {
				requestLogger.Debug($"IsSatisfiedBy -- _skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {((SkillStaffPeriod) obj).SkillDay.Skill.Id} -- {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
	        }

			var skillStaffignInterval = obj as SkillStaffingInterval;
	        if (skillStaffignInterval != null)
	        {
				requestLogger.Debug($"IsSatisfiedBy -- _skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {((SkillStaffingInterval)obj).SkillId} -- {obj.DateTimePeriod} -- rel diff: {obj.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
			}
			return obj.RelativeDifference < _skill.StaffingThresholds.Understaffing.Value;
        }

    }
}