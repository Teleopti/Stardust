﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class BudgetCalculatorTest
    {
        private BudgetCalculator _target;
        private IBudgetGroup _budgetGroup;
        private IScenario _scenario;
        private DateOnly _date;
        private IList<IBudgetDay> _listOfBudgetDays;
        private BudgetDay _budgetDay1;
        private BudgetDay _budgetDay2;
        private BudgetDay _budgetDay3;
        private BudgetDay _budgetDay4;
        private BudgetDay _budgetDay5;
        private BudgetDay _budgetDay6;
        private BudgetDay _budgetDay7;
        private BudgetDay _budgetDay8;
        private BudgetDay _budgetDay9;
        private BudgetDay _budgetDay10;
        private List<ICalculator> _calcList;

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _budgetGroup = new BudgetGroup { Name = "Group" };
            _budgetGroup.TrySetDaysPerYear(365);
            _budgetGroup.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.GetSystemTimeZones()[3]); 
            _date = new DateOnly(2010, 02, 01);
			_budgetDay1 = new BudgetDay(_budgetGroup, _scenario, _date) { AttritionRate = new Percent(0.1), StaffEmployed = 777, Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 171, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3};
			_budgetDay2 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(1)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay3 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(2)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay4 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(3)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3, AbsenceOverride = 10, AbsenceThreshold = new Percent(0.8)};
			_budgetDay5 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(4)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 151, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay6 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(5)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 120, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay7 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(6)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 152, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay8 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(7)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay9 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(8)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 162, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay10 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(9)) { IsClosed = true, AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 156, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            _listOfBudgetDays = new List<IBudgetDay> { _budgetDay1, _budgetDay2, _budgetDay3, _budgetDay4, _budgetDay5, _budgetDay6, _budgetDay7, _budgetDay8, _budgetDay9, _budgetDay10 };

            var netStaffCalculator = new NetStaffCalculator(new GrossStaffCalculator());
            var info = new CultureInfo(1053);

            _calcList = new List<ICalculator>
                            {
                                new DifferencePercentCalculator(netStaffCalculator, info),
                                new AllowanceCalculator(netStaffCalculator, info)
                            };
            _target = new BudgetCalculator(_listOfBudgetDays, netStaffCalculator, _calcList);
        }

        [Test]
        public void ShouldCalculate()
        {
            var result = _target.Calculate(_budgetDay4);
            result.GrossStaff.Should().Be.EqualTo(776.1488430210959d);
            result.NetStaff.Should().Be.EqualTo(556.53488787221136d);
            result.NetStaffFcAdj.Should().Be.EqualTo(596.07024229401054d);
            result.BudgetedStaff.Should().Be.EqualTo(596.72813703085262d);
            result.ForecastedStaff.Should().Be.EqualTo(22.105263157894736d);
            result.Difference.Should().Be.EqualTo(574.62287387295794d);
            result.DifferencePercent.Should().Be.EqualTo(new Percent(25.994844294252861d));
            result.BudgetedLeave.Should().Be.EqualTo(0);
            result.BudgetedSurplus.Should().Be.EqualTo(574.62287387295794d);
            result.TotalAllowance.Should().Be.EqualTo(10);
            result.Allowance.Should().Be.EqualTo(8);
        }

        [Test]
        public void ShouldCalculateEmptyResultIfDayIsClosed()
        {
            var result = _target.Calculate(_budgetDay10);
            result.GrossStaff.Should().Be.EqualTo(0);
            result.NetStaff.Should().Be.EqualTo(0);
            result.NetStaffFcAdj.Should().Be.EqualTo(0);
            result.BudgetedStaff.Should().Be.EqualTo(0);
            result.ForecastedStaff.Should().Be.EqualTo(0);
            result.Difference.Should().Be.EqualTo(0);
            result.DifferencePercent.Should().Be.EqualTo(new Percent(0));
            result.BudgetedLeave.Should().Be.EqualTo(0);
            result.BudgetedSurplus.Should().Be.EqualTo(0);
            result.TotalAllowance.Should().Be.EqualTo(0);
            result.Allowance.Should().Be.EqualTo(0);
        }
    }
}
