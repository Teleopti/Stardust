using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupDayOffOptimizerValidateDayOffToRemoveTest
	{
		private IGroupDayOffOptimizerValidateDayOffToRemove _target;
		private MockRepository _mock;
		private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
		private IPerson _person1;
		private DateOnly _date1;
		private DateOnly _lockedDate;
		private IScheduleMatrixPro _matrix1;
		private IList<IScheduleMatrixPro> _matrixes;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayLocked;
		private IScheduleDay _scheduleDay1;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_groupOptimizerFindMatrixesForGroup = _mock.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
			_target = new GroupDayOffOptimizerValidateDayOffToRemove(_groupOptimizerFindMatrixesForGroup);

			_person1 = PersonFactory.CreatePerson();
			_date1 = new DateOnly(2012, 1, 1);
			_lockedDate = new DateOnly();
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayLocked = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();

			_matrixes = new List<IScheduleMatrixPro> { _matrix1 };
		}

		[Test]
		public void ShouldBeSuccessfulIfNotUsingSameDaysOff()
		{
			using (_mock.Record())
			{

			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _date1 }, false);
			}

			Assert.IsTrue(result.Success);
		}

		[Test]
		public void ShouldSuccessAndLockIfDateIsLockedForOneMember()
		{

			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_matrix1.GetScheduleDayByKey(_lockedDate)).Return(_scheduleDayLocked).Repeat.Any();
				Expect.Call(_matrix1.UnlockedDays).Return(
				new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 })).Repeat.Any();
			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _lockedDate }, true);
			}

			Assert.IsTrue(result.Success);
			Assert.IsTrue(result.DaysToLock.Value.Contains(_lockedDate));
		}

		[Test]
		public void ShouldSuccessAndLockIfNotDayOff()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_matrix1.UnlockedDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 })).Repeat.Any();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _date1 }, true);
			}

			Assert.IsTrue(result.Success);
			Assert.IsTrue(result.DaysToLock.Value.Contains(_date1));
		}

		[Test]
		public void ShouldSuccessAndNotLockIfNoFailure()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_matrix1.UnlockedDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 })).Repeat.Any();
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _date1 }, true);
			}

			Assert.IsTrue(result.Success);
			Assert.IsFalse(result.DaysToLock.HasValue);
		}

		[Test]
		public void ShouldSuccessIfNotUseSameDaysOff()
		{
			using (_mock.Record())
			{

			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _date1 }, false);
			}

			Assert.IsTrue(result.Success);
			Assert.IsFalse(result.DaysToLock.HasValue);
		}


		private void commonMocks()
		{
			Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person1, _date1)).Return(_matrixes).Repeat.Any();
			Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person1, _lockedDate)).Return(_matrixes).Repeat.Any();
			Expect.Call(_matrix1.GetScheduleDayByKey(_date1)).Return(_scheduleDayPro1).Repeat.Any();
			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1).Repeat.Any();


		}
	}
}