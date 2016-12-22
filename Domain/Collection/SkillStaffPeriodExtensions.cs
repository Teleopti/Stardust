using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public static class SkillStaffPeriodExtensions
	{
		public static double ResourceLoggonOnDiff(this ISkillStaffPeriod skillStaffPeriod)
		{
			return skillStaffPeriod.CalculatedResource - skillStaffPeriod.CalculatedLoggedOn;
		}

		public static bool TryGetSkillStaffPeriod(this ISkillStaffPeriodHolder skillStaffPeriodHolder, ISkill skill, DateTimePeriod period, out ISkillStaffPeriod skillStaffPeriod)
		{
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
			if(skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out skillStaffPeriodDictionary))
			{
				if(skillStaffPeriodDictionary.TryGetValue(period, out skillStaffPeriod))
				{
					return true;
				}
			}
			skillStaffPeriod = null;
			return false;
		}

		public static void AddResources(this ISkillStaffPeriod skillStaffPeriod, double resourceToAdd)
		{
			var newValue = Math.Max(0, skillStaffPeriod.CalculatedResource + resourceToAdd);
			skillStaffPeriod.SetCalculatedResource65(newValue);
		}
	}
}