﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture, SetCulture("sv-SE")]
    public class BudgetedStaffCalculatorTest
    {
        private BudgetedStaffCalculator _target;
        private BudgetDay _budgetDay5;
        private List<IBudgetDay> listOfBudgetDays;
	    private NetStaffCalculator netStaffCalculator;

        [SetUp]
        public void Setup()
        {
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var budgetGroup = new BudgetGroup { Name = "Group" };
            budgetGroup.TrySetDaysPerYear(365);
            budgetGroup.TimeZone = (TimeZoneInfo.GetSystemTimeZones()[3]); 
            var date = new DateOnly(2010, 10, 4);
			var budgetDay1 = new BudgetDay(budgetGroup, scenario, date) { AttritionRate = new Percent(0.1), StaffEmployed = 777, Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 171, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay2 = new BudgetDay(budgetGroup, scenario, date.AddDays(1)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay3 = new BudgetDay(budgetGroup, scenario, date.AddDays(2)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay4 = new BudgetDay(budgetGroup, scenario, date.AddDays(3)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay5 = new BudgetDay(budgetGroup, scenario, date.AddDays(4)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 151, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay6 = new BudgetDay(budgetGroup, scenario, date.AddDays(5)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 120, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay7 = new BudgetDay(budgetGroup, scenario, date.AddDays(6)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 152, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay8 = new BudgetDay(budgetGroup, scenario, date.AddDays(7)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay9 = new BudgetDay(budgetGroup, scenario, date.AddDays(8)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 162, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay10 = new BudgetDay(budgetGroup, scenario, date.AddDays(9)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 156, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            listOfBudgetDays = new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3, budgetDay4, _budgetDay5, budgetDay6, budgetDay7, budgetDay8, budgetDay9, budgetDay10 };

            netStaffCalculator = new NetStaffCalculator(new GrossStaffCalculator());
            netStaffCalculator.Initialize(listOfBudgetDays);
            _target = new BudgetedStaffCalculator();
        }

        [Test]
        public void ShouldGetBudgetedStaff()
        {
            var grossStaffCalculator = new GrossStaffCalculator();
            grossStaffCalculator.Initialize(listOfBudgetDays);
            var result = grossStaffCalculator.CalculatedResult(_budgetDay5);
	        var netStaffFcAdjCalc = new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator);
			netStaffFcAdjCalc.Calculate(_budgetDay5, listOfBudgetDays, ref result);
            _target.Calculate(_budgetDay5, listOfBudgetDays, ref result);
            var budget = Math.Round(result.BudgetedStaff, 2);
            budget.Should().Be.EqualTo(536.41d);
        }

        [Test]
        public void ShouldGetBudgetedStaffWhenFulltimeEquivalentHoursIsZero()
        {
            _budgetDay5.FulltimeEquivalentHours = 0;
            var grossStaffCalculator = new GrossStaffCalculator();
            grossStaffCalculator.Initialize(listOfBudgetDays);
            var result = grossStaffCalculator.CalculatedResult(_budgetDay5);
            _target.Calculate(_budgetDay5, listOfBudgetDays, ref result);
            var budget = Math.Round(result.BudgetedStaff, 2);
            budget.Should().Be.EqualTo(0d);
        }
    }
}
