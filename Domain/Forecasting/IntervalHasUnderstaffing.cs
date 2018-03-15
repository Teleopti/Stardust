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

        public override bool IsSatisfiedBy(IValidatePeriod validatePeriod)
		{
			logIfDebugIsEnabled(validatePeriod);
			return validatePeriod.RelativeDifference < _skill.StaffingThresholds.Understaffing.Value;
        }

		private void logIfDebugIsEnabled(IValidatePeriod validatePeriod)
		{
			if(validatePeriod.DateTimePeriod.StartDateTime == validatePeriod.DateTimePeriod.EndDateTime)
				requestLogger.Debug(System.Environment.StackTrace);
			
			switch (validatePeriod)
			{
				case SkillStaffPeriod skillStaffPeriod:
					requestLogger.Debug($"IsSatisfiedBy(SkillStaffPeriod) -- " +
										$"_skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {skillStaffPeriod.SkillDay.Skill.Id} -- " +
										$"{validatePeriod.DateTimePeriod} -- rel diff: {validatePeriod.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
					break;
				case SkillStaffingInterval skillStaffingInterval:
					requestLogger.Debug($"IsSatisfiedBy(SkillStaffingInterval) -- " +
										$"_skill: {_skill.Name}, _skill.Id: {_skill.Id}  obj.skill {skillStaffingInterval.SkillId} -- " +
										$"{validatePeriod.DateTimePeriod} -- rel diff: {validatePeriod.RelativeDifference} -- threshold {_skill.StaffingThresholds.Understaffing.Value}");
					break;
			}
		}

    }
}