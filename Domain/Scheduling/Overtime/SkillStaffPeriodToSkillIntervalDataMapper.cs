using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface ISkillStaffPeriodToSkillIntervalDataMapper
	{
		IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriod);
	}

	public class SkillStaffPeriodToSkillIntervalDataMapper : ISkillStaffPeriodToSkillIntervalDataMapper
	{
		public IList<ISkillIntervalData> MapSkillIntervalData(IList<ISkillStaffPeriod> skillStaffPeriodList)
		{
			var skillIntervalList = new List<ISkillIntervalData>();
			if (skillStaffPeriodList != null)
				foreach (var skillStaffPeriod in skillStaffPeriodList)
				{
					skillIntervalList.Add(new SkillIntervalData(skillStaffPeriod.Period, skillStaffPeriod.RelativeDifference));
				}
			return skillIntervalList;
		}
	}
}