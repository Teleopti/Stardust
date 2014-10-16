using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillStaffPeriodEvaluator
	{
		bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter);
	}

	public class SkillStaffPeriodEvaluator
	{
		public bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter)
		{
			foreach (var skillStaffPeriodAfter in listAfter)
			{
				var worse = isWorse(skillStaffPeriodAfter, listBefore);
				if (worse) return false;
			}

			foreach (var skillStaffPeriodAfter in listAfter)
			{
				var better = isBetter(skillStaffPeriodAfter, listBefore);
				if (better) return true;
			}

			return false;
		}

		private bool isWorse(ISkillStaffPeriod skillStaffPeriodAfter, IEnumerable<ISkillStaffPeriod> listBefore)
		{
			var exists = false;
			foreach (var skillStaffPeriodBefore in listBefore)
			{
				if (!skillStaffPeriodBefore.Period.Equals(skillStaffPeriodAfter.Period)) continue;
				exists = true;
				if (skillStaffPeriodBefore.IntraIntervalValue > skillStaffPeriodAfter.IntraIntervalValue) return true;
			}

			return !exists;
		}

		private bool isBetter(ISkillStaffPeriod skillStaffPeriodAfter, IEnumerable<ISkillStaffPeriod> listBefore)
		{
			foreach (var skillStaffPeriodBefore in listBefore)
			{
				if (!skillStaffPeriodBefore.Period.Equals(skillStaffPeriodAfter.Period)) continue;
				if (skillStaffPeriodBefore.IntraIntervalValue < skillStaffPeriodAfter.IntraIntervalValue) return true;
			}

			return false;
		}
	}
}
