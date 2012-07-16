using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IPossibleCombinationsOfStartEndCategoryCreator
	{
		HashSet<IPossibleStartEndCategory> FindCombinations(IWorkTimeMinMax workTimeMinMax, ISchedulingOptions schedulingOptions);
	}

	public class PossibleCombinationsOfStartEndCategoryCreator : IPossibleCombinationsOfStartEndCategoryCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public HashSet<IPossibleStartEndCategory> FindCombinations(IWorkTimeMinMax workTimeMinMax, ISchedulingOptions schedulingOptions)
		{
			var ret = new HashSet<IPossibleStartEndCategory>();

			foreach (var poss in workTimeMinMax.PossibleStartEndCategories)
			{
				if(schedulingOptions.NotAllowedShiftCategories.Contains(poss.ShiftCategory))
					continue;
				var possible = new PossibleStartEndCategory();
				if (schedulingOptions.UseGroupSchedulingCommonStart)
					possible.StartTime = poss.StartTime;
				if (schedulingOptions.UseGroupSchedulingCommonEnd)
					possible.EndTime = poss.EndTime;
				if (schedulingOptions.UseGroupSchedulingCommonCategory )
					possible.ShiftCategory = poss.ShiftCategory;
				ret.Add(possible);
			}
			return ret;
		}

	}
}