using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessOptimizer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays,
		                             IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage);
	}

	public class ShiftCategoryFairnessOptimizer : IShiftCategoryFairnessOptimizer
	{
		private readonly IShiftCategoryFairnessAggregateManager _shiftCategoryFairnessAggregateManager;
		private readonly IShiftCategoryFairnessSwapper _shiftCategoryFairnessSwapper;
		private readonly IShiftCategoryFairnessSwapFinder _shiftCategoryFairnessSwapFinder;

		public ShiftCategoryFairnessOptimizer(IShiftCategoryFairnessAggregateManager shiftCategoryFairnessAggregateManager,
			IShiftCategoryFairnessSwapper shiftCategoryFairnessSwapper, IShiftCategoryFairnessSwapFinder shiftCategoryFairnessSwapFinder)
		{
			_shiftCategoryFairnessAggregateManager = shiftCategoryFairnessAggregateManager;
			_shiftCategoryFairnessSwapper = shiftCategoryFairnessSwapper;
			_shiftCategoryFairnessSwapFinder = shiftCategoryFairnessSwapFinder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage)
		{
			// as we do this from left to right we don't need a list of days that we should not try again
			foreach (var selectedDay in selectedDays)
			{
				if (backgroundWorker.CancellationPending)
					return true;
				// run that day
				runDay(backgroundWorker, persons, selectedDays, selectedDay, matrixListForFairnessOptimization, groupPage);
			}
			return true;
		}

		private void runDay(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, DateOnly dateOnly,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage)
		{

			// we need a rollback service somewhere too
			var blackList = new List<IShiftCategoryFairnessSwap>();
			var fairnessResults =
				_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
					x => x.StandardDeviation).ToList();

			// if no diff it should be fair
			var diff = fairnessResults[fairnessResults.Count - 1].StandardDeviation - fairnessResults[0].StandardDeviation;
			if (diff.Equals(0))
				return;

			// get a suggestion (do until no more suggestions, what is returned then????)
			var swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
			if (swapSuggestion == null)
				return;
			do
			{
				if (backgroundWorker.CancellationPending)
					return;

				//try to swap, in this we will have another class that schedule those that can't be swapped (if the number of members differ)
				// it will keep track off the business rules too, if we brake some with the swap
				var success = _shiftCategoryFairnessSwapper.TrySwap(swapSuggestion, dateOnly, matrixListForFairnessOptimization);
				if (!success)
				{
					blackList.Add(swapSuggestion);
				}
				else
				{
					// success but did it get better
					fairnessResults =
					_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
						x => x.StandardDeviation).ToList();

					var newdiff = fairnessResults[fairnessResults.Count - 1].StandardDeviation - fairnessResults[0].StandardDeviation;
					if (newdiff > diff) // not better
					{
						blackList.Add(swapSuggestion);
						// do a rollback (if scheduled we need to resourcecalculate again??)
					}
					else
					{
						// if we did swap start all over again and we do this day until no more suggestions
						blackList = new List<IShiftCategoryFairnessSwap>();
					}
				}
				//get another one could we get stucked here with new suggestions all the time?
				swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
			} while (swapSuggestion != null);

		}
	}

	

	public interface IShiftCategoryFairnessSwapFinder
	{
		IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> inList, IList<IShiftCategoryFairnessSwap> blacklist);
	}
}