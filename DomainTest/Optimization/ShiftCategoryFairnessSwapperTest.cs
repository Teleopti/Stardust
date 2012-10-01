using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ShiftCategoryFairnessSwapperTest
	{
		private MockRepository _mocks;

		private ISwapServiceNew _swapService;
		private ShiftCategoryFairnessSwapper _target;
		private ISchedulingResultStateHolder _resultState;
		private IShiftCategoryFairnessRescheduler _fairnessRescheduler;
		private IShiftCategoryChecker _shiftCatChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapService = _mocks.DynamicMock<ISwapServiceNew>();
			_resultState = _mocks.DynamicMock<ISchedulingResultStateHolder>();
			_fairnessRescheduler = _mocks.DynamicMock<IShiftCategoryFairnessRescheduler>();
			_shiftCatChecker = _mocks.DynamicMock<IShiftCategoryChecker>();
			
		}

		[Test]
		public void ShouldAssertSomething()
		{
			var dateOnly = new DateOnly(2012,10,1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();
			
			var group1 = new ShiftCategoryFairnessCompareResult {OriginalMembers = new List<IPerson> {person1}};
			var group2 = new ShiftCategoryFairnessCompareResult {OriginalMembers = new List<IPerson> {person2}};

			var suggestion = new ShiftCategoryFairnessSwap
			                 	{Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2};
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();
			var part2 = _mocks.DynamicMock<IScheduleDay>();

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay1}));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay2}));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part2, cat2)).Return(true);
			Expect.Call(rollbackService.ModifyParts(null)).Return(new BindingList<IBusinessRuleResponse>());
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessRescheduler, _shiftCatChecker);
			_target.TrySwap(suggestion, dateOnly, matrixes,rollbackService);
			_mocks.VerifyAll();
		}
	}

	[TestFixture]
	public class ShiftCategoryCheckerTest
	{
		private MockRepository _mocks;
		private ShiftCategoryChecker _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new ShiftCategoryChecker();
		}

		[Test]
		public void ShouldReturnFalseIfNotMainShift()
		{
			var part = _mocks.DynamicMock<IScheduleDay>();
			var cat = new ShiftCategory("katten");
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.Overtime);

			_mocks.ReplayAll();
			Assert.That(_target.DayHasShiftCategory(part, cat), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfMainShiftAndCorrectCategory()
		{
			var part = _mocks.DynamicMock<IScheduleDay>();
			var cat = new ShiftCategory("katten");
			var personAss = _mocks.DynamicMock<IPersonAssignment>();
			var mainShift = _mocks.DynamicMock<IMainShift>();
			
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(part.PersonAssignmentCollection()).Return(
				new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {personAss}));
			Expect.Call(personAss.MainShift).Return(mainShift);
			Expect.Call(mainShift.ShiftCategory).Return(cat);
			_mocks.ReplayAll();
			Assert.That(_target.DayHasShiftCategory(part, cat), Is.True);
			_mocks.VerifyAll();
		}
	}
}