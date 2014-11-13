using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization
{
	public interface ISkillStaffPeriodEvaluator
	{
		bool ResultIsBetter(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit);
		bool ResultIsWorse(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit);
	}

	public class SkillStaffPeriodEvaluator : ISkillStaffPeriodEvaluator
	{
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

		public bool ResultIsWorse(IList<ISkillStaffPeriod> listBefore, IList<ISkillStaffPeriod> listAfter, double limit)
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
	}
}
