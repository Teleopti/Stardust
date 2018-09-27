using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ISkillStaffingIntervalUnderstaffing
	{
		bool IsSatisfiedBy(ISkill skill, IValidatePeriod validatePeriod);
	}
}