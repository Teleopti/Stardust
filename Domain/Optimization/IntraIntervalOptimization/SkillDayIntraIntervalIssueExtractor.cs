﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillDayIntraIntervalIssueExtractor
	{
		IList<ISkillStaffPeriod> Extract(IList<ISkillDay> skillDays, ISkill skill);
	}

	public class SkillDayIntraIntervalIssueExtractor : ISkillDayIntraIntervalIssueExtractor
	{
		public IList<ISkillStaffPeriod> Extract(IList<ISkillDay> skillDays, ISkill skill)
		{
			var result = new List<ISkillStaffPeriod>();

			foreach (var skillDay in skillDays)
			{
				if (skillDay.Skill != skill) continue;

				foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
				{
					if (skillStaffPeriod.HasIntraIntervalIssue)
					{
						result.Add((ISkillStaffPeriod)skillStaffPeriod.NoneEntityClone());
					}
				}
			}

			return result;	
		}
	}
}
