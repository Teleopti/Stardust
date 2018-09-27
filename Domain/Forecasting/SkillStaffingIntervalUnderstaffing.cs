using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class SkillStaffingIntervalUnderstaffing : ISkillStaffingIntervalUnderstaffing
	{
		public bool IsSatisfiedBy(ISkill skill, IValidatePeriod validatePeriod)
		{
			var understaffingValue = skill.StaffingThresholds.Understaffing.Value;
			var isSatified = validatePeriod.RelativeDifference <= understaffingValue;
			return isSatified;
		}
	}
}