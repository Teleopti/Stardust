using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class DifferencePercentCalculatorTest
    {
        private DifferencePercentCalculator _target;
        private BudgetDay _budgetDay4;
        private List<IBudgetDay> listOfBudgetDays;

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
			_budgetDay4 = new BudgetDay(budgetGroup, scenario, date.AddDays(3)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay5 = new BudgetDay(budgetGroup, scenario, date.AddDays(4)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 151, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay6 = new BudgetDay(budgetGroup, scenario, date.AddDays(5)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 120, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay7 = new BudgetDay(budgetGroup, scenario, date.AddDays(6)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 152, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay8 = new BudgetDay(budgetGroup, scenario, date.AddDays(7)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay9 = new BudgetDay(budgetGroup, scenario, date.AddDays(8)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 162, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			var budgetDay10 = new BudgetDay(budgetGroup, scenario, date.AddDays(9)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 156, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
            listOfBudgetDays = new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3, _budgetDay4, budgetDay5, budgetDay6, budgetDay7, budgetDay8, budgetDay9, budgetDay10 };

            var netStaffCalculator = new NetStaffCalculator(new GrossStaffCalculator());
            netStaffCalculator.Initialize(listOfBudgetDays);
            _target = new DifferencePercentCalculator();
        }

        [Test]
        public void ShouldGetBudgetedStaff()
        {
            var result = new BudgetCalculationResult();
	        result.ForecastedStaff = 4;
	        result.Difference = 100;
            _target.Calculate(_budgetDay4, listOfBudgetDays, ref result);
            var differencePercent = Math.Round(result.DifferencePercent.Value, 4);
            differencePercent.Should().Be.EqualTo(25d);
        }
    }
}
