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
    public class NetStaffCalculatorTest
    {
        private NetStaffCalculator _target;
        private BudgetDay _budgetDay4;
        private List<IBudgetDay> listOfBudgetDays;

        [SetUp]
        public void Setup()
        {
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var budgetGroup = new BudgetGroup { Name = "Group" };
            budgetGroup.TrySetDaysPerYear(365);
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

            _target = new NetStaffCalculator(new GrossStaffCalculator());
        }

        [Test]
        public void ShouldGetNetStaff()
        {
            _target.Initialize(listOfBudgetDays);
            var result = _target.CalculatedResult(_budgetDay4);
            var gross = Math.Round(result.GrossStaff, 2);
            var net = Math.Round(result.NetStaff, 2);
            gross.Should().Be.EqualTo(776.15d);
            net.Should().Be.EqualTo(556.53d); 
        }
        
        [Test]
        public void ShouldGetNettStaffFor5DayWeek()
        {
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            var budgetGroup = new BudgetGroup { Name = "Group" };
            budgetGroup.TrySetDaysPerYear(365);
            var date = new DateOnly(2010, 10, 4);
            var budgetDay1 = new BudgetDay(budgetGroup, scenario, date) { StaffEmployed = 1,          ForecastedHours = 9, FulltimeEquivalentHours = 8};
            var budgetDay2 = new BudgetDay(budgetGroup, scenario, date.AddDays(1)) { Recruitment = 0, ForecastedHours = 7, FulltimeEquivalentHours = 8};
            var budgetDay3 = new BudgetDay(budgetGroup, scenario, date.AddDays(2)) { Recruitment = 0, ForecastedHours = 9, FulltimeEquivalentHours = 8};
            var budgetDay4 = new BudgetDay(budgetGroup, scenario, date.AddDays(3)) { Recruitment = 0, ForecastedHours = 7, FulltimeEquivalentHours = 8};
            var budgetDay5 = new BudgetDay(budgetGroup, scenario, date.AddDays(4)) { Recruitment = 0, ForecastedHours = 8, FulltimeEquivalentHours = 8};
            var budgetDay6 = new BudgetDay(budgetGroup, scenario, date.AddDays(5)) { Recruitment = 0, ForecastedHours = 0, FulltimeEquivalentHours = 8, IsClosed = true};
            var budgetDay7 = new BudgetDay(budgetGroup, scenario, date.AddDays(6)) { Recruitment = 0, ForecastedHours = 0, FulltimeEquivalentHours = 8, IsClosed = true};
            listOfBudgetDays = new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3, budgetDay4, budgetDay5, budgetDay6, budgetDay7 };

            var target = new NetStaffCalculator(new GrossStaffCalculator());
            target.Initialize(listOfBudgetDays);
            target.CalculatedResult(budgetDay1).NetStaff.Should().Be.EqualTo(1);
            target.CalculatedResult(budgetDay2).NetStaff.Should().Be.EqualTo(1);
            target.CalculatedResult(budgetDay3).NetStaff.Should().Be.EqualTo(1);
            target.CalculatedResult(budgetDay4).NetStaff.Should().Be.EqualTo(1);
            target.CalculatedResult(budgetDay5).NetStaff.Should().Be.EqualTo(1);
            target.CalculatedResult(budgetDay6).NetStaff.Should().Be.EqualTo(0);
            target.CalculatedResult(budgetDay7).NetStaff.Should().Be.EqualTo(0);
        }
    }
}
