using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
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
			_schedulePeriod.Stub(x => x.Person).Return(person);

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

		[Test]
		public void ShouldNotIncludePreferenceDayOffInTargetTimeWhenEmploymentTypeIsFixedStaffDayWorkTime()
		{
			var contract = ContractFactory.CreateContract(" ");
			contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;

			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)));
			virtualSchedulePeriod.Stub(x => x.Contract).Return(contract);
			virtualSchedulePeriod.Stub(x => x.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));

			var dayOffPreferenceRestriction = new PreferenceRestriction {DayOffTemplate = new DayOffTemplate(new Description())};
			var scheduleDayWithDayOffPreference = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDayWithDayOffPreference.Stub(x => x.RestrictionCollection()).Return(new[] {dayOffPreferenceRestriction});

			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today.AddDays(1));

			var scheduleDays = new[] { scheduleDayWithDayOffPreference, scheduleDay };

			var result = target.TargetTime(virtualSchedulePeriod, scheduleDays);

			result.Should().Be(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldIncludePreferenceAbsenceInTargetTimeWhenEmploymentTypeIsFixedStaffDayWorkTime()
		{
			var contract = ContractFactory.CreateContract(" ");
			contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;

			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)));
			virtualSchedulePeriod.Stub(x => x.Contract).Return(contract);
			virtualSchedulePeriod.Stub(x => x.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));

			var absencePreferenceRestriction = new PreferenceRestriction { Absence = new Absence() };
			var scheduleDayWithAbsencePreference = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDayWithAbsencePreference.Stub(x => x.RestrictionCollection()).Return(new[] { absencePreferenceRestriction });

			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today.AddDays(1));

			var scheduleDays = new[] { scheduleDayWithAbsencePreference, scheduleDay };

			var result = target.TargetTime(virtualSchedulePeriod, scheduleDays);

			result.Should().Be(TimeSpan.FromHours(16));
		}

		[Test]
		public void ShouldNotExcludeDayOffTwice()
		{
			var dayOffTemplate = new DayOffTemplate(new Description());

			var contract = ContractFactory.CreateContract(" ");
			contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;

			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)));
			virtualSchedulePeriod.Stub(x => x.Contract).Return(contract);
			virtualSchedulePeriod.Stub(x => x.AverageWorkTimePerDay).Return(TimeSpan.FromHours(8));

			var dayOffPreferenceRestriction = new PreferenceRestriction { DayOffTemplate = dayOffTemplate };
			var scheduleDayWithDayOffPreference = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, new PersonDayOff(new Person(), new Scenario(" "), dayOffTemplate, DateOnly.Today.AddDays(1)));
			scheduleDayWithDayOffPreference.Stub(x => x.RestrictionCollection()).Return(new[] { dayOffPreferenceRestriction });

			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today.AddDays(1));

			var scheduleDays = new[] { scheduleDayWithDayOffPreference, scheduleDay };

			var result = target.TargetTime(virtualSchedulePeriod, scheduleDays);

			result.Should().Be(TimeSpan.FromHours(8));
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
