using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public interface ISkillStaffingIntervalProvider
	{
		List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod periodUtc, List<SkillCombinationResource> combinationResources, bool useShrinkage);
		IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution, bool useShrinkage);
		IList<SkillStaffingInterval> StaffingIntervalsForSkills(Guid[] skillIdList, DateTimePeriod period, bool useShrinkage, bool useBpoStaffing = true);
	}
}