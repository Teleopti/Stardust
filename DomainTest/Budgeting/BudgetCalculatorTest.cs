using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture, SetCulture("sv-SE")]
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
			_budgetGroup.TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			_date = new DateOnly(2010, 02, 01);
			_budgetDay1 = new BudgetDay(_budgetGroup, _scenario, _date) { AttritionRate = new Percent(0.1), StaffEmployed = 777, Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 171, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay2 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(1)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay3 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(2)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay4 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(3)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3, AbsenceOverride = 10, AbsenceThreshold = new Percent(0.8) };
			_budgetDay5 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(4)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 151, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay6 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(5)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 120, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay7 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(6)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 152, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay8 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(7)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 168, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay9 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(8)) { AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 162, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_budgetDay10 = new BudgetDay(_budgetGroup, _scenario, _date.AddDays(9)) { IsClosed = true, AttritionRate = new Percent(0.1), Recruitment = 0, Contractors = 22.8, DaysOffPerWeek = 2, ForecastedHours = 156, FulltimeEquivalentHours = 7.6, OvertimeHours = 2, StudentHours = 3 };
			_listOfBudgetDays = new List<IBudgetDay> { _budgetDay1, _budgetDay2, _budgetDay3, _budgetDay4, _budgetDay5, _budgetDay6, _budgetDay7, _budgetDay8, _budgetDay9, _budgetDay10 };

			var grossStaffCalculator = new GrossStaffCalculator();
			var netStaffCalculator = new NetStaffCalculator(grossStaffCalculator);

			_calcList = new List<ICalculator>
				{
					new ForecastedStaffCalculator(),
					new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator),
					new BudgetedStaffCalculator(),
					new DifferenceCalculator(),
					new DifferencePercentCalculator()
				};
			_calcList.AddRange(new List<ICalculator>
					{
						new BudgetedLeaveCalculator(netStaffCalculator),
						new BudgetedSurplusCalculator(),
						new FullAllowanceCalculator(),
						new AllowanceCalculator()
					});
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
			result.FullAllowance.Should().Be.EqualTo(10);
			result.ShrinkedAllowance.Should().Be.EqualTo(8);
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
			result.FullAllowance.Should().Be.EqualTo(0);
			result.ShrinkedAllowance.Should().Be.EqualTo(0);
		}

		[Test]
		public void AllowanceShouldEqualAbsenceOverriteWhenDayIsClosed()
		{
			var result = _target.Calculate(_budgetDay4);
			result.FullAllowance.Should().Be.EqualTo(10);
			result.ShrinkedAllowance.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldCalculateWithoutnetStaffFcAdj()
		{
			var result = _target.CalculateWithoutNetStaffFcAdj(_budgetDay4, 596.07024229401054d);
			result.GrossStaff.Should().Be.EqualTo(776.1488430210959d);
			result.NetStaff.Should().Be.EqualTo(556.53488787221136d);
			result.BudgetedStaff.Should().Be.EqualTo(596.72813703085262d);
			result.ForecastedStaff.Should().Be.EqualTo(22.105263157894736d);
			result.Difference.Should().Be.EqualTo(574.62287387295794d);
			result.DifferencePercent.Should().Be.EqualTo(new Percent(25.994844294252861d));
			result.BudgetedLeave.Should().Be.EqualTo(0);
			result.BudgetedSurplus.Should().Be.EqualTo(574.62287387295794d);
			result.FullAllowance.Should().Be.EqualTo(10);
			result.ShrinkedAllowance.Should().Be.EqualTo(8);
		}

		[Test]
		public void VerifyCalculatorList()
		{
			var list = _target.CalculatorList;
			_target.CalculatorList = list;
			list.Count.Should().Not.Be.EqualTo(0);
			_target.CalculatorList.Count.Should().Not.Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyResultOnClosedDay()
		{
			var result = _target.CalculateWithoutNetStaffFcAdj(_budgetDay10, 0);
			result.GrossStaff.Should().Be.EqualTo(0);
			result.NetStaff.Should().Be.EqualTo(0);
			result.NetStaffFcAdj.Should().Be.EqualTo(0);
			result.BudgetedStaff.Should().Be.EqualTo(0);
			result.ForecastedStaff.Should().Be.EqualTo(0);
			result.Difference.Should().Be.EqualTo(0);
			result.DifferencePercent.Should().Be.EqualTo(new Percent(0));
			result.BudgetedLeave.Should().Be.EqualTo(0);
			result.BudgetedSurplus.Should().Be.EqualTo(0);
			result.FullAllowance.Should().Be.EqualTo(0);
			result.ShrinkedAllowance.Should().Be.EqualTo(0);
		}
	}
}