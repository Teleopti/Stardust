using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IPossibleCombinationsOfStartEndCategoryCreator
	{
		HashSet<PossibleStartEndCategory> FindCombinations(IWorkTimeMinMax workTimeMinMax, ISchedulingOptions schedulingOptions);
	}

	public class PossibleCombinationsOfStartEndCategoryCreator : IPossibleCombinationsOfStartEndCategoryCreator
	{
		public HashSet<PossibleStartEndCategory> FindCombinations(IWorkTimeMinMax workTimeMinMax, ISchedulingOptions schedulingOptions)
		{
			var ret = new HashSet<PossibleStartEndCategory>();

			foreach (var poss in workTimeMinMax.PossibleStartEndCategories)
			{
				var possible = new PossibleStartEndCategory();
				if (schedulingOptions.UseGroupSchedulingCommonStart)
					possible.StartTime = poss.StartTime;
				if (schedulingOptions.UseGroupSchedulingCommonEnd)
					possible.EndTime = poss.EndTime;
				if (schedulingOptions.UseGroupSchedulingCommonCategory)
					possible.ShiftCategory = poss.ShiftCategory;
				ret.Add(possible);
			}
			return ret;
		}

	}
}