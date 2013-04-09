using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupOptimizationValidatorRunnerTest
	{
		private IGroupOptimizationValidatorRunner _target;
		private MockRepository _mocks;
		private IGroupDayOffOptimizerValidateDayOffToRemove _groupDayOffOptimizerValidateDayOffToRemove;
		private IGroupDayOffOptimizerValidateDayOffToAdd _groupDayOffOptimizerValidateDayOffToAdd;
		private IGroupOptimizerValidateProposedDatesInSameMatrix _groupOptimizerValidateProposedDatesInSameMatrix;
		private IGroupOptimizerValidateProposedDatesInSameGroup _groupOptimizerValidateProposedDatesInSameGroup;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupDayOffOptimizerValidateDayOffToRemove =  _mocks.StrictMock<IGroupDayOffOptimizerValidateDayOffToRemove>();
			_groupDayOffOptimizerValidateDayOffToAdd =  _mocks.StrictMock<IGroupDayOffOptimizerValidateDayOffToAdd>();
			_groupOptimizerValidateProposedDatesInSameMatrix =  _mocks.StrictMock<IGroupOptimizerValidateProposedDatesInSameMatrix>();
			_groupOptimizerValidateProposedDatesInSameGroup =  _mocks.StrictMock<IGroupOptimizerValidateProposedDatesInSameGroup>();
			_target = new GroupOptimizationValidatorRunner(_groupDayOffOptimizerValidateDayOffToRemove,
			                                               _groupDayOffOptimizerValidateDayOffToAdd,
			                                               _groupOptimizerValidateProposedDatesInSameMatrix,
			                                               _groupOptimizerValidateProposedDatesInSameGroup);
		}

		[Test]
		public void ShouldRunValidation()
		{
			var person = PersonFactory.CreatePerson("bill");
			var dateOnly = new DateOnly(2013, 4, 8);
			var daysMoveTimeFrom = new List<DateOnly> {dateOnly};
			var daysMoveTimeTo = new List<DateOnly> {dateOnly.AddDays(1)};
			var allDays = new List<DateOnly> {dateOnly, dateOnly.AddDays(1)};
			var matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var validationResult = new ValidatorResult {Success = true, MatrixList = new List<IScheduleMatrixPro> {matrix}, DaysToLock = dateOnlyPeriod};
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			using (_mocks.Record())
			{
				Expect.Call(matrix.SchedulePeriod).Return(schedulePeriod).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(() => matrix.LockPeriod(dateOnlyPeriod)).Repeat.AtLeastOnce();
				Expect.Call(_groupDayOffOptimizerValidateDayOffToRemove.Validate(person, daysMoveTimeFrom, false)).Return(validationResult);
				Expect.Call(_groupDayOffOptimizerValidateDayOffToAdd.Validate(person, daysMoveTimeTo, false)).Return(validationResult);
				Expect.Call(_groupOptimizerValidateProposedDatesInSameMatrix.Validate(person, allDays, false)).Return(validationResult);
				Expect.Call(_groupOptimizerValidateProposedDatesInSameGroup.Validate(person, allDays, false)).Return(validationResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Run(person, daysMoveTimeFrom, daysMoveTimeTo, false);
				Assert.IsTrue(result.Success);
			}
		}
	
		[Test]
		public void ShouldReturnFalseIfValidationFailed()
		{
			var person = PersonFactory.CreatePerson("bill");
			var dateOnly = new DateOnly(2013, 4, 8);
			var daysMoveTimeFrom = new List<DateOnly> {dateOnly};
			var daysMoveTimeTo = new List<DateOnly> {dateOnly.AddDays(1)};
			var allDays = new List<DateOnly> {dateOnly, dateOnly.AddDays(1)};
			var validationResult = new ValidatorResult();
			using (_mocks.Record())
			{
				Expect.Call(_groupDayOffOptimizerValidateDayOffToRemove.Validate(person, daysMoveTimeFrom, false)).Return(validationResult);
				Expect.Call(_groupDayOffOptimizerValidateDayOffToAdd.Validate(person, daysMoveTimeTo, false)).Return(validationResult);
				Expect.Call(_groupOptimizerValidateProposedDatesInSameMatrix.Validate(person, allDays, false)).Return(validationResult);
				Expect.Call(_groupOptimizerValidateProposedDatesInSameGroup.Validate(person, allDays, false)).Return(validationResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Run(person, daysMoveTimeFrom, daysMoveTimeTo, false);
				Assert.IsFalse(result.Success);
			}
		}
	}
}
