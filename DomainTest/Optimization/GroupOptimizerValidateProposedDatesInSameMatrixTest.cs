using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupOptimizerValidateProposedDatesInSameMatrixTest
	{
		private IGroupOptimizerValidateProposedDatesInSameMatrix _target;
		private MockRepository _mock;
		private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;

		private IPerson _person1;
		private DateOnly _date1;

		private IScheduleMatrixPro _matrix1;
		private IList<IScheduleMatrixPro> _matrixes;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_groupOptimizerFindMatrixesForGroup = _mock.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
			_target = new GroupOptimizerValidateProposedDatesInSameMatrix(_groupOptimizerFindMatrixesForGroup);
			_person1 = PersonFactory.CreatePerson();
			_date1 = new DateOnly(2012, 1, 1);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixes = new List<IScheduleMatrixPro> { _matrix1 };
			_schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
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
				result = _target.Validate(_person1, new List<DateOnly> {_date1}, false);
			}

			Assert.IsTrue(result.Success);
			Assert.IsFalse(result.DaysToLock.HasValue);
		}

		[Test]
		public void ShouldSuccessAndLockIfDateNotFoundInMatrix()
		{
			using (_mock.Record())
			{
				Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person1, _date1)).Return(_matrixes);
				Expect.Call(_matrix1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(), new DateOnly()));
			}

			ValidatorResult result;
			using (_mock.Playback())
			{
				result = _target.Validate(_person1, new List<DateOnly> { _date1 }, true);
			}

			Assert.IsTrue(result.Success);
			Assert.IsTrue(result.DaysToLock.Value.Contains(_date1));
		}

	}
}