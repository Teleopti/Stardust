using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public AvailableResourcesToMoveOnSkill AvailableSum(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> allSkillGroups, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var primarySkillsExistsButTheyAreAllClosed = true;
			var dic = new Dictionary<ISkill, double>();
			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				ISkillStaffPeriod skillStaffPeriod;
				if (!skillStaffPeriodHolder.TryGetSkillStaffPeriod(primarySkill, interval, out skillStaffPeriod))
						continue; 
				primarySkillsExistsButTheyAreAllClosed = false;
				var forecast = skillStaffPeriod.FStaff;
				var overstaff = skillStaffPeriod.AbsoluteDifference;
				if (!overstaff.IsOverstaffed())
					continue;

				var resourcesOnOtherSkillGroups = 0d;
				foreach (var otherSkillGroup in allSkillGroups.Where(x => !x.Equals(skillGroup) && x.PrimarySkills.Contains(primarySkill)))
				{
					var resourcesOnOtherSkillGroup = otherSkillGroup.Resources;
					foreach (var otherPrimarySkill in otherSkillGroup.PrimarySkills.Where(x => !x.Equals(primarySkill)))
					{
						resourcesOnOtherSkillGroup -= skillStaffPeriodHolder.SkillStaffPeriodOrDefault(otherPrimarySkill, interval).CalculatedResource;
					}
					resourcesOnOtherSkillGroups += resourcesOnOtherSkillGroup;
				}

				var otherSkillGroupOverstaff = Math.Max(resourcesOnOtherSkillGroups - forecast, 0);
				dic.Add(primarySkill, overstaff - otherSkillGroupOverstaff);
			}

			return new AvailableResourcesToMoveOnSkill(dic, primarySkillsExistsButTheyAreAllClosed);
		}
	}
}