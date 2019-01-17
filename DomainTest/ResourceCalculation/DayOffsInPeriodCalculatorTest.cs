using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture, SetCulture("sv-SE")]
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

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dictionary = _mocks.StrictMock<IScheduleDictionary>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _target = new DayOffsInPeriodCalculator();
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
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Times(3);
			_mocks.ReplayAll();
			IList<IScheduleDay> ret = _target.CountDayOffsOnPeriod(_dictionary, _virtualSchedulePeriod);
			Assert.That(ret.Count, Is.EqualTo(0));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnThreeIfThreeDayOffs()
		{
			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);

			_mocks.ReplayAll();
			IList<IScheduleDay> ret = _target.CountDayOffsOnPeriod(_dictionary, _virtualSchedulePeriod);
			Assert.That(ret.Count, Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnTrueIfEmploymentTypeIsHourly()
		{
            var contract = _mocks.StrictMock<IContract>();

		    Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.HourlyStaff);
			_mocks.ReplayAll();
			int targetDaysOff;
			IList<IScheduleDay> current;
			var ret = _target.HasCorrectNumberOfDaysOff(_dictionary, _virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnFalseWhenNotEnoughDayOffs()
		{
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(5);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay>
			                                                                   	{_scheduleDay, _scheduleDay, _scheduleDay});

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);


			_mocks.ReplayAll();
			int targetDaysOff;
			IList<IScheduleDay> current;
			var ret = _target.HasCorrectNumberOfDaysOff(_dictionary, _virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.False);
			Assert.That(targetDaysOff,Is.EqualTo(5));
			Assert.That(current.Count,Is.EqualTo(3));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnFalseWhenTooManyDayOffs()
		{
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);
            Expect.Call(contract.PositiveDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(4);

			_mocks.ReplayAll();
			int targetDaysOff;
			IList<IScheduleDay> current;
			var ret = _target.HasCorrectNumberOfDaysOff(_dictionary, _virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.False);
			Assert.That(targetDaysOff, Is.EqualTo(2));
			Assert.That(current.Count, Is.EqualTo(4));
			_mocks.VerifyAll();
		}

		[Test]
		public void HasCorrectNumberOfDaysOffShouldReturnTrueWhenCorrectNumberOffDayOffs()
		{
			var contract = _mocks.StrictMock<IContract>();

            Expect.Call(_virtualSchedulePeriod.Contract).Return(contract);
			Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(3);
            Expect.Call(contract.NegativeDayOffTolerance).Return(1);
            Expect.Call(contract.PositiveDayOffTolerance).Return(1);

			Expect.Call(_virtualSchedulePeriod.Person).Return(_person);
			Expect.Call(_dictionary[_person]).Return(_range);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
			Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

			Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);

			_mocks.ReplayAll();
			int targetDaysOff;
			IList<IScheduleDay> current;
			var ret = _target.HasCorrectNumberOfDaysOff(_dictionary, _virtualSchedulePeriod, out targetDaysOff, out current);
			Assert.That(ret, Is.True);
			Assert.That(targetDaysOff, Is.EqualTo(3));
			Assert.That(current.Count, Is.EqualTo(3));
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
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(2);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay});

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(1);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMinimumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(4);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(5);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
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
                Expect.Call(_dictionary[_person]).Return(_range);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_range.ScheduledDayCollection(_dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay, _scheduleDay, _scheduleDay });

                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(3);
            }

            bool ret;

            using (_mocks.Playback())
            {
                ret = _target.OutsideOrAtMaximumTargetDaysOff(_dictionary, _virtualSchedulePeriod);
            }

            Assert.IsFalse(ret);

        }

		[Test]
		public void ShouldReturnSortedWeekPeriods()
		{
			var scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			var scheduleDayPro3 = _mocks.StrictMock<IScheduleDayPro>();
			var dateOnly1 = new DateOnly(2000, 1, 1);
			var dateOnly2 = new DateOnly(2000, 1, 2);
			var dateOnly3 = new DateOnly(2000, 2, 3);
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay3 = _mocks.StrictMock<IScheduleDay>();

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrixPro.UnlockedDays).Return(new HashSet<IScheduleDayPro> { scheduleDayPro1, scheduleDayPro2, scheduleDayPro3}).Repeat.AtLeastOnce();
				Expect.Call(scheduleMatrixPro.OuterWeeksPeriodDays).Return(new [] {scheduleDayPro1, scheduleDayPro2, scheduleDayPro3}).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro1.Day).Return(dateOnly1).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro2.Day).Return(dateOnly2).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro3.Day).Return(dateOnly3).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay1);
				Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay2);
				Expect.Call(scheduleDayPro3.DaySchedulePart()).Return(scheduleDay3);
				Expect.Call(scheduleDay1.SignificantPart()).Return(SchedulePartView.ContractDayOff);
				Expect.Call(scheduleDay2.SignificantPart()).Return(SchedulePartView.ContractDayOff);
				Expect.Call(scheduleDay3.SignificantPart()).Return(SchedulePartView.ContractDayOff);
			}

			using (_mocks.Playback())
			{
				var periods = _target.WeekPeriodsSortedOnDayOff(scheduleMatrixPro);
				Assert.AreEqual(2, periods.Count);
				Assert.AreEqual(1, periods[0].DaysOffCount);
				Assert.AreEqual(2, periods[1].DaysOffCount);

				var expectedPeriod1 = DateHelper.GetWeekPeriod(dateOnly3, CultureInfo.CurrentCulture);
				var expectedPeriod2 = DateHelper.GetWeekPeriod(dateOnly1, CultureInfo.CurrentCulture);

				Assert.AreEqual(expectedPeriod1, periods[0].Period);
				Assert.AreEqual(expectedPeriod2, periods[1].Period);
			}
		}
	}	
}