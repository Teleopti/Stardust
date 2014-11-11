using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillStaffPeriodEvaluator
	{
		bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit);
		bool ResultIsWorse(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter);
		bool ResultIsWorseX(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit);
	}

	public class SkillStaffPeriodEvaluator : ISkillStaffPeriodEvaluator
	{
		//public bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter)
		//{
		//	foreach (var skillStaffPeriodAfter in listAfter)
		//	{
		//		var worse = isWorse(skillStaffPeriodAfter, listBefore);
		//		if (worse) return false;
		//	}

		//	if(listAfter.Count<listBefore.Count)
		//		return true;

		//	foreach (var skillStaffPeriodAfter in listAfter)
		//	{
		//		var better = isBetter(skillStaffPeriodAfter, listBefore);
		//		if (better) return true;
		//	}

		//	return false;
		//}

		public bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit)
		{
			var beforeTotals = 0d;
			var afterTotals = 0d;

			if (listAfter.Count == 0) return true;

			foreach (var skillStaffPeriod in listBefore)
			{
				beforeTotals += limit - skillStaffPeriod.IntraIntervalValue;
			}

			foreach (var skillStaffPeriod in listAfter)
			{
				afterTotals += limit - skillStaffPeriod.IntraIntervalValue;
			}

			return afterTotals < beforeTotals;
		}

		public bool ResultIsWorseX(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit)
		{
			var beforeTotals = 0d;
			var afterTotals = 0d;

			if (listAfter.Count == 0) return false;

			foreach (var skillStaffPeriod in listBefore)
			{
				beforeTotals += limit - skillStaffPeriod.IntraIntervalValue;
			}

			foreach (var skillStaffPeriod in listAfter)
			{
				afterTotals += limit - skillStaffPeriod.IntraIntervalValue;
			}

			return afterTotals > beforeTotals;
		}

		public bool ResultIsWorse(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter)
		{
			foreach (var skillStaffPeriodAfter in listAfter)
			{
				var worse = isWorse(skillStaffPeriodAfter, listBefore);
				if (worse) return true;
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
