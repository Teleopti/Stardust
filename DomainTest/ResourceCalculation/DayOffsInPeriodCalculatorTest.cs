using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class DayOffsInPeriodCalculatorTest
	{
		private MockRepository _mocks;
		private IScheduleDictionary _dictionary;
		private DayOffsInPeriodCalculator _target;
		private IScheduleRange _range;
		private IPerson _person;
		private IScheduleDay _scheduleDay;
		private DateOnly _date1;
		private DateOnly _date3;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
	    private ISchedulingResultStateHolder _stateHolder;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		    _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_dictionary = _mocks.StrictMock<IScheduleDictionary>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _target = new DayOffsInPeriodCalculator(_stateHolder);
			_range = _mocks.StrictMock<IScheduleRange>();
			_person = _mocks.StrictMock<IPerson>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_date1 = new DateOnly(2010,10,4);
			_date3 = new DateOnly(2010,10,6);
			_dateOnlyPeriod = new DateOnlyPeriod(_date1, _date3);
		}
		
		[Test]
		public void ShouldReturnZeroIfNoDayOffs()
		{
			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
		    Expect.Call(_stateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Times(3);
			_mocks.ReplayAll();
			int ret = _target.CountDayOffsOnPeriod(_virtualSchedulePeriod);
			Assert.That(ret, Is.EqualTo(0));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnThreeIfThreeDayOffs()
		{
			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
            Expect.Call(_stateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);

			_mocks.ReplayAll();
			int ret = _target.CountDayOffsOnPeriod(_virtualSchedulePeriod);
			Assert.That(ret, Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnTrueIfEmploymentTypeIsHourly()
		{
            //var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            //var personContract = _mocks.StrictMock<IPersonContract>();
            var contract = _mocks.StrictMock<IContract>();

		    Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			//Expect.Call(_virtualSchedulePeriod.PersonPeriod).Return(personPeriod);
			//Expect.Call(personPeriod.PersonContract).Return(personContract);
			//Expect.Call(personContract.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.HourlyStaff);
			_mocks.ReplayAll();
			int targetDaysOff;
			int current;
			var ret = _target.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnFalseWhenNotEnoughDayOffs()
		{
            //var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            //var personContract = _mocks.StrictMock<IPersonContract>();
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			//Expect.Call(_virtualSchedulePeriod.PersonPeriod).Return(personPeriod);
			//Expect.Call(personPeriod.PersonContract).Return(personContract);
			//Expect.Call(personContract.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(5);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
            Expect.Call(_stateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay>
			                                                                   	{_scheduleDay, _scheduleDay, _scheduleDay});

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);


			_mocks.ReplayAll();
			int targetDaysOff;
			int current;
			var ret = _target.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.False);
			Assert.That(targetDaysOff,Is.EqualTo(5));
			Assert.That(current,Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnFalseWhenTooManyDayOffs()
		{
            //var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            //var personContract = _mocks.StrictMock<IPersonContract>();
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			//Expect.Call(_virtualSchedulePeriod.PersonPeriod).Return(personPeriod);
			//Expect.Call(personPeriod.PersonContract).Return(personContract);
			//Expect.Call(personContract.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);
            Expect.Call(contract.PositiveDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
            Expect.Call(_stateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(4);

			_mocks.ReplayAll();
			int targetDaysOff;
			int current;
			var ret = _target.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.False);
			Assert.That(targetDaysOff, Is.EqualTo(2));
			Assert.That(current, Is.EqualTo(4));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnTrueWhenCorrectNumberOffDayOffs()
		{
            //var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            //var personContract = _mocks.StrictMock<IPersonContract>();
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			//Expect.Call(_virtualSchedulePeriod.PersonPeriod).Return(personPeriod);
			//Expect.Call(personPeriod.PersonContract).Return(personContract);
			//Expect.Call(personContract.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);
            Expect.Call(contract.PositiveDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
            Expect.Call(_stateHolder.Schedules).Return(_dictionary);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);

			_mocks.ReplayAll();
			int targetDaysOff;
			int current;
			var ret = _target.HasCorrectNumberOfDaysOff(_virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.True);
			Assert.That(targetDaysOff, Is.EqualTo(3));
			Assert.That(current, Is.EqualTo(3));
			_mocks.VerifyAll();
		}

        [Test]
        public void OutsideOrAtMinimumTargetDaysOffShouldReturnFalseIfEmploymentTypeIsHourly()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.HourlyStaff);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsFalse(ret);

        }

        [Test]
        public void OutsideOrAtMinimumTargetDaysOffShouldReturnTrueIfAtLimit()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.NegativeDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(2);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsTrue(ret);

        }

        [Test]
        public void OutsideOrAtMinimumTargetDaysOffShouldReturnTrueIfUnderLimit()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.NegativeDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay});

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(1);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsTrue(ret);

        }

        [Test]
        public void OutsideOrAtMinimumTargetDaysOffShouldReturnFalseIfInside()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.NegativeDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsFalse(ret);

        }

        [Test]
        public void OutsideOrAtMaximumTargetDaysOffShouldReturnFalseIfEmploymentTypeIsHourly()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.HourlyStaff);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsFalse(ret);

        }

        [Test]
        public void OutsideOrAtMaximumTargetDaysOffShouldReturnTrueIfAtLimit()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.PositiveDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(4);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsTrue(ret);

        }

        [Test]
        public void OutsideOrAtMaximumTargetDaysOffShouldReturnTrueIfOverLimit()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.PositiveDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(5);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsTrue(ret);

        }

        [Test]
        public void OutsideOrAtMaximumTargetDaysOffShouldReturnFalseIfInside()
        {

            var contract = _mocks.StrictMock<IContract>();

            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
                Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
                Expect.Call(contract.PositiveDayOffTolerance).Return(1);

                Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
                Expect.Call(_stateHolder.Schedules).Return(_dictionary);
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_virtualSchedulePeriod);
            }

            Assert.IsFalse(ret);

        }
	}

	

	
}