﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessOptimizer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
			IGroupPageLight groupPage, ISchedulePartModifyAndRollbackService rollbackService);
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
		public bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, IList<IScheduleMatrixPro> matrixListForFairnessOptimization, 
			IGroupPageLight groupPage, ISchedulePartModifyAndRollbackService rollbackService)
		{
			// as we do this from left to right we don't need a list of days that we should not try again
			foreach (var selectedDay in selectedDays)
			{
				if (backgroundWorker.CancellationPending)
					return true;
				// run that day
				runDay(backgroundWorker, persons, selectedDays, selectedDay, matrixListForFairnessOptimization, groupPage, rollbackService);
			}
			return true;
		}

		private void runDay(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, DateOnly dateOnly,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var blackList = new List<IShiftCategoryFairnessSwap>();
			var fairnessResults =
				_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
					x => x.StandardDeviation).ToList();

			// if zero it should be fair
			var diff = fairnessResults[fairnessResults.Count - 1].StandardDeviation;
			if (diff.Equals(0))
				return;

			var swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
			if (swapSuggestion == null)
				return;
			do
			{
				if (backgroundWorker.CancellationPending)
					return;

				var success = _shiftCategoryFairnessSwapper.TrySwap(swapSuggestion, dateOnly, matrixListForFairnessOptimization, rollbackService, backgroundWorker);
				if (!success)
				{
					blackList.Add(swapSuggestion);
					rollbackService.Rollback();
				}
				else
				{
					// success but did it get better
					fairnessResults =
					_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
						x => x.StandardDeviation).ToList();

					var newdiff = fairnessResults[fairnessResults.Count - 1].StandardDeviation; 
					if (newdiff >= diff) // not better
					{
						blackList.Add(swapSuggestion);
						// do a rollback (if scheduled we need to resourcecalculate again??)
						rollbackService.Rollback();
					}
					else
					{
						// if we did swap start all over again and we do this day until no more suggestions
						blackList = new List<IShiftCategoryFairnessSwap>();
						rollbackService.ClearModificationCollection();
					}
				}
				//get another one, could we get stucked here with new suggestions all the time?
				swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
			} while (swapSuggestion != null);

		}
	}

}