using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class NewSchedulePeriodTargetCalculatorFactoryTest
	{
		private NewSchedulePeriodTargetCalculatorFactory _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new NewSchedulePeriodTargetCalculatorFactory(_matrix);
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldReturnDynamicIfFixedStaffDayWorkTime()
		{
			_person.Period(new DateOnly()).PersonContract.Contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;
			using(_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(true);
			    Expect.Call(_schedulePeriod.Contract).Return(_person.Period(new DateOnly()).PersonContract.Contract);
				//Expect.Call(_schedulePeriod.PersonPeriod).Return(_person.Period(new DateOnly()));
			}

			using (_mocks.Playback())
			{
				ISchedulePeriodTargetCalculator calculator = _target.CreatePeriodTargetCalculator() as NewDynamicDayOffSchedulePeriodTargetCalculator;
				Assert.IsNotNull(calculator);
			}
		}

		[Test]
		public void ShouldReturnNullIfSchedulePeriodNotValid()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.IsValid).Return(false);
			}

			using (_mocks.Playback())
			{
				ISchedulePeriodTargetCalculator calculator = _target.CreatePeriodTargetCalculator();
				Assert.IsNull(calculator);
			}
		}
	}
}