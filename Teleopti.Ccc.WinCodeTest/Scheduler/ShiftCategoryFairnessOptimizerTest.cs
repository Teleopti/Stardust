using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ShiftCategoryFairnessOptimizerTest
	{
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			
		}


	}

	public class ShiftCategoryFairnessOptimizer
	{
		private readonly IShiftCategoryFairnessAggregateManager _shiftCategoryFairnessAggregateManager;

		public ShiftCategoryFairnessOptimizer(IShiftCategoryFairnessAggregateManager shiftCategoryFairnessAggregateManager)
		{
			_shiftCategoryFairnessAggregateManager = shiftCategoryFairnessAggregateManager;
		}

		public bool Execute(BackgroundWorker backgroundWorker, IList<IPerson> persons, IList<DateOnly> selectedDays, 
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage)
		{
			// as we do this from left to right we don't need a list of days that we should not try again
			foreach (var selectedDay in selectedDays)
			{
				if (backgroundWorker.CancellationPending)
					return true;
				// run that day
				runDay(backgroundWorker,persons,selectedDays,selectedDay,matrixListForFairnessOptimization,groupPage);
			}
			return false;
		}

		private void runDay(BackgroundWorker backgroundWorker, IList<IPerson> persons,  IList<DateOnly> selectedDays, DateOnly dateOnly,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage)
		{

			// we need a rollback service somewhere too
			var blackList = new List<object>();
			var fairnessResults =
				_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
					x => x.StandardDeviation).ToList();

			// if no diff it should be fair (do it the other way around???)
			var diff = fairnessResults[0].StandardDeviation - fairnessResults[fairnessResults.Count - 1].StandardDeviation;
			if(diff.Equals(0))
				return;

			object suggestion = new object();
			
			// get a suggestion (do until no more suggestions, what is returned then????)
			// var suggestion = _theNewSuggestionFinder.GetSuggstion(fairnessResults, blackList)
			if (backgroundWorker.CancellationPending)
				return;

			bool success = false;
			//try to swap, in this we will have another class that schedule those that can't be swapped (if the number of members differ)
			// it will keep track off the business rules too, if we brake some with the swap
			// bool success = _theNewSwapService.TrySwap(suggestion,dateOnly, matrixListForWorkShiftOptimization);
			if(!success)
			{
				blackList.Add(suggestion);
				//get another one
				// var suggestion = _theNewSuggestionFinder.GetSuggstion(fairnessResults, blackList)
			}
			else
			{
				// success but did it get better
				fairnessResults =
				_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
					x => x.StandardDeviation).ToList();
				
				var newdiff = fairnessResults[0].StandardDeviation - fairnessResults[fairnessResults.Count - 1].StandardDeviation;
				if(newdiff > diff) // not better
				{
					blackList.Add(suggestion);
					// do a rollback (if scheduled we need to resourcecalculate again??)
				}
				else
				{
					// if we did swap start all over again and we do this day until no more suggestions
					// could we get stucked here with new suggestions all the time?
					blackList = new List<object>();
					fairnessResults = _shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays);
				}
				
			}
		}
	}
}