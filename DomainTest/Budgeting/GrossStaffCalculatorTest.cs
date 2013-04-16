﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class GrossStaffCalculatorTest
    {
        private GrossStaffCalculator _target;
        private BudgetDay _budgetDay5;
        private List<IBudgetDay> listOfBudgetDays;

        [SetUp]
        public void Setup()
        {
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var budgetGroup = new BudgetGroup { Name = "Group" };
            budgetGroup.TrySetDaysPerYear(365);
            var date = new DateOnly(2010, 10, 4);
            var budgetDay1 = new BudgetDay(budgetGroup, scenario, date) { AttritionRate = new Percent(0.10), StaffEmployed = 777, Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 171, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3};
            var budgetDay2 = new BudgetDay(budgetGroup, scenario, date.AddDays(1)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay3 = new BudgetDay(budgetGroup, scenario, date.AddDays(2)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay4 = new BudgetDay(budgetGroup, scenario, date.AddDays(3)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            _budgetDay5 = new BudgetDay(budgetGroup, scenario, date.AddDays(4)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 151, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay6 = new BudgetDay(budgetGroup, scenario, date.AddDays(5)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 120, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay7 = new BudgetDay(budgetGroup, scenario, date.AddDays(6)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 152, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay8 = new BudgetDay(budgetGroup, scenario, date.AddDays(7)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay9 = new BudgetDay(budgetGroup, scenario, date.AddDays(8)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 162, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            var budgetDay10 = new BudgetDay(budgetGroup, scenario, date.AddDays(9)) { AttritionRate = new Percent(0.10), Recruitment = 0, Contractors = 3, DaysOffPerWeek = 2, ForecastedHours = 156, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            listOfBudgetDays = new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3, budgetDay4, _budgetDay5, budgetDay6, budgetDay7, budgetDay8, budgetDay9, budgetDay10 };
            
            _target = new GrossStaffCalculator();
        }

        [Test]
        public void ShouldGetGrossStaff()
        {
            _target.Initialize(listOfBudgetDays); 

            var result = _target.CalculatedResult(_budgetDay5);
            var gross = Math.Round(result.GrossStaff, 2);
			gross.Should().Be.EqualTo(775.94d); 
        }

		[Test]
		public void ShouldNotCrashWhenNoStaffEmployed()
        {
            listOfBudgetDays[0].StaffEmployed = null;
            _target.Initialize(listOfBudgetDays);
			var result = _target.CalculatedResult(_budgetDay5);
			result.GrossStaff.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnWhenEmptyList()
		{
			_target.Initialize(new List<IBudgetDay>());
		}
    }
}
