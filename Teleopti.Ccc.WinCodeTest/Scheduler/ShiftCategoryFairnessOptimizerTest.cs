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

	
}