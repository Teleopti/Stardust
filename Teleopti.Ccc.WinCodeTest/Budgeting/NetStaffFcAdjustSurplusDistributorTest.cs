using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture, SetCulture("sv-SE")]
	public class NetStaffFcAdjustSurplusDistributorTest
	{
		private IBudgetGroupDayDetailModel _day0, _day1, _day2, _day3, _day4, _day5, _day6;
		private List<IBudgetGroupDayDetailModel> _models;
		private ICalculator _netStaffForecastAdjCalculator;
		private INetStaffCalculator _netStaffCalculator;
		private IGrossStaffCalculator _grossStaffCalculator;
		private IBudgetCalculator _calculator;

		[SetUp]
		public void Setup()
		{
			var budgetGroup = new BudgetGroup();
			budgetGroup.TrySetDaysPerYear(365);
			var scenario = new Scenario("Test");

			_day0 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 15)))
				{
					ForecastedHours = 800,
				};
			_day1 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 16)))
				{
					ForecastedHours = 300
				};
			_day2 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 17)))
				{
					ForecastedHours = 300
				};
			_day3 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 18)))
				{
					ForecastedHours = 300
				};
			_day4 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 19)))
				{
					ForecastedHours = 300
				};
			_day5 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 20)))
				{
					ForecastedHours = 100
				};
			_day6 = new BudgetGroupDayDetailModel(new BudgetDay(budgetGroup, scenario, new DateOnly(2013, 04, 21)))
				{
					ForecastedHours = 100
				};

			_models = new List<IBudgetGroupDayDetailModel>
				{
					_day0,
					_day1,
					_day2,
					_day3,
					_day4,
					_day5,
					_day6
				};

			foreach (var day in _models)
			{
				day.BudgetDay.NetStaffFcAdjustedSurplus = 0;
				day.BudgetDay.StaffEmployed = 90;
				day.BudgetDay.DaysOffPerWeek = 2;
				if (day == _day0)
					continue;
				day.NetStaffFcAdj = 61.36;
				if (day == _day5 || day == _day6)
					day.NetStaffFcAdj = 20.45;
			}
			_grossStaffCalculator = new GrossStaffCalculator();
			_netStaffCalculator = new NetStaffCalculator(_grossStaffCalculator);
			_netStaffForecastAdjCalculator = new NetStaffForecastAdjustCalculator(_netStaffCalculator, _grossStaffCalculator);
			var calculators = new List<ICalculator>
				{
					_netStaffForecastAdjCalculator
				};

			_calculator = new BudgetCalculator(_models.Select(d => d.BudgetDay), _netStaffCalculator,
			                                   calculators);

			_day0.BudgetDay.NetStaffFcAdjustedSurplus = 73.64;
			_day0.NetStaffFcAdj = 90;
		}

		[Test]
		public void ShouldDistributeSurplusFromOneDay()
		{
			NetStaffFcAdjustSurplusDistributor.Distribute(_calculator, _grossStaffCalculator, _netStaffForecastAdjCalculator,
			                                              _models);
			Math.Round(_day0.NetStaffFcAdj, 2).Should().Be.EqualTo(90);
			Math.Round(_day1.NetStaffFcAdj, 2).Should().Be.EqualTo(73.63);
			Math.Round(_day2.NetStaffFcAdj, 2).Should().Be.EqualTo(73.63);
			Math.Round(_day3.NetStaffFcAdj, 2).Should().Be.EqualTo(73.63);
			Math.Round(_day4.NetStaffFcAdj, 2).Should().Be.EqualTo(73.63);
			Math.Round(_day5.NetStaffFcAdj, 2).Should().Be.EqualTo(32.72);
			Math.Round(_day6.NetStaffFcAdj, 2).Should().Be.EqualTo(32.72);
		}
	}
}
