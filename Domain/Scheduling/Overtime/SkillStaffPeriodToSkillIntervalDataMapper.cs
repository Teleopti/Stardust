using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeSkillStaffPeriodToSkillIntervalDataMapper
	{
		IList<IOvertimeSkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod);
	}

	public class OvertimeSkillStaffPeriodToSkillIntervalDataMapper : IOvertimeSkillStaffPeriodToSkillIntervalDataMapper
	{
		public IList<IOvertimeSkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriodList)
		{
			var skillIntervalList = new List<IOvertimeSkillIntervalData>();
			if (skillStaffPeriodList != null)
				foreach (var skillStaffPeriod in skillStaffPeriodList)
				{
					skillIntervalList.Add(new OvertimeSkillIntervalData(skillStaffPeriod.Period, skillStaffPeriod.RelativeDifference));
				}
			return skillIntervalList;
		}
	}
}