using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
    public class SchedulePeriodTargetTimeCalculatorTest
    {
        private ISchedulePeriodTargetTimeCalculator _targetTime;
        private MockRepository _mocks;
        private IScheduleMatrixPro _matrix;
        private IVirtualSchedulePeriod _schedulePeriod;
        //private IPersonPeriod _personPeriod;
        //private IPersonContract _personContract;
        private IContract _contract;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _targetTime = new SchedulePeriodTargetTimeTimeCalculator();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            //_personPeriod = _mocks.StrictMock<IPersonPeriod>();
            //_personContract = _mocks.StrictMock<IPersonContract>();
            _contract = _mocks.StrictMock<IContract>();
        }

        [Test]
        public void VerifyHourlyStaff()
        {
            //two weeks affected, 8h minContractTime, week max 40h
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 7);
            IPerson person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("en-GB"));
            using (_mocks.Record())
            {
                commonMocks(0.5);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff).Repeat.Once();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.Once();
                Expect.Call(_contract.WorkTimeDirective).Return(new WorkTimeDirective(TimeSpan.FromHours(40),
                                                                                      TimeSpan.Zero, TimeSpan.Zero)).
                    Repeat.Once();
                Expect.Call(_schedulePeriod.MinTimeSchedulePeriod).Return(TimeSpan.FromHours(8)).Repeat.AtLeastOnce();
                Expect.Call(_matrix.Person).Return(person).Repeat.Once();
            }
            TimePeriod result;
            using (_mocks.Playback())
            {
                result = _targetTime.TargetWithTolerance(_matrix);
            }
            Assert.AreEqual(TimeSpan.FromHours(8), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(80), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffDayWorkTime()
        {
            //8h average wt and flex 1h
            var dayDo = _mocks.StrictMock<IScheduleDayPro>();
            var dayMain = _mocks.StrictMock<IScheduleDayPro>();
            var partDo = _mocks.StrictMock<IScheduleDay>();
            var partMain = _mocks.StrictMock<IScheduleDay>();
            var periodDays = new List<IScheduleDayPro> { dayDo, dayDo, dayMain, dayDo };
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 4);
            
            using (_mocks.Record())
            {
                commonMocks(0.5);
				Expect.Call(_schedulePeriod.Extra).Return(TimeSpan.FromHours(2));
				Expect.Call(_schedulePeriod.BalanceIn).Return(TimeSpan.FromHours(3));
				Expect.Call(_schedulePeriod.BalanceOut).Return(TimeSpan.FromHours(4));
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime).Repeat.Twice();
                Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays)).Repeat.Once();
                Expect.Call(dayDo.DaySchedulePart()).Return(partDo).Repeat.Any();
                Expect.Call(dayMain.DaySchedulePart()).Return(partMain).Repeat.Any();
                Expect.Call(partDo.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
                Expect.Call(partMain.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.Any();
                Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8)).Repeat.Any();
                

            }
            TimePeriod result;
            using (_mocks.Playback())
            {
                result = _targetTime.TargetWithTolerance(_matrix);
            }
            Assert.AreEqual(TimeSpan.FromHours(14), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(15.5), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffNormal()
        {
            using (_mocks.Record())
            {
                commonMocks(0.5);
				Expect.Call(_schedulePeriod.Extra).Return(TimeSpan.FromHours(2));
				Expect.Call(_schedulePeriod.BalanceIn).Return(TimeSpan.FromHours(3));
				Expect.Call(_schedulePeriod.BalanceOut).Return(TimeSpan.FromHours(4));
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.Twice();
                Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(16)).Repeat.Once();
            }
            TimePeriod result;
            using (_mocks.Playback())
            {
                result = _targetTime.TargetWithTolerance(_matrix);
            }
            Assert.AreEqual(TimeSpan.FromHours(26), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(27.5), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffNormalWithNegativeSeasonality()
        {
            using (_mocks.Record())
            {
                commonMocks(-0.5);
                Expect.Call(_schedulePeriod.Extra).Return(TimeSpan.FromHours(2));
                Expect.Call(_schedulePeriod.BalanceIn).Return(TimeSpan.FromHours(3));
                Expect.Call(_schedulePeriod.BalanceOut).Return(TimeSpan.FromHours(4));
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.Twice();
                Expect.Call(_schedulePeriod.PeriodTarget()).Return(TimeSpan.FromHours(16)).Repeat.Once();
            }
            TimePeriod result;
            using (_mocks.Playback())
            {
                result = _targetTime.TargetWithTolerance(_matrix);
            }
            Assert.AreEqual(TimeSpan.FromHours(10), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(11.5), result.EndTime);
        }

        private void commonMocks(double seasonality)
        {
            Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            Expect.Call(_schedulePeriod.Contract).Return(_contract).Repeat.Any();
            //Expect.Call(_schedulePeriod.PersonPeriod).Return(_personPeriod).Repeat.Any();
            //Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Any();
            //Expect.Call(_personContract.Contract).Return(_contract).Repeat.Any();
            Expect.Call(_contract.NegativePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(1)).Repeat.Any();
            Expect.Call(_contract.PositivePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(0.5)).Repeat.Any();
            Expect.Call(_schedulePeriod.Seasonality).Return(new Percent(seasonality)).Repeat.Any();

        }
    }
}
