using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessOptimizer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, IList<IScheduleMatrixPro> matrixListForFairnessOptimization, 
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, bool useAverageShiftLengths);

		bool ExecutePersonal(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays,
		                     IList<IScheduleMatrixPro> matrixListForFairnessOptimization,
		                     IOptimizationPreferences optimizationPreferences,
		                     ISchedulePartModifyAndRollbackService rollbackService,
		                     bool useAverageShiftLength);	
		event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
	}

	public class ShiftCategoryFairnessOptimizer : IShiftCategoryFairnessOptimizer
	{
		private readonly IShiftCategoryFairnessAggregateManager _shiftCategoryFairnessAggregateManager;
		private readonly IShiftCategoryFairnessSwapper _shiftCategoryFairnessSwapper;
		private readonly IShiftCategoryFairnessSwapFinder _shiftCategoryFairnessSwapFinder;
		private readonly IGroupPersonsBuilder _groupPersonsBuilder;
		private bool _cancelMe;

		public ShiftCategoryFairnessOptimizer(IShiftCategoryFairnessAggregateManager shiftCategoryFairnessAggregateManager,
			IShiftCategoryFairnessSwapper shiftCategoryFairnessSwapper, IShiftCategoryFairnessSwapFinder shiftCategoryFairnessSwapFinder,
			IGroupPersonsBuilder groupPersonsBuilder)
		{
			_shiftCategoryFairnessAggregateManager = shiftCategoryFairnessAggregateManager;
			_shiftCategoryFairnessSwapper = shiftCategoryFairnessSwapper;
			_shiftCategoryFairnessSwapFinder = shiftCategoryFairnessSwapFinder;
			_groupPersonsBuilder = groupPersonsBuilder;
		}

		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void OnReportProgress(string message)
		{
			var handler = ReportProgress;
			if (handler != null)
			{
				var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
				handler(this, args);
				if (args.Cancel)
					_cancelMe = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ExecutePersonal(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, 
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			bool useAverageShiftLength)
		{
			// as we do this from left to right we don't need a list of days that we should not try again
			foreach (var selectedDay in selectedDays)
			{
				var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(selectedDay, persons, false, null);
				foreach (var groupPerson in groupPersons)
				{
					// run that day
					runDay(backgroundWorker, groupPerson.GroupMembers, selectedDays, selectedDay, matrixListForFairnessOptimization, optimizationPreferences, rollbackService, true, useAverageShiftLength);	
				}
				
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, IList<IScheduleMatrixPro> matrixListForFairnessOptimization, 
			IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService, bool useAverageShiftLengths)
		{
			foreach (var selectedDay in selectedDays)
			{
				// run that day
				runDay(backgroundWorker, persons, selectedDays, selectedDay, matrixListForFairnessOptimization, optimizationPreferences, rollbackService, false, useAverageShiftLengths);
			}
			return true;
		}

		private void runDay(BackgroundWorker backgroundWorker, IEnumerable<IPerson> persons, IList<DateOnly> selectedDays, DateOnly dateOnly,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			bool runPersonal, bool useAverageShiftLengths)
		{
			if (backgroundWorker.CancellationPending)
				return;
			if (_cancelMe)
				return;
			IList<IShiftCategoryFairnessCompareResult> fairnessResults;
			if(runPersonal)
				// if we run per person (in the team,group), not between teams 
				fairnessResults = _shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, optimizationPreferences.Extra.GroupPageOnCompareWith, dateOnly).OrderByDescending(
						x => x.StandardDeviation).ToList();
			else
				fairnessResults = _shiftCategoryFairnessAggregateManager.GetForGroups(persons, optimizationPreferences.Extra.GroupPageOnTeam, dateOnly, selectedDays).OrderByDescending(
					x => x.StandardDeviation).ToList();
			fairnessResults = stripOutAllZeros(fairnessResults);
			// if zero it should be fair
			var diff = fairnessResults.Sum(shiftCategoryFairnessCompareResult => shiftCategoryFairnessCompareResult.StandardDeviation);
			if (diff.Equals(0))
				return;
			var optFairnessOnDate = Resources.FairnessOptimizationOn + dateOnly.ToShortDateString(CultureInfo.CurrentCulture);
			OnReportProgress(optFairnessOnDate + Resources.FairnessOptimizationValueBefore + diff);
			
			var swapSuggestionList = _shiftCategoryFairnessSwapFinder.GetAllGroupsToSwap(fairnessResults);
            foreach (var swap in swapSuggestionList)
            {
                if (backgroundWorker.CancellationPending)
                    return;
				
				var success = _shiftCategoryFairnessSwapper.TrySwap(swap, dateOnly, matrixListForFairnessOptimization, rollbackService, backgroundWorker, useAverageShiftLengths, optimizationPreferences);
                if (!success)
                {
                    rollbackService.Rollback();
                }
                else
                {
                    // success but did it get better
                    if (runPersonal)
						fairnessResults = _shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, optimizationPreferences.Extra.GroupPageOnCompareWith, dateOnly).OrderBy(
                                x => x.StandardDeviation).ToList();
                    else
						fairnessResults = _shiftCategoryFairnessAggregateManager.GetForGroups(persons, optimizationPreferences.Extra.GroupPageOnTeam, dateOnly, selectedDays).OrderBy(
                            x => x.StandardDeviation).ToList();

                    fairnessResults = stripOutAllZeros(fairnessResults);
                    var newdiff = fairnessResults.Sum(shiftCategoryFairnessCompareResult => shiftCategoryFairnessCompareResult.StandardDeviation);
                    if (newdiff >= diff) // not better
                    {
                        OnReportProgress(optFairnessOnDate + Resources.FairnessOptimizationRollingBack);
                        // do a rollback (if scheduled we need to resourcecalculate again??)
                        rollbackService.Rollback();
                    }
                    else
                    {
                        diff = newdiff;
                        OnReportProgress(optFairnessOnDate + Resources.FairnessOptimizationValueAfter + diff);
                        // if we did swap start all over again and we do this day until no more suggestions
                        rollbackService.ClearModificationCollection();
                    }
                }
            }
            //do
            //{
            //    if (backgroundWorker.CancellationPending)
            //        return;

            //    var success = _shiftCategoryFairnessSwapper.TrySwap(swapSuggestion, dateOnly, matrixListForFairnessOptimization, rollbackService, backgroundWorker);
            //    if (!success)
            //    {
            //        blackList.Add(swapSuggestion);
            //        rollbackService.Rollback();
            //    }
            //    else
            //    {
            //        // success but did it get better
            //        if (runPersonal)
            //            fairnessResults = _shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, groupPage, dateOnly).OrderBy(
            //                    x => x.StandardDeviation).ToList();
            //        else
            //            fairnessResults = _shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
            //                x => x.StandardDeviation).ToList();

            //        fairnessResults = stripOutAllZeros(fairnessResults);
            //        var newdiff = fairnessResults.Sum(shiftCategoryFairnessCompareResult => shiftCategoryFairnessCompareResult.StandardDeviation);
            //        if (newdiff >= diff) // not better
            //        {
            //            OnReportProgress(optFairnessOnDate + Resources.FairnessOptimizationRollingBack);
            //            blackList.Add(swapSuggestion);
            //            // do a rollback (if scheduled we need to resourcecalculate again??)
            //            rollbackService.Rollback();
            //        }
            //        else
            //        {
            //            diff = newdiff;
            //            OnReportProgress(optFairnessOnDate + Resources.FairnessOptimizationValueAfter + diff);
            //            // if we did swap start all over again and we do this day until no more suggestions
            //            blackList = new List<IShiftCategoryFairnessSwap>();
            //            rollbackService.ClearModificationCollection();
            //        }
            //    }
            //    //get another one, could we get stucked here with new suggestions all the time?
            //    swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
            //} while (swapSuggestion != null);

		}

		private static IList<IShiftCategoryFairnessCompareResult> stripOutAllZeros(IEnumerable<IShiftCategoryFairnessCompareResult> fairnessCompareResults)
		{
			var ret = new List<IShiftCategoryFairnessCompareResult>();
			foreach (var shiftCategoryFairnessCompareResult in fairnessCompareResults)
			{
				if (shiftCategoryFairnessCompareResult.ShiftCategoryFairnessCompareValues == null)
					continue;
				var toRemove = new List<IShiftCategoryFairnessCompareValue>();
				foreach (var shiftCategoryFairnessCompareValue in shiftCategoryFairnessCompareResult.ShiftCategoryFairnessCompareValues)
				{
					if(shiftCategoryFairnessCompareValue.Original.Equals(0.0) && shiftCategoryFairnessCompareValue.ComparedTo.Equals(0.0))
						toRemove.Add(shiftCategoryFairnessCompareValue);
				}
				foreach (var shiftCategoryFairnessCompareValue in toRemove)
				{
					shiftCategoryFairnessCompareResult.ShiftCategoryFairnessCompareValues.Remove(shiftCategoryFairnessCompareValue);
				}
				if (shiftCategoryFairnessCompareResult.ShiftCategoryFairnessCompareValues.Any())
				{
					ret.Add(shiftCategoryFairnessCompareResult);
				}
			}
			return ret;
		}
	}

}