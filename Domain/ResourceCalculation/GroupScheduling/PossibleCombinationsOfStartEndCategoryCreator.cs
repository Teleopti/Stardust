using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class PossibleCombinationsOfStartEndCategoryCreator
	{
		private readonly ISchedulingOptions _schedulingOptions;

		public PossibleCombinationsOfStartEndCategoryCreator(ISchedulingOptions schedulingOptions)
		{
			_schedulingOptions = schedulingOptions;
		}

		public HashSet<PossibleStartEndCategory> FindCombinations(IList<IShiftProjectionCache> shiftProjectionCaches)
		{
			var ret = new HashSet<PossibleStartEndCategory>();

			foreach (var shiftProjectionCach in shiftProjectionCaches)
			{
				var possible = new PossibleStartEndCategory();
				if (_schedulingOptions.UseGroupSchedulingCommonStart)
					possible.StartTime = shiftProjectionCach.MainShiftStartDateTime.TimeOfDay;
				if (_schedulingOptions.UseGroupSchedulingCommonEnd)
					possible.EndTime = shiftProjectionCach.MainShiftEndDateTime.TimeOfDay;
				if (_schedulingOptions.UseGroupSchedulingCommonCategory)
					possible.ShiftCategory = shiftProjectionCach.TheMainShift.ShiftCategory;
				ret.Add(possible);
			}
			return ret;
		}
		//

		public HashSet<PossibleStartEndCategory> FindCombinations(IWorkTimeMinMax workTimeMinMax)
		{
			var ret = new HashSet<PossibleStartEndCategory>();

			foreach (var poss in workTimeMinMax.PossibleStartEndCategories)
			{
				var possible = new PossibleStartEndCategory();
				if (_schedulingOptions.UseGroupSchedulingCommonStart)
					possible.StartTime = poss.StartTime;
				if (_schedulingOptions.UseGroupSchedulingCommonEnd)
					possible.EndTime = poss.EndTime;
				if (_schedulingOptions.UseGroupSchedulingCommonCategory)
					possible.ShiftCategory = poss.ShiftCategory;
				ret.Add(possible);
			}
			return ret;
		}

	}
}