using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class PrimarySkillOverstaff
	{
		public AvailableResourcesToMoveOnSkill Sum(ISkillStaffPeriodHolder skillStaffPeriodHolder, IEnumerable<CascadingSkillGroup> allSkillGroups, CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var allPrimarySkillsClosed = true;
			var dic = new Dictionary<ISkill, double>();
			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				ISkillStaffPeriod skillStaffPeriod;
				if (!skillStaffPeriodHolder.TryGetSkillStaffPeriod(primarySkill, interval, out skillStaffPeriod))
						continue; 
				allPrimarySkillsClosed = false;

				var resourcesOnOtherSkillGroups = 0d;
				foreach (var otherSkillGroup in allSkillGroups)
				{
					if (otherSkillGroup.Equals(skillGroup))
						continue;

					if (!otherSkillGroup.PrimarySkills.Contains(primarySkill))
						continue;

					var resourcesOnOtherSkillGroup = otherSkillGroup.Resources;
					foreach (var otherPrimarySkill in otherSkillGroup.PrimarySkills)
					{
						if (!otherPrimarySkill.Equals(primarySkill))
						{
							resourcesOnOtherSkillGroup -= skillStaffPeriodHolder.SkillStaffPeriodOrDefault(otherPrimarySkill, interval).CalculatedResource;
						}
					}
					resourcesOnOtherSkillGroups += resourcesOnOtherSkillGroup;
				}
				var forecast = skillStaffPeriod.FStaff;
				var overstaff = skillStaffPeriod.AbsoluteDifference;

				if(!overstaff.IsOverstaffed())
					continue;

				var otherSkillGroupOverstaff = Math.Max(resourcesOnOtherSkillGroups - forecast, 0);
				dic.Add(primarySkill, overstaff - otherSkillGroupOverstaff);
			}

			return new AvailableResourcesToMoveOnSkill(dic, allPrimarySkillsClosed);
		}
	}
}