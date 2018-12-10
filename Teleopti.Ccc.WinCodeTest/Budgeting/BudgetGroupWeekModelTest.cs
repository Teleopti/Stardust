using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupWeekModelTest
	{
		private BudgetGroupWeekDetailModel target;
		private IList<IBudgetGroupDayDetailModel> budgetDays;
		private ICustomShrinkage customShrinkage;
		private ICustomEfficiencyShrinkage customEfficiencyShrinkage;
		private IBudgetDayProvider budgetDayProvider;
		private IBudgetPermissionService budgetPermissionService;

		[SetUp]
		public void Setup()
		{
			var budgetGroup = new BudgetGroup { Name = "BG", TimeZone = (TimeZoneInfo.GetSystemTimeZones()[7]) };
			budgetGroup.TrySetDaysPerYear(365);
			customShrinkage = new CustomShrinkage("Gaffa");
			var customShrinkageGuid = Guid.NewGuid();
			customShrinkage.SetId(customShrinkageGuid);
			budgetGroup.AddCustomShrinkage(customShrinkage);

			var customEfficencyShrinkageGuid = Guid.NewGuid();
			customEfficiencyShrinkage = new CustomEfficiencyShrinkage("Eff");
			customEfficiencyShrinkage.SetId(customEfficencyShrinkageGuid);
			budgetGroup.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);

			createBudgetDays(budgetGroup, customShrinkageGuid, customEfficencyShrinkageGuid);

			budgetDayProvider = MockRepository.GenerateMock<IBudgetDayProvider>();
			budgetPermissionService = MockRepository.GenerateMock<IBudgetPermissionService>();
			budgetPermissionService.Stub(x => x.IsAllowancePermitted).Return(true);
			target = new BudgetGroupWeekDetailModel(budgetDays, budgetDayProvider, budgetPermissionService);
		}

		private void createBudgetDays(IBudgetGroup budgetGroup, Guid customShrinkageGuid, Guid customEfficencyShrinkageGuid)
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var startDay = new DateOnly(2010, 5, 9);

			//Set up the days
			var budgetDay1 = new BudgetDay(budgetGroup, scenario, startDay)
			{
				ForecastedHours = 234,
				AttritionRate = new Percent(0.1),
				DaysOffPerWeek = 2,
				OvertimeHours = 1,
				FulltimeEquivalentHours = 14d,
				StaffEmployed = 19d,
				Contractors = 12d,
				StudentHours = 3,
				Recruitment = 2
			};
			budgetDay1.CustomShrinkages.SetShrinkage(customShrinkageGuid, new Percent(0.2));
			budgetDay1.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(customEfficencyShrinkageGuid, new Percent(0.1));

			var budgetDay2 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(1))
			{
				ForecastedHours = 123,
				AttritionRate = new Percent(0.1),
				DaysOffPerWeek = 2,
				OvertimeHours = 2,
				FulltimeEquivalentHours = 4.2d,
				StaffEmployed = 19655d,
				Contractors = 9d,
				StudentHours = 4,
				Recruitment = 42
			};
			budgetDay2.CustomShrinkages.SetShrinkage(customShrinkageGuid, new Percent(0.15));
			budgetDay2.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(customEfficencyShrinkageGuid, new Percent(0.3));

			var budgetDay3 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(2))
			{
				ForecastedHours = 34,
				AttritionRate = new Percent(0.1),
				DaysOffPerWeek = 2,
				OvertimeHours = 4,
				FulltimeEquivalentHours = 9.6d,
				StaffEmployed = 2d,
				Contractors = 8d,
				StudentHours = 3,
				Recruitment = 56
			};
			budgetDay3.CustomShrinkages.SetShrinkage(customShrinkageGuid, new Percent(0.1));
			budgetDay3.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(customEfficencyShrinkageGuid, new Percent(0.4));

			var budgetDay4 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(3))
			{
				ForecastedHours = 19,
				AttritionRate = new Percent(0.1),
				DaysOffPerWeek = 3,
				OvertimeHours = 5,
				FulltimeEquivalentHours = 14d,
				StaffEmployed = 19d,
				Contractors = 2d,
				StudentHours = 13,
				Recruitment = 89
			};
			budgetDay4.CustomShrinkages.SetShrinkage(customShrinkageGuid, new Percent(0.1));

			var budgetDay5 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(4))
			{
				ForecastedHours = 144,
				AttritionRate = new Percent(0.3),
				DaysOffPerWeek = 1,
				OvertimeHours = 3,
				FulltimeEquivalentHours = 16d,
				Contractors = 0d,
				StudentHours = 32,
				Recruitment = 2
			};
			budgetDay5.CustomShrinkages.SetShrinkage(customShrinkageGuid, new Percent(0.25));

			var budgetDay6 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(4))
			{
				ForecastedHours = 112,
				AttritionRate = new Percent(0.2),
				DaysOffPerWeek = 2,
				OvertimeHours = 1,
				FulltimeEquivalentHours = 10.3d,
				Contractors = 0d,
				StudentHours = 56,
				Recruitment = 7
			};

			var budgetDay7 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(4))
			{
				ForecastedHours = 13,
				AttritionRate = new Percent(0.1),
				DaysOffPerWeek = 2,
				OvertimeHours = 1,
				FulltimeEquivalentHours = 14d,
				Contractors = 12d,
				StudentHours = 1,
				Recruitment = 21
			};
			budgetDay7.CustomEfficiencyShrinkages.SetEfficiencyShrinkage(customEfficencyShrinkageGuid, new Percent(0.1));

			budgetDays = new List<IBudgetGroupDayDetailModel>
				{
					new BudgetGroupDayDetailModel(budgetDay1)
						{
							NetStaff = 12,
							GrossStaff = 17d,
							ForecastedStaff = 2,
							BudgetedStaff = 19,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay2)
						{
							NetStaff = 22,
							GrossStaff = 12d,
							BudgetedStaff = 11,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay3)
						{
							NetStaff = 45,
							GrossStaff = 3d,
							BudgetedStaff = 19,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay4)
						{
							NetStaff = 72,
							GrossStaff = 45d,
							BudgetedStaff = 12,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay5)
						{
							NetStaff = 88,
							GrossStaff = 6d,
							BudgetedStaff = 11,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay6)
						{
							NetStaff = 19,
							GrossStaff = 9d,
							BudgetedStaff = 19,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay7)
						{
							NetStaff = 13,
							GrossStaff = 2d,
							BudgetedStaff = 19,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						}
				};
		}

		[Test]
		public void ShouldCalculateAverageCustomEfficiencyShrinkage()
		{
			var shrink = Math.Round(target.GetEfficiencyShrinkage(customEfficiencyShrinkage).Value, 2);
			shrink.Should().Be.EqualTo(0.13);
		}

		[Test]
		public void ShouldCalculateAverageCustomShrinkage()
		{
			var shrink = Math.Round(target.GetCustomShrinkage(customShrinkage).Value, 2);
			shrink.Should().Be.EqualTo(0.11);
		}

		[Test]
		public void ShouldSetCustomShrinkage()
		{
			target.SetCustomShrinkage(customShrinkage, new Percent(0.05d));
			budgetDays[0].GetShrinkage(customShrinkage).Value.Should().Be.EqualTo(0.05);
		}

		[Test]
		public void ShouldSetCustomEfficiencyShrinkage()
		{
			target.SetCustomEfficiencyShrinkage(customEfficiencyShrinkage, new Percent(0.05d));
			budgetDays[0].GetEfficiencyShrinkage(customEfficiencyShrinkage).Value.Should().Be.EqualTo(0.05);
		}

		[Test]
		public void ShouldCalculateAverageNetStaff()
		{
			var netStaff = Math.Round(target.NetStaff, 2);
			netStaff.Should().Be.EqualTo(38.71);
		}

		[Test]
		public void ShouldCalculateSumRecruitment()
		{
			var recruitment = Math.Round(target.Recruitment, 2);
			recruitment.Should().Be.EqualTo(219);
		}

		[Test]
		public void ShouldCalculateDifference()
		{
			//38.71d - 8.27
			var difference = Math.Round(target.Difference, 2);
			difference.Should().Be.EqualTo(7.44);
		}

		[Test]
		public void ShouldCalculateDifferencePercent()
		{
			var difference = Math.Round(target.DifferencePercent.Value, 2);
			difference.Should().Be.EqualTo(0.9);
		}

		[Test]
		public void ShouldCalculateFullAllowance()
		{
			var fullAllowance = Math.Round(target.FullAllowance, 2);
			fullAllowance.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldCalculateAllowance()
		{
			var allowance = Math.Round(target.ShrinkedAllowance, 2);
			allowance.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldCalculateForecastedStaff()
		{
			var forecastedFtes = Math.Round(target.ForecastedStaff, 2);
			forecastedFtes.Should().Be.EqualTo(8.27);
		}

		[Test]
		public void ShouldHaveAverageOfGrossStaff()
		{
			var gross = Math.Round(target.GrossStaff, 2);
			gross.Should().Be.EqualTo(13.43);
		}

		[Test]
		public void ShouldHaveAverageStudentHours()
		{
			target.StudentsHours.Should().Be.EqualTo(112);
		}

		[Test]
		public void ShouldHaveSumOfContractors()
		{
			var contractors = Math.Round(target.Contractors, 2);
			contractors.Should().Be.EqualTo(43);
		}

		[Test]
		public void ShouldGetTheStaffEmployedFromFirstDay()
		{
			target.StaffEmployed.Should().Be.EqualTo(19d);
		}

		[Test]
		public void ShouldHaveAverageOfFte()
		{
			var fte = Math.Round(target.FulltimeEquivalentHours, 2);
			fte.Should().Be.EqualTo(11.73);
		}

		[Test]
		public void ShouldHaveSumOfOvertimeHours()
		{
			var overtime = Math.Round(target.OvertimeHours, 2);
			overtime.Should().Be.EqualTo(17);
		}

		[Test]
		public void ShouldHaveAverageOfDaysOfPerWeek()
		{
			var daysOff = Math.Round(target.DaysOffPerWeek, 2);
			daysOff.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveAverageOfAttritionRate()
		{
			var attrRate = Math.Round(target.AttritionRate.Value, 2);
			attrRate.Should().Be.EqualTo(0.14);
		}

		[Test]
		public void ShouldHaveASumOfDaysForecastedHours()
		{
			Assert.AreEqual(679, target.ForecastedHours);
		}

		[Test]
		public void ShouldHaveAverageOfNetNetStaff()
		{
			Assert.AreEqual(15.71d, Math.Round(target.NetNetStaff, 2));
		}

		[Test]
		public void ShouldUpdateTotalForecastedHoursWhenChangingADay()
		{
			budgetDays[0].ForecastedHours = 501;
			Assert.AreEqual(946, target.ForecastedHours);
		}

		[Test]
		public void ShouldUpdateAverageNetStaffWhenChangingADay()
		{
			budgetDays[0].BudgetedStaff = 99;
			Assert.AreEqual(27.14, Math.Round(target.NetNetStaff, 2));
		}

		[Test]
		public void ShouldDistributeForecastedHoursToDaysFromWeek()
		{
			var notification = false; //hmm
			target.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "ForecastedHours")
						notification = true;
				};

			target.ForecastedHours = 700;
			Assert.AreEqual(100, budgetDays[0].ForecastedHours);
			Assert.AreEqual(100, budgetDays[1].ForecastedHours);
			Assert.AreEqual(100, budgetDays[2].ForecastedHours);
			Assert.AreEqual(100, budgetDays[3].ForecastedHours);
			Assert.AreEqual(100, budgetDays[4].ForecastedHours);
			Assert.AreEqual(100, budgetDays[5].ForecastedHours);
			Assert.AreEqual(100, budgetDays[6].ForecastedHours);
			Assert.AreEqual(700, target.ForecastedHours);
			Assert.IsTrue(notification);
		}

		[Test]
		public void ShouldDistributeFte()
		{
			target.FulltimeEquivalentHours = 7;
			Assert.AreEqual(7, budgetDays[0].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[1].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[2].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[3].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[4].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[5].FulltimeEquivalentHours);
			Assert.AreEqual(7, budgetDays[6].FulltimeEquivalentHours);
			Assert.AreEqual(7, target.FulltimeEquivalentHours);
		}

		[Test]
		public void ShouldDistributeAttritionRate()
		{
			target.AttritionRate = new Percent(0.25);
			Assert.AreEqual(new Percent(0.25), budgetDays[0].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[1].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[2].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[3].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[4].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[5].AttritionRate);
			Assert.AreEqual(new Percent(0.25), budgetDays[6].AttritionRate);
			Assert.AreEqual(new Percent(0.25), target.AttritionRate);
		}

		[Test]
		public void ShouldSetRecruitmentOnFirstDay()
		{
			foreach (var day in budgetDays)
				day.Recruitment = 0d;

			target.Recruitment = 20;
			Assert.AreEqual(20, budgetDays[0].Recruitment);
			Assert.AreEqual(0, budgetDays[1].Recruitment);
			Assert.AreEqual(0, budgetDays[2].Recruitment);
			Assert.AreEqual(0, budgetDays[3].Recruitment);
			Assert.AreEqual(0, budgetDays[4].Recruitment);
			Assert.AreEqual(0, budgetDays[5].Recruitment);
			Assert.AreEqual(0, budgetDays[6].Recruitment);
			Assert.AreEqual(20, target.Recruitment);
		}

		[Test]
		public void ShouldDistributeContractors()
		{
			target.Contractors = 161;
			Assert.AreEqual(23, budgetDays[0].Contractors);
			Assert.AreEqual(23, budgetDays[1].Contractors);
			Assert.AreEqual(23, budgetDays[2].Contractors);
			Assert.AreEqual(23, budgetDays[3].Contractors);
			Assert.AreEqual(23, budgetDays[4].Contractors);
			Assert.AreEqual(23, budgetDays[5].Contractors);
			Assert.AreEqual(23, budgetDays[6].Contractors);
			Assert.AreEqual(161, target.Contractors);
		}

		[Test]
		public void ShouldDistributeDaysOfPerWeek()
		{
			target.DaysOffPerWeek = 3; //hmm konstig siffra, borde vara max 7?!?
			Assert.AreEqual(3, budgetDays[0].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[1].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[2].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[3].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[4].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[5].DaysOffPerWeek);
			Assert.AreEqual(3, budgetDays[6].DaysOffPerWeek);
			Assert.AreEqual(3, target.DaysOffPerWeek);
		}

		[Test]
		public void ShouldDistributeOvertime()
		{
			target.OvertimeHours = 63;
			Assert.AreEqual(9, budgetDays[0].OvertimeHours);
			Assert.AreEqual(9, budgetDays[1].OvertimeHours);
			Assert.AreEqual(9, budgetDays[2].OvertimeHours);
			Assert.AreEqual(9, budgetDays[3].OvertimeHours);
			Assert.AreEqual(9, budgetDays[4].OvertimeHours);
			Assert.AreEqual(9, budgetDays[5].OvertimeHours);
			Assert.AreEqual(9, budgetDays[6].OvertimeHours);
			Assert.AreEqual(63, target.OvertimeHours);
		}

		[Test]
		public void ShouldDistributeStudents()
		{
			target.StudentsHours = 63;
			Assert.AreEqual(9, budgetDays[0].StudentsHours);
			Assert.AreEqual(9, budgetDays[1].StudentsHours);
			Assert.AreEqual(9, budgetDays[2].StudentsHours);
			Assert.AreEqual(9, budgetDays[3].StudentsHours);
			Assert.AreEqual(9, budgetDays[4].StudentsHours);
			Assert.AreEqual(9, budgetDays[5].StudentsHours);
			Assert.AreEqual(9, budgetDays[6].StudentsHours);
			Assert.AreEqual(63, target.StudentsHours);
		}

		[Test]
		public void AllDaysClosedShouldNotCrash()
		{
			budgetPermissionService.Stub(x => x.IsAllowancePermitted).Return(true);

			budgetDays[0].IsClosed = true;
			budgetDays[1].IsClosed = true;
			budgetDays[2].IsClosed = true;
			budgetDays[3].IsClosed = true;
			budgetDays[4].IsClosed = true;
			budgetDays[5].IsClosed = true;
			budgetDays[6].IsClosed = true;

			var netStaff = Math.Round(target.NetStaff, 2);
			var netNetStaff = Math.Round(target.NetNetStaff, 2);
			var forecastedStaff = Math.Round(target.ForecastedStaff, 2);
			netStaff.Should().Be.EqualTo(0);
			netNetStaff.Should().Be.EqualTo(0);
			forecastedStaff.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldDistributeAbsenceExtra()
		{
			target.AbsenceExtra = 2d;
			Assert.AreEqual(2d, budgetDays[0].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[1].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[2].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[3].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[4].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[5].AbsenceExtra);
			Assert.AreEqual(2d, budgetDays[6].AbsenceExtra);
			Assert.AreEqual(2d, target.AbsenceExtra);
		}

		[Test]
		public void ShouldDistributeAbsenceOverride()
		{
			target.AbsenceOverride = 2d;
			Assert.AreEqual(2d, budgetDays[0].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[1].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[2].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[3].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[4].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[5].AbsenceOverride);
			Assert.AreEqual(2d, budgetDays[6].AbsenceOverride);
			Assert.AreEqual(2d, target.AbsenceOverride);
		}

		[Test]
		public void ShouldDistributeAbsenceThreshold()
		{
			var threshold = new Percent(0.8);
			target.AbsenceThreshold = threshold;
			Assert.AreEqual(threshold, budgetDays[0].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[1].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[2].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[3].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[4].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[5].AbsenceThreshold);
			Assert.AreEqual(threshold, budgetDays[6].AbsenceThreshold);
			Assert.AreEqual(threshold, target.AbsenceThreshold);
		}

		[Test]
		public void ClosedDaysShouldNotBeInCalculation()
		{
			budgetPermissionService.Stub(x => x.IsAllowancePermitted).Return(true);

			budgetDays[0].IsClosed = true;
			budgetDays[1].IsClosed = true;

			var netStaff = Math.Round(target.NetStaff, 2);
			var netNetStaff = Math.Round(target.NetNetStaff, 2);
			var forecastedStaff = Math.Round(target.ForecastedStaff, 2);
			netStaff.Should().Be.EqualTo(47.4); //(45+72+88+19+13)/5
			netNetStaff.Should().Be.EqualTo(16); //(19+12+11+19+14)/5
			forecastedStaff.Should().Be.EqualTo(5.04);
		}

		[Test]
		public void ShouldSetStaffEmployedOnFirstDay()
		{
			//Reset staff employed
			foreach (var day in budgetDays)
				day.StaffEmployed = 0;

			target.StaffEmployed = 300;
			Assert.AreEqual(300, budgetDays[0].StaffEmployed);
			Assert.AreEqual(0, budgetDays[1].StaffEmployed);
			Assert.AreEqual(0, budgetDays[2].StaffEmployed);
			Assert.AreEqual(0, budgetDays[3].StaffEmployed);
			Assert.AreEqual(0, budgetDays[4].StaffEmployed);
			Assert.AreEqual(0, budgetDays[5].StaffEmployed);
			Assert.AreEqual(0, budgetDays[6].StaffEmployed);
			Assert.AreEqual(300, target.StaffEmployed);
		}

		[Test, SetCulture("en-US")]
		public void ShouldShowWeekHeader()
		{
			var result = string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.WeekAbbreviationDot, 20,
				budgetDays.First().BudgetDay.Day.ToShortDateString());

			//"w. 20 5/9/2010"
			target.Week.Should().Be.EqualTo(result);
		}

		[Test]
		public void ShouldShowMonthYearHeader()
		{
			var result = string.Format(CultureInfo.CurrentUICulture, "{0} {1}",
				CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(
					CultureInfo.CurrentUICulture.Calendar.GetMonth(budgetDays.First().BudgetDay.Day.Date)),
				CultureInfo.CurrentUICulture.Calendar.GetYear(budgetDays.First().BudgetDay.Day.Date));

			//"June 2010"
			target.MonthYear.Should().Be.EqualTo(result);
		}

		[Test]
		public void StudenHours_ShouldOnlyDistributeOverOpenDays()
		{
			target.BudgetDays.Skip(5).ForEach(b => b.IsClosed = true);
			target.StudentsHours = 100;
			target.BudgetDays.Take(5).ForEach(b => b.StudentsHours.Should().Be.EqualTo(20));
			target.BudgetDays.Skip(5).ForEach(b => b.StudentsHours.Should().Be.EqualTo(0));

			target.StudentsHours.Should().Be.EqualTo(100);
		}

		[Test]
		public void OvertimeHours_ShouldOnlyDistributeOverOpenDays()
		{
			target.BudgetDays.Skip(5).ForEach(b => b.IsClosed = true);
			target.OvertimeHours = 100;
			target.BudgetDays.Take(5).ForEach(b => b.OvertimeHours.Should().Be.EqualTo(20));
			target.BudgetDays.Skip(5).Take(2).ForEach(b => b.OvertimeHours.Should().Be.EqualTo(0));
			target.OvertimeHours.Should().Be.EqualTo(100);
		}
	}
}