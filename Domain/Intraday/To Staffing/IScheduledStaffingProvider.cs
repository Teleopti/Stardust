using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IScheduledStaffingProvider
	{
		IList<SkillStaffingInterval> StaffingPerSkill(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage = false, bool useBpoStaffing = true);
		IList<SkillStaffingIntervalLightModel> StaffingPerSkill(IList<ISkill> skills, int minutesPerInterval, DateOnly? dateOnly = null, bool useShrinkage = false);
	}
}