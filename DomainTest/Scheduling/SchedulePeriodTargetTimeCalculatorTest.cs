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
        private SchedulePeriodTargetTimeCalculator target;
        private IScheduleMatrixPro _matrix;
        private IVirtualSchedulePeriod _schedulePeriod;
		private IContract _contract;

        [SetUp]
        public void Setup()
        {
            target = new SchedulePeriodTargetTimeCalculator();
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
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.HourlyStaff);
			_schedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);

			_contract.Stub(x => x.WorkTimeDirective)
				.Return(new WorkTimeDirective(TimeSpan.FromHours(40), TimeSpan.Zero, TimeSpan.Zero));
			_schedulePeriod.Stub(x => x.MinTimeSchedulePeriod).Return(TimeSpan.FromHours(8));
			_matrix.Stub(x => x.Person).Return(person);

        	var result = target.TargetWithTolerance(_matrix);

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
            _contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
            _matrix.Stub(x => x.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodDays));
            dayDo.Stub(x => x.DaySchedulePart()).Return(partDo);
            dayMain.Stub(x => x.DaySchedulePart()).Return(partMain);
            partDo.Stub(x => x.SignificantPart()).Return(SchedulePartView.DayOff);
            partMain.Stub(x => x.SignificantPart()).Return(SchedulePartView.MainShift);
            _schedulePeriod.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
            _schedulePeriod.Stub(x => x.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));

        	var result = target.TargetWithTolerance(_matrix);
            
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
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			_schedulePeriod.Stub(x => x.PeriodTarget()).Return(TimeSpan.FromHours(16));

        	var result = target.TargetWithTolerance(_matrix);

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
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
			_schedulePeriod.Stub(x => x.PeriodTarget()).Return(TimeSpan.FromHours(16));

        	var result = target.TargetWithTolerance(_matrix);

            Assert.AreEqual(TimeSpan.FromHours(10), result.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(11.5), result.EndTime);
        }

        private void CommonMocks(double seasonality)
        {
            _matrix.Stub(x => x.SchedulePeriod).Return(_schedulePeriod);
            _schedulePeriod.Stub(x => x.Contract).Return(_contract);
            _contract.Stub(x => x.NegativePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(1));
            _contract.Stub(x => x.PositivePeriodWorkTimeTolerance).Return(TimeSpan.FromHours(0.5));
            _schedulePeriod.Stub(x => x.Seasonality).Return(new Percent(seasonality));
        }
    }
}
