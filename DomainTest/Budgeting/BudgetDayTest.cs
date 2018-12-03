using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
	[TestFixture]
	public class BudgetDayTest
	{
		private MockRepository mocks;
		private IBudgetGroup budgetGroup;
		private IScenario scenario;
		private IBudgetDay target;
		private DateOnly date;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			budgetGroup = mocks.StrictMock<IBudgetGroup>();
			scenario = mocks.StrictMock<IScenario>();
			date = new DateOnly(2010, 9, 29);
			target = new BudgetDay(budgetGroup, scenario, date);
		}

		[Test]
		public void ShouldHavePropertiesSet()
		{
			Assert.AreEqual(budgetGroup, target.BudgetGroup);
			Assert.AreEqual(scenario, target.Scenario);
			Assert.AreEqual(date, target.Day);
		}

		[Test]
		public void ShouldHaveDefaultConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(), true));
		}

		[Test]
		public void ShouldBeAbleToChangeBudgetGroup()
		{
			var bg = new BudgetGroup();
			target.BudgetGroup = bg;
			Assert.AreEqual(target.BudgetGroup, bg);
		}

		[Test]
		public void ShouldBeAbleToChangeFulltimeEquivalentHours()
		{
			target.FulltimeEquivalentHours = 7.6d;
			Assert.AreEqual(7.6d, target.FulltimeEquivalentHours);
		}

		[Test]
		public void ShouldBeAbleToChangeStaffEmployed()
		{
			Assert.IsNull(target.StaffEmployed);
			target.StaffEmployed = 50d;
			Assert.AreEqual(50d, target.StaffEmployed);
		}

		[Test]
		public void ShouldBeAbleToChangeAttritionRate()
		{
			target.AttritionRate = new Percent(0.05);
			Assert.AreEqual(new Percent(0.05), target.AttritionRate);
		}

		[Test]
		public void ShouldBeAbleToChangeRecruitment()
		{
			target.Recruitment = 2.75d;
			Assert.AreEqual(2.75d, target.Recruitment);
		}

		[Test]
		public void ShouldBeAbleToChangeContractors()
		{
			target.Contractors = 2.5d;
			Assert.AreEqual(2.5d, target.Contractors);
		}

		[Test]
		public void ShouldBeAbleToChangeDaysOffPerWeek()
		{
			target.DaysOffPerWeek = 2d;
			Assert.AreEqual(2d, target.DaysOffPerWeek);
		}

		[Test]
		public void ShouldBeAbleToChangeOvertimeHours()
		{
			target.OvertimeHours = 2;
			Assert.AreEqual(2, target.OvertimeHours);
		}

		[Test]
		public void ShouldBeAbleToChangeStudentHours()
		{
			target.StudentHours = 2;
			Assert.AreEqual(2, target.StudentHours);
		}

		[Test]
		public void ShouldBeAbleToChangeForecastedHours()
		{
			target.ForecastedHours = 171d;
			Assert.AreEqual(171d, target.ForecastedHours);
		}

		[Test]
		public void ShouldBeAbleToCloseThatDay()
		{
			target.IsClosed = true;
			Assert.IsTrue(target.IsClosed);
		}

		[Test]
		public void ShouldBeAbleToChangeAbsenceThreshold()
		{
			var percentage = new Percent(0.85);
			target.AbsenceThreshold = percentage;
			Assert.AreEqual(percentage, target.AbsenceThreshold);
		}

		[Test]
		public void ShouldBeAbleToChangeAbsenceExtra()
		{
			target.AbsenceExtra = 2d;
			Assert.AreEqual(2d, target.AbsenceExtra);
		}

		[Test]
		public void ShouldBeAbleToChangeAbsenceOverride()
		{
			target.AbsenceOverride = 10d;
			Assert.AreEqual(10d, target.AbsenceOverride);
		}

		[Test]
		public void ShouldBeAbleToGetTheShrinkageValuesWrapper()
		{
			Assert.IsNotNull(target.CustomShrinkages);
		}

		[Test]
		public void ShouldBeAbleToGetTheEfficiencyShrinkageValuesWrapper()
		{
			Assert.IsNotNull(target.CustomEfficiencyShrinkages);
		}

		[Test]
		public void ShouldGetProperties()
		{
			target.FullAllowance = 10d;
			target.ShrinkedAllowance = 10d;
			target.NetStaffFcAdjustedSurplus = 10d;

			target.FullAllowance.Should().Be.EqualTo(10d);
			target.ShrinkedAllowance.Should().Be.EqualTo(10d);
			target.NetStaffFcAdjustedSurplus.Should().Be.EqualTo(10d);
		}

		[Test]
		public void ShouldRecalculate()
		{
			var theBudgetGroup = new BudgetGroup { Name = "BG" };
			theBudgetGroup.TrySetDaysPerYear(365);

			var budgetDay1 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 1));
			budgetDay1.StaffEmployed = 23;
			budgetDay1.AttritionRate = new Percent(0.1);
			var budgetDay2 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 2));
			budgetDay2.AttritionRate = new Percent(0.1);
			var budgetDay3 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 3));
			budgetDay3.AttritionRate = new Percent(0.1);

			var budgetCalculator = new BudgetCalculator(new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3 },
														new NetStaffCalculator(new GrossStaffCalculator()),
														new List<ICalculator>());
			var calculations = budgetDay2.Calculate(budgetCalculator);

			//Dont know about the value, this is tested in the calculator, Im testing the concept
			calculations.GrossStaff.Should().Be.EqualTo(22.987398986676673d);
		}

		[Test]
		public void ShouldCalculateWithoutNetStaffFcAdj()
		{
			var theBudgetGroup = new BudgetGroup { Name = "BG" };
			theBudgetGroup.TrySetDaysPerYear(365);

			var budgetDay1 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 1));
			budgetDay1.StaffEmployed = 23;
			budgetDay1.AttritionRate = new Percent(0.1);
			var budgetDay2 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 2));
			budgetDay2.AttritionRate = new Percent(0.1);
			var budgetDay3 = new BudgetDay(theBudgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 12, 3));
			budgetDay3.AttritionRate = new Percent(0.1);

			var budgetCalculator = new BudgetCalculator(new List<IBudgetDay> { budgetDay1, budgetDay2, budgetDay3 },
														new NetStaffCalculator(new GrossStaffCalculator()),
														new List<ICalculator>());
			var calculations = budgetDay2.CalculateWithoutNetStaffFcAdj(budgetCalculator, 123);

			//Dont know about the value, this is tested in the calculator, Im testing the concept
			calculations.GrossStaff.Should().Be.EqualTo(22.987398986676673d);
			calculations.NetStaffFcAdj.Should().Be.EqualTo(123);
		}

		[Test]
		public void ShouldRecalculateAllowance()
		{
			var g1 = Guid.NewGuid();
			var g2 = Guid.NewGuid();

			var vacation = mocks.StrictMock<ICustomShrinkage>();
			var coffee = mocks.StrictMock<ICustomEfficiencyShrinkage>();
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(g1)).Return(true);
				Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(g2)).Return(true);
				Expect.Call(budgetGroup.CustomShrinkages).Return(new List<ICustomShrinkage>
																	 {
																		 vacation
																	 }).Repeat.Any();

				Expect.Call(budgetGroup.CustomEfficiencyShrinkages).Return(new List<ICustomEfficiencyShrinkage>
																			   {
																				   coffee
																			   }).Repeat.Any();
				Expect.Call(budgetGroup.DaysPerYear).Return(365);
				Expect.Call(vacation.Id).Return(g1).Repeat.Any();
				Expect.Call(coffee.Id).Return(g2).Repeat.Any();
				Expect.Call(vacation.IncludedInAllowance).Return(true).Repeat.Any();
				Expect.Call(coffee.IncludedInAllowance).Return(true).Repeat.Any();
			}
			using (mocks.Playback())
			{
				target.FulltimeEquivalentHours = 8;
				target.StaffEmployed = 20;
				target.Contractors = 16d;
				target.ForecastedHours = 14 * 8;
				target.AbsenceThreshold = new Percent(0.8);
				target.AbsenceExtra = 2;
				target.CustomShrinkages.SetShrinkage(g1, new Percent(0.2d));
				target.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(g2, new Percent(0.1d));
				var grossStaffCalculator = new GrossStaffCalculator();
				var netStaffCalculator = new NetStaffCalculator(grossStaffCalculator);

				var calcList = new List<ICalculator>
				{
					new ForecastedStaffCalculator(),
					new NetStaffForecastAdjustCalculator(netStaffCalculator, grossStaffCalculator),
					new BudgetedStaffCalculator(),
					new DifferenceCalculator(),
					new DifferencePercentCalculator()
				};
				calcList.AddRange(new List<ICalculator>
					{
						new BudgetedLeaveCalculator(netStaffCalculator),
						new BudgetedSurplusCalculator(),
						new FullAllowanceCalculator(),
						new AllowanceCalculator()
					});
				var budgetCalculator = new BudgetCalculator(new List<IBudgetDay> { target },
															netStaffCalculator,
															calcList);
				var calculations = target.Calculate(budgetCalculator);
				Assert.AreEqual(1.84d, Math.Round(calculations.Difference, 2));
				Assert.AreEqual(4.40d, Math.Round(calculations.BudgetedLeave, 2));
				Assert.AreEqual(2.04d, Math.Round(calculations.BudgetedSurplus, 2));
				Assert.AreEqual(8.44d, Math.Round(calculations.FullAllowance, 2));
				Assert.AreEqual(6.76d, Math.Round(calculations.ShrinkedAllowance, 2));
			}
		}

		[Test]
		public void ShouldAllowanceBeZeroIfFteIsZero()
		{
			var g1 = Guid.NewGuid();
			var g2 = Guid.NewGuid();

			var vacation = mocks.StrictMock<ICustomShrinkage>();
			var coffee = mocks.StrictMock<ICustomEfficiencyShrinkage>();
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(g1)).Return(true);
				Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(g2)).Return(true);
				Expect.Call(budgetGroup.CustomShrinkages).Return(new List<ICustomShrinkage>
																	 {
																		 vacation
																	 }).Repeat.Any();

				Expect.Call(budgetGroup.CustomEfficiencyShrinkages).Return(new List<ICustomEfficiencyShrinkage>
																			   {
																				   coffee
																			   }).Repeat.Any();
				Expect.Call(budgetGroup.DaysPerYear).Return(365);
				Expect.Call(vacation.Id).Return(g1).Repeat.Any();
				Expect.Call(coffee.Id).Return(g2).Repeat.Any();
				Expect.Call(vacation.IncludedInAllowance).Return(true).Repeat.Any();
				Expect.Call(coffee.IncludedInAllowance).Return(true).Repeat.Any();
			}
			using (mocks.Playback())
			{
				target.FulltimeEquivalentHours = 0;
				target.StaffEmployed = 20;
				target.Contractors = 16d;
				target.ForecastedHours = 14 * 8;
				target.AbsenceThreshold = new Percent(0.8);
				target.AbsenceExtra = 2;
				target.CustomShrinkages.SetShrinkage(g1, new Percent(0.2d));
				target.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(g2, new Percent(0.1d));
				var netStaffCalculator = new NetStaffCalculator(new GrossStaffCalculator());
				var budgetCalculator = new BudgetCalculator(new List<IBudgetDay> { target },
															netStaffCalculator,
															new List<ICalculator>
																{
																	new DifferencePercentCalculator(),
																	new AllowanceCalculator()
																});
				var calculations = target.Calculate(budgetCalculator);
				Assert.AreEqual(0d, calculations.FullAllowance);
				Assert.AreEqual(0d, calculations.ShrinkedAllowance);
			}
		}
	}
}