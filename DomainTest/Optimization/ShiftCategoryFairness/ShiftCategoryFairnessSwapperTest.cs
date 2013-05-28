using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
	[TestFixture]
	public class ShiftCategoryFairnessSwapperTest
	{
		private MockRepository _mocks;

		private ISwapServiceNew _swapService;
		private ShiftCategoryFairnessSwapper _target;
		private ISchedulingResultStateHolder _resultState;
		private IShiftCategoryFairnessReScheduler _fairnessReScheduler;
		private IShiftCategoryChecker _shiftCatChecker;
		private IDeleteSchedulePartService _deleteService;
		private IShiftCategoryFairnessPersonsSwappableChecker _swappableChecker;
		private IShiftCategoryFairnessContractToleranceChecker _shiftCategoryFairnessContractToleranceChecker;
		private IOptimizationOverLimitByRestrictionDeciderCreator _optimizationOverLimitByRestrictionDeciderCreator;
		private OptimizationPreferences _optimizationPreferences;
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapService = _mocks.DynamicMock<ISwapServiceNew>();
			_resultState = _mocks.DynamicMock<ISchedulingResultStateHolder>();
			_fairnessReScheduler = _mocks.DynamicMock<IShiftCategoryFairnessReScheduler>();
			_shiftCatChecker = _mocks.DynamicMock<IShiftCategoryChecker>();
			_deleteService = _mocks.DynamicMock<IDeleteSchedulePartService>();
			_swappableChecker = _mocks.DynamicMock<IShiftCategoryFairnessPersonsSwappableChecker>();
			_shiftCategoryFairnessContractToleranceChecker = _mocks.DynamicMock<IShiftCategoryFairnessContractToleranceChecker>();
			_optimizationOverLimitByRestrictionDeciderCreator =
				_mocks.StrictMock<IOptimizationOverLimitByRestrictionDeciderCreator>();
			_optimizationPreferences = new OptimizationPreferences();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnFalseIfWrongCategory()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(false);
			
			
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker,_optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.False);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldSkipDayOffs()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(_shiftCatChecker.DayHasDayOff(part1)).Return(true);
			
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker,_optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.False);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldSkipIfDayTwoDayOff()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();
			var part2 = _mocks.DynamicMock<IScheduleDay>();
			
			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay2 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasDayOff(part2)).Return(true);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);

			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker,_optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.False);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldSkipIfNotSwappable()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();
			var part2 = _mocks.DynamicMock<IScheduleDay>();
			var scheduleDays = new List<IScheduleDay> { part1, part2 };

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay2 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);
			Expect.Call(_swappableChecker.PersonsAreSwappable(person1, person2, dateOnly, scheduleDays, true)).Return(false);

			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker,_optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.False);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnFalseIfDayTwoWrongCategory()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person();
			var person2 = new Person();

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();
			var part2 = _mocks.DynamicMock<IScheduleDay>();
		    var scheduleDays = new List<IScheduleDay> {part1, part2};

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay2 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);
            Expect.Call(_swappableChecker.PersonsAreSwappable(person1, person2, dateOnly, scheduleDays, true)).Return(true);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part2, cat2)).Return(false);
			
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker,
				_shiftCategoryFairnessContractToleranceChecker, _optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.False);
			_mocks.VerifyAll();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCallRescheduleIfMoreMembers()
		{
			var dateOnly = new DateOnly(2012, 10, 1);
			var cat1 = _mocks.DynamicMock<IShiftCategory>();
			var cat2 = _mocks.DynamicMock<IShiftCategory>();
			var person1 = new Person {Name = new Name("First", "")};
		    var person2 = new Person {Name = new Name("Second", "")};
		    var person3 = new Person {Name = new Name("Third", "")};

			var group1 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person1, person3 } };
			var group2 = new ShiftCategoryFairnessCompareResult { OriginalMembers = new List<IPerson> { person2 } };

			var suggestion = new ShiftCategoryFairnessSwap { Group1 = group1, Group2 = group2, ShiftCategoryFromGroup1 = cat1, ShiftCategoryFromGroup2 = cat2 };
			var matrix1 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.DynamicMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var rollbackService = _mocks.DynamicMock<ISchedulePartModifyAndRollbackService>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
			var part1 = _mocks.DynamicMock<IScheduleDay>();
            var part2 = _mocks.DynamicMock<IScheduleDay>();
            var scheduleDays = new List<IScheduleDay> { part1, part1 };
	        var decider = _mocks.DynamicMock<IOptimizationOverLimitByRestrictionDecider>();

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay1 }));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDay2 }));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);
			Expect.Call(_swappableChecker.PersonsAreSwappable(person2, person1, dateOnly, scheduleDays, true)).Return(true).IgnoreArguments();			
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part2, cat2)).Return(true);
			Expect.Call(rollbackService.ModifyParts(null)).Return(new BindingList<IBusinessRuleResponse>());
	        Expect.Call(_optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix1,
	                                                                                 _optimizationPreferences))
	              .Return(decider);
			Expect.Call(_optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix2,
																					 _optimizationPreferences))
				  .Return(decider);
	        Expect.Call(decider.OverLimit()).Return(new List<DateOnly>()).Repeat.AtLeastOnce();

			Expect.Call(_fairnessReScheduler.Execute(new List<IPerson>(),dateOnly,matrixes )).IgnoreArguments().Return(true);
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker, _optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.True);
            _mocks.VerifyAll();
			
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnTrueIfAllGoWell()
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
            var scheduleDays = new List<IScheduleDay> { part1, part2 };
			var decider = _mocks.DynamicMock<IOptimizationOverLimitByRestrictionDecider>();

			Expect.Call(matrix1.Person).Return(person1);
			Expect.Call(matrix2.Person).Return(person2);
			Expect.Call(matrix1.GetScheduleDayByKey(dateOnly)).Return(scheduleDay1);
			Expect.Call(matrix2.GetScheduleDayByKey(dateOnly)).Return(scheduleDay2);
			Expect.Call(matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay1}));
			Expect.Call(matrix2.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDay2}));
			Expect.Call(scheduleDay1.DaySchedulePart()).Return(part1);
			Expect.Call(scheduleDay2.DaySchedulePart()).Return(part2);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part1, cat1)).Return(true);
			Expect.Call(_swappableChecker.PersonsAreSwappable(person1, person2, dateOnly, scheduleDays, true)).Return(true);
			Expect.Call(_shiftCatChecker.DayHasShiftCategory(part2, cat2)).Return(true);
			Expect.Call(rollbackService.ModifyParts(null)).Return(new BindingList<IBusinessRuleResponse>());
			Expect.Call(_optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix1,
																					 _optimizationPreferences))
				  .Return(decider);
			Expect.Call(_optimizationOverLimitByRestrictionDeciderCreator.GetDecider(dateOnly, matrix2,
																					 _optimizationPreferences))
				  .Return(decider);
			Expect.Call(decider.OverLimit()).Return(new List<DateOnly>()).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessSwapper(_swapService, _resultState, _fairnessReScheduler, _shiftCatChecker, _deleteService, _swappableChecker, 
				_shiftCategoryFairnessContractToleranceChecker,_optimizationOverLimitByRestrictionDeciderCreator);
			Assert.That(_target.TrySwap(suggestion, dateOnly, matrixes, rollbackService, new BackgroundWorker(), true, _optimizationPreferences), Is.True);
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
			
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(part.PersonAssignmentCollection()).Return(
				new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {personAss}));
			Expect.Call(personAss.ShiftCategory).Return(cat);
			_mocks.ReplayAll();
			Assert.That(_target.DayHasShiftCategory(part, cat), Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnIfDayOff()
		{
			var part = _mocks.DynamicMock<IScheduleDay>();
			Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff);
			_mocks.ReplayAll();
			Assert.That(_target.DayHasDayOff(part), Is.True);
			_mocks.VerifyAll();
		}
	}
}