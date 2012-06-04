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
        private IScheduleMatrixPro _matrix;
        private IVirtualSchedulePeriod _schedulePeriod;
		private IContract _contract;

        [SetUp]
        public void Setup()
        {
            _targetTime = new SchedulePeriodTargetTimeTimeCalculator();
        	_matrix = MockRepository.GenerateMock<IScheduleMatrixPro>();
        	_schedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			_contract = MockRepository.GenerateMock<IContract>();
        }

        [Test]
        public void VerifyHourlyStaff()
        {
            //two weeks affected, 8h minContractTime, week max 40h
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 7);
            IPerson person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("en-GB"));

			CommonMocks(0.5);
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.HourlyStaff).Repeat.Once();
			_schedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.Once();

			_contract.Stub(x => x.WorkTimeDirective).Return(new WorkTimeDirective(TimeSpan.FromHours(40),
																				  TimeSpan.Zero, TimeSpan.Zero)).Repeat.Once();
			_schedulePeriod.Stub(x => x.MinTimeSchedulePeriod).Return(TimeSpan.FromHours(8)).Repeat.AtLeastOnce();
			_matrix.Stub(x => x.Person).Return(person).Repeat.Once();

        	var result = _targetTime.TargetWithTolerance(_matrix);

			Assert.AreEqual(TimeSpan.FromHours(8), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(80), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffDayWorkTime()
        {
            //8h average wt and flex 1h
        	var dayDo = MockRepository.GenerateMock<IScheduleDayPro>();
			var dayMain = MockRepository.GenerateMock<IScheduleDayPro>();
			var partDo = MockRepository.GenerateMock<IScheduleDay>();
			var partMain = MockRepository.GenerateMock<IScheduleDay>();
            var periodDays = new List<IScheduleDayPro> { dayDo, dayDo, dayMain, dayDo };
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 4);
            
			CommonMocks(0.5);
			_schedulePeriod.Stub(x => x.Extra).Return(TimeSpan.FromHours(2));
			_schedulePeriod.Stub(x => x.BalanceIn).Return(TimeSpan.FromHours(3));
			_schedulePeriod.Stub(x => x.BalanceOut).Return(TimeSpan.FromHours(4));
            _contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime).Repeat.Twice();
            _matrix.Stub(x => x.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays)).Repeat.Once();
            dayDo.Stub(x => x.DaySchedulePart()).Return(partDo).Repeat.Any();
            dayMain.Stub(x => x.DaySchedulePart()).Return(partMain).Repeat.Any();
            partDo.Stub(x => x.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            partMain.Stub(x => x.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            _schedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.Any();
            _schedulePeriod.Stub(x => x.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8)).Repeat.Any();

        	var result = _targetTime.TargetWithTolerance(_matrix);
            
            Assert.AreEqual(TimeSpan.FromHours(14), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(15.5), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffNormal()
        {
			CommonMocks(0.5);
			_schedulePeriod.Stub(x => x.Extra).Return(TimeSpan.FromHours(2));
			_schedulePeriod.Stub(x => x.BalanceIn).Return(TimeSpan.FromHours(3));
			_schedulePeriod.Stub(x => x.BalanceOut).Return(TimeSpan.FromHours(4));
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.Twice();
			_schedulePeriod.Stub(x => x.PeriodTarget()).Return(TimeSpan.FromHours(16)).Repeat.Once();

        	var result = _targetTime.TargetWithTolerance(_matrix);

            Assert.AreEqual(TimeSpan.FromHours(26), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(27.5), result.EndTime);
        }

        [Test]
        public void VerifyFixedStaffNormalWithNegativeSeasonality()
        {
			CommonMocks(-0.5);
			_schedulePeriod.Stub(x => x.Extra).Return(TimeSpan.FromHours(2));
			_schedulePeriod.Stub(x => x.BalanceIn).Return(TimeSpan.FromHours(3));
			_schedulePeriod.Stub(x => x.BalanceOut).Return(TimeSpan.FromHours(4));
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime).Repeat.Twice();
			_schedulePeriod.Stub(x => x.PeriodTarget()).Return(TimeSpan.FromHours(16)).Repeat.Once();

        	var result = _targetTime.TargetWithTolerance(_matrix);

            Assert.AreEqual(TimeSpan.FromHours(10), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(11.5), result.EndTime);
        }

        private void CommonMocks(double seasonality)
        {
            _matrix.Stub(x => x.SchedulePeriod).Return(_schedulePeriod).Repeat.Any();
            _schedulePeriod.Stub(x => x.Contract).Return(_contract).Repeat.Any();
            _contract.Stub(x => x.NegativePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(1)).Repeat.Any();
            _contract.Stub(x => x.PositivePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(0.5)).Repeat.Any();
            _schedulePeriod.Stub(x => x.Seasonality).Return(new Percent(seasonality)).Repeat.Any();
        }
    }
}
