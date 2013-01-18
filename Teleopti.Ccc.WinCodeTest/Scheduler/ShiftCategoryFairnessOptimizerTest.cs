using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

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
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IGroupPersonsBuilder _groupPersonBuilder;


		[SetUp]
		public void Setup()
		{
			_bgWorker = new BackgroundWorker();
			_mocks = new MockRepository();
			_shiftCategoryFairnessAggregateManager = _mocks.StrictMock<IShiftCategoryFairnessAggregateManager>();
			_shiftCategoryFairnessSwapper = _mocks.DynamicMock<IShiftCategoryFairnessSwapper>();
			_shiftCategoryFairnessSwapFinder = _mocks.StrictMock<IShiftCategoryFairnessSwapFinder>();
			_groupPersonBuilder = _mocks.DynamicMock<IGroupPersonsBuilder>();
			_target = new ShiftCategoryFairnessOptimizer(_shiftCategoryFairnessAggregateManager, _shiftCategoryFairnessSwapper, _shiftCategoryFairnessSwapFinder,_groupPersonBuilder);
			_rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
		}

		[Test]
		public void ShouldSkipOutIfFair()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly>{dateOnly};
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var value = new ShiftCategoryFairnessCompareValue { Original = 1 };
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(
				new List<IShiftCategoryFairnessCompareResult> {compare1, compare2});
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
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
			var value = new ShiftCategoryFairnessCompareValue { Original = 1 };
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var list = new List<IShiftCategoryFairnessCompareResult> {compare1, compare2};
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
			Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupListOfSwaps(list, new List<IShiftCategoryFairnessSwap>())).Return(new List<IShiftCategoryFairnessSwap>());
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
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
			var value = new ShiftCategoryFairnessCompareValue { Original = 1 };
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
            Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupListOfSwaps(list, new List<IShiftCategoryFairnessSwap>())).Return(new List<IShiftCategoryFairnessSwap> { toSwap });
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes, _rollbackService, _bgWorker, true)).Return(false);
			//Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list, new List<IShiftCategoryFairnessSwap> { toSwap })).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
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
			var value = new ShiftCategoryFairnessCompareValue { Original = 1 };
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare3 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 2, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var list2 = new List<IShiftCategoryFairnessCompareResult> { compare1, compare3 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
            Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupListOfSwaps(list, new List<IShiftCategoryFairnessSwap>())).Return(new List<IShiftCategoryFairnessSwap> { toSwap });
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes, _rollbackService, _bgWorker, true)).Return(true);
			//second
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list2);
			//Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list2, new List<IShiftCategoryFairnessSwap>())).Return(null);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldAddToBlacklistIfWorse()
		{
			var persons = new List<IPerson>();
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var value = new ShiftCategoryFairnessCompareValue{Original = 1};
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare3 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 4, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var list2 = new List<IShiftCategoryFairnessCompareResult> { compare1, compare3 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list);
            Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupListOfSwaps(list, new List<IShiftCategoryFairnessSwap>())).Return(new List<IShiftCategoryFairnessSwap>{toSwap});
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes, _rollbackService, _bgWorker, true)).Return(true);
			//second
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetForGroups(persons, gropPage, dateOnly, days)).Return(list2);
			_mocks.ReplayAll();
			_target.Execute(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
			_mocks.VerifyAll();
		}
		[Test]
		public void ShouldRunForEachGroupPersonWhenPersonal()
		{
			var persons = new List<IPerson>{new Person()};
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly, dateOnly.AddDays(1) };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var value = new ShiftCategoryFairnessCompareValue { Original = 1 };
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare2 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 3, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var compare3 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 4, ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { value } };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1, compare2 };
			var list2 = new List<IShiftCategoryFairnessCompareResult> { compare1, compare3 };
			var toSwap = _mocks.DynamicMock<IShiftCategoryFairnessSwap>();
			var groupPerson = new GroupPerson(persons, dateOnly, "name", null);
			Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(dateOnly,persons,false,null)).Return(new List<IGroupPerson>{groupPerson});
			Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(dateOnly.AddDays(1), persons, false, null)).Return(new List<IGroupPerson> { groupPerson });
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, gropPage, dateOnly)).Return(list).IgnoreArguments();
            Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupListOfSwaps(list, new List<IShiftCategoryFairnessSwap>())).Return(new List<IShiftCategoryFairnessSwap>{toSwap});
			Expect.Call(_shiftCategoryFairnessSwapper.TrySwap(toSwap, dateOnly, matrixes, _rollbackService, _bgWorker, true)).Return(true);
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, gropPage, dateOnly)).Return(list2).IgnoreArguments();
			//Expect.Call(_shiftCategoryFairnessSwapFinder.GetGroupsToSwap(list2, new List<IShiftCategoryFairnessSwap> { toSwap })).Return(null);
			_mocks.ReplayAll();
			_target.ReportProgress += reportProgress;
			_target.ExecutePersonal(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
			_target.ReportProgress -= reportProgress;
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldStripOutNullList()
		{
			var persons = new List<IPerson> { new Person() };
			var dateOnly = new DateOnly(2012, 9, 21);
			var days = new List<DateOnly> { dateOnly };
			var matrixes = new List<IScheduleMatrixPro>();
			var gropPage = new GroupPageLight();
			var compare1 = new ShiftCategoryFairnessCompareResult { StandardDeviation = 0 };
			var list = new List<IShiftCategoryFairnessCompareResult> { compare1};
			
			var groupPerson = new GroupPerson(persons, dateOnly, "name", null);
			Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(dateOnly, persons, false, null)).Return(new List<IGroupPerson> { groupPerson });
			Expect.Call(_shiftCategoryFairnessAggregateManager.GetPerPersonsAndGroup(persons, gropPage, dateOnly)).Return(list).IgnoreArguments();
			_mocks.ReplayAll();
			_target.ReportProgress += reportProgress;
			_target.ExecutePersonal(_bgWorker, persons, days, matrixes, gropPage, _rollbackService, true);
			_target.ReportProgress -= reportProgress;
			_mocks.VerifyAll();
		}

		void reportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			Debug.WriteLine(e.Message);
			e.Cancel = true;
		}
	}

	
}