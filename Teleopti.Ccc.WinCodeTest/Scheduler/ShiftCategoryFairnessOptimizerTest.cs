using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class ShiftCategoryFairnessOptimizerTest
	{
		private MockRepository _mocks;
		private IShiftCategoryFairnessAggregateManager _shiftCategoryFairnessAggregateManager;
		private IShiftCategoryFairnessSwapper _shiftCategoryFairnessSwapper;
		private IShiftCategoryFairnessSwapFinder _shiftCategoryFairnessSwapFinder;
		private ShiftCategoryFairnessOptimizer _target;
		private BackgroundWorker _bgWorker;

		[SetUp]
		public void Setup()
		{
			_bgWorker = new BackgroundWorker();
			_mocks = new MockRepository();
			_shiftCategoryFairnessAggregateManager = _mocks.StrictMock<IShiftCategoryFairnessAggregateManager>();
			_shiftCategoryFairnessSwapper = _mocks.DynamicMock<IShiftCategoryFairnessSwapper>();
			_shiftCategoryFairnessSwapFinder = _mocks.StrictMock<IShiftCategoryFairnessSwapFinder>();
			_target = new ShiftCategoryFairnessOptimizer(_shiftCategoryFairnessAggregateManager, _shiftCategoryFairnessSwapper, _shiftCategoryFairnessSwapFinder);

		}

		[Test]
		public void ShouldSkipOutIfFair()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly>{dateOnly};
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult{StandardDeviation = 0};
			var compare2 = new ShiftCategoryFairnessCompareResult{StandardDeviation = 0};

			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(
				new List<IShiftCategoryFairnessCompareResult> {compare1, compare2});
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSkipOutIfNoSuggestion()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0 };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3 };
			var list = new List<IShiftCategoryFairnessCompareResult> {compare1, compare2};
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap>())).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldTrySwapSuggestion()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0 };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3 };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap>())).Return(toSwap);
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes)).Return(false);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap> { toSwap })).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldTryAgainToAnotherSwapSuggestion()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0 };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3 };
			var compare3 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 2 };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var list2 = new List<IShiftCategoryFairnessCompareResult> { compare1, compare3 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap>())).Return(toSwap);
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes)).Return(true);
			//second
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list2);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list2, new List<IShiftCategoryFairnessSwap>())).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddToBlackListIfWorse()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0 };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3 };
			var compare3 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 4 };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var list2 = new List<IShiftCategoryFairnessCompareResult> { compare1, compare3 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap>())).Return(toSwap);
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes)).Return(true);
			//second
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list2);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list2, new List<IShiftCategoryFairnessSwap>{toSwap})).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage);
			_mocks.VerifyAll();
		}
	}

	public class ShiftCategoryFairnessOptimizer
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
				runDay(backgroundWorker,persons,selectedDays,selectedDay,matrixListForFairnessOptimization,groupPage);
			}
			return true;
		}

		private void runDay(BackgroundWorker backgroundWorker, IList<IPerson> persons,  IList<DateOnly> selectedDays, DateOnly dateOnly,
			IList<IScheduleMatrixPro> matrixListForFairnessOptimization, IGroupPageLight groupPage)
		{

			// we need a rollback service somewhere too
			var blackList = new List<IShiftCategoryFairnessSwap>();
			var fairnessResults =
				_shiftCategoryFairnessAggregateManager.GetForGroups(persons, groupPage, dateOnly, selectedDays).OrderBy(
					x => x.StandardDeviation).ToList();

			// if no diff it should be fair
			var diff = fairnessResults[fairnessResults.Count - 1].StandardDeviation - fairnessResults[0].StandardDeviation;
			if(diff.Equals(0))
				return;

			// get a suggestion (do until no more suggestions, what is returned then????)
			var swapSuggestion = _shiftCategoryFairnessSwapFinder.GetGroupsToSwap(fairnessResults, blackList);
			if(swapSuggestion == null)
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

	public interface IShiftCategoryFairnessSwap
	{
		 ShiftCategoryFairnessCompareResult Group1 { get; set; }
		 ShiftCategoryFairnessCompareResult Group2 { get; set; }
		 IShiftCategory ShiftCategoryFromGroup1 { get; set; }
		 IShiftCategory ShiftCategoryFromGroup2 { get; set; }
	}

	public interface IShiftCategoryFairnessSwapFinder
	{
		IShiftCategoryFairnessSwap GetGroupsToSwap(IList<IShiftCategoryFairnessCompareResult> inList, IList<IShiftCategoryFairnessSwap> blacklist);
	}
}