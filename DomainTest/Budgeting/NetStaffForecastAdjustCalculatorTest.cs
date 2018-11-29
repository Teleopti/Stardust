using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture,SetCulture("sv-SE")]
    public class NetStaffForecastAdjustCalculatorTest
    {
        private NetStaffForecastAdjustCalculator _target;
	    private BudgetDay _budgetDay1, _budgetDay2, _budgetDay3, _budgetDay4, _budgetDay5, _budgetDay6, _budgetDay7;
        private List<IBudgetDay> _listOfBudgetDays;
	    private BudgetGroup _budgetGroup;
	    private IScenario _scenario;
	    private DateOnly _date;
	    private BudgetCalculationResult _result;

	    [SetUp]
	    public void Setup()
	    {
		    _scenario = ScenarioFactory.CreateScenarioAggregate();
		    _budgetGroup = new BudgetGroup {Name = "Group"};
		    _budgetGroup.TrySetDaysPerYear(365);
		    _budgetGroup.TimeZone = (TimeZoneInfo.GetSystemTimeZones()[3]);
		    _date = new DateOnly(2010, 10, 4);
		    _budgetDay1 = new BudgetDay(_budgetGroup, _scenario, _date)
			    {
				    AttritionRate = new Percent(0.1),
				    StaffEmployed = 777,
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 171,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay2 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(1))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 168,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay3 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(2))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 168,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay4 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(3))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 168,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay5 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(4))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 151,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay6 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(5))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 120,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _budgetDay7 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(6))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 152,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    var budgetDay8 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(7))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 168,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    var budgetDay9 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(8))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 162,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    var budgetDay10 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(9))
			    {
				    AttritionRate = new Percent(0.1),
				    Recruitment = 0,
				    Contractors = 22.8,
				    DaysOffPerWeek = 2,
				    ForecastedHours = 156,
				    FulltimeEquivalentHours = 7.6,
				    OvertimeHours = 2,
				    StudentHours = 3
			    };
		    _listOfBudgetDays = new List<IBudgetDay>
			    {
				    _budgetDay1,
				    _budgetDay2,
				    _budgetDay3,
				    _budgetDay4,
				    _budgetDay5,
				    _budgetDay6,
				    _budgetDay7,
				    budgetDay8,
				    budgetDay9,
				    budgetDay10
			    };
		    var grossStaffCalculator = new GrossStaffCalculator();
			var netStaffCalculator = new NetStaffCalculator(grossStaffCalculator);
		    netStaffCalculator.Initialize(_listOfBudgetDays);
			_target = new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator);
	    }

	    [Test]
        public void ShouldGetNetStaffFcAdjust()
        {
            var result = new BudgetCalculationResult
	            {
		            GrossStaff = 10000
	            };
            _target.Calculate(_budgetDay5, _listOfBudgetDays, ref result);
            var net = Math.Round(result.NetStaffFcAdj, 2);
            net.Should().Be.EqualTo(535.75d);
		}

		[Test]
		public void ShouldNetStaffFCAdjustShouldNotExceedMaxAgents()
		{
			var peakBudgetDay = new BudgetDay(_budgetGroup, _scenario, _date)
				{
					IsClosed = false,
					StaffEmployed = 100,
					AttritionRate = new Percent(0),
					Recruitment = 0,
					Contractors = 0,
					ForecastedHours = 800
				};

			var guid = Guid.NewGuid();
			var customShrinkage = new CustomShrinkage("Vacation");
			customShrinkage.SetId(guid);
			_budgetGroup.AddCustomShrinkage(customShrinkage);			
			peakBudgetDay.CustomShrinkages.SetShrinkage(customShrinkage.Id.GetValueOrDefault(), new Percent(0.1D));

			var budgetDays = fixBudgetDays(new List<IBudgetDay>
				{
					_budgetDay2,
					_budgetDay3,
					_budgetDay4,
					_budgetDay5,
					_budgetDay6,
					_budgetDay7
				}, customShrinkage.Id.GetValueOrDefault());
			budgetDays.Insert(0, peakBudgetDay);
			var grossStaffCalculator = new GrossStaffCalculator();
			var netStaffCalculator = new NetStaffCalculator(grossStaffCalculator);
			netStaffCalculator.Initialize(budgetDays);
			_target = new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator);
			
			_result = new BudgetCalculationResult
				{
					GrossStaff = 100
				};
			_target.Calculate(peakBudgetDay, budgetDays, ref _result);
			_result.NetStaffFcAdj.Should().Not.Be.GreaterThan(90);
		}

		private static IList<IBudgetDay> fixBudgetDays(IList<IBudgetDay> budgetDays, Guid customShrinkage)
		{
			foreach (var day in budgetDays)
			{
				day.IsClosed = false;
				day.StaffEmployed = 100;
				day.AttritionRate = new Percent(0);
				day.Recruitment = 0;
				day.Contractors = 0;
				day.ForecastedHours = 300;
				day.CustomShrinkages.SetShrinkage(customShrinkage, new Percent(0.01D));
			}
			return budgetDays;
		}
    }

}

