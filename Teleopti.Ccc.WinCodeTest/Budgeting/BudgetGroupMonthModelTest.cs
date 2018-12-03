using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupMonthModelTest
	{
		private BudgetGroupMonthDetailModel target;
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

			createBudgetDayList(budgetGroup, customShrinkageGuid, customEfficencyShrinkageGuid);

			budgetDayProvider = MockRepository.GenerateMock<IBudgetDayProvider>();
			budgetPermissionService = MockRepository.GenerateMock<IBudgetPermissionService>();
			budgetPermissionService.Stub(x => x.IsAllowancePermitted).Return(true);
			target = new BudgetGroupMonthDetailModel(budgetDays, budgetDayProvider, budgetPermissionService);
		}

		private void createBudgetDayList(BudgetGroup budgetGroup, Guid customShrinkageGuid, Guid customEfficencyShrinkageGuid)
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var startDay = new DateOnly(2010, 6, 1);

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

			var budgetDay6 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(5))
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

			var budgetDay7 = new BudgetDay(budgetGroup, scenario, startDay.AddDays(6))
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

			var list = new List<BudgetDay>();

			for (var i = 7; i < 30; i++)
			{
				var budgetDay = new BudgetDay(budgetGroup, scenario, startDay.AddDays(i))
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

				list.Add(budgetDay);
			}

			budgetDays = new List<IBudgetGroupDayDetailModel>
				{
					new BudgetGroupDayDetailModel(budgetDay1)
						{
							NetStaff = 12,
							GrossStaff = 17d,
							ForecastedStaff = 2,
							BudgetedStaff = 19,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay2)
						{
							NetStaff = 22,
							GrossStaff = 12d,
							BudgetedStaff = 11,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay3)
						{
							NetStaff = 45,
							GrossStaff = 3d,
							BudgetedStaff = 19,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay4)
						{
							NetStaff = 72,
							GrossStaff = 45d,
							BudgetedStaff = 12,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay5)
						{
							NetStaff = 88,
							GrossStaff = 6d,
							BudgetedStaff = 11,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay6)
						{
							NetStaff = 19,
							GrossStaff = 9d,
							BudgetedStaff = 19,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						},
					new BudgetGroupDayDetailModel(budgetDay7)
						{
							NetStaff = 13,
							GrossStaff = 2d,
							BudgetedStaff = 19,
							BudgetedLeave = 10,
							BudgetedSurplus = 6,
							FullAllowance = 10,
							ShrinkedAllowance = 8
						}
				};

			foreach (var t in list)
			{
				budgetDays.Add(new BudgetGroupDayDetailModel(t)
				{
					NetStaff = 13,
					GrossStaff = 2d,
					BudgetedStaff = 19,
					BudgetedLeave = 10,
					BudgetedSurplus = 6,
					FullAllowance = 10,
					ShrinkedAllowance = 8
				});
			}
		}

		[Test]
		public void ShouldCalculateAverageCustomEfficiencyShrinkage()
		{
			var shrink = Math.Round(target.GetEfficiencyShrinkage(customEfficiencyShrinkage).Value, 2);
			shrink.Should().Be.EqualTo(0.03);
		}

		[Test]
		public void ShouldHandleLockOnUpdate()
		{
			var lockable = new lockClassForTest();
			target.PropertyChanged += lockable.Update;
			target.DisablePropertyChangedInvocation();
			target.EnablePropertyChangedInvocation();

			Assert.AreEqual(1, lockable.LockCount);
			Assert.AreEqual(1, lockable.ReleaseCount);
		}

		private class lockClassForTest : ILockable
		{
			public int LockCount { get; private set; }
			public int ReleaseCount { get; private set; }
			private int UpdateCount { get; set; }

			public void Lock()
			{
				LockCount++;
			}

			public void Release()
			{
				ReleaseCount++;
			}

			public bool IsLocked
			{
				get { return LockCount != ReleaseCount; }
			}

			public void Update(object sender, PropertyChangedEventArgs e)
			{
				UpdateCount++;
			}
		}

		[Test]
		public void ShouldCalculateAverageCustomShrinkage()
		{
			var shrink = Math.Round(target.GetCustomShrinkage(customShrinkage).Value, 2);
			shrink.Should().Be.EqualTo(0.03);
		}

		[Test]
		public void ShouldCalculateAverageNetStaff()
		{
			var netStaff = Math.Round(target.NetStaff, 2);
			netStaff.Should().Be.EqualTo(19);
		}

		[Test]
		public void ShouldCalculateSumRecruitment()
		{
			var recruitment = Math.Round(target.Recruitment, 2);
			recruitment.Should().Be.EqualTo(380);
		}

		[Test]
		public void ShouldCalculateDifference()
		{
			var difference = Math.Round(target.Difference, 2);
			difference.Should().Be.EqualTo(8.03);
		}

		[Test]
		public void ShouldCalculateDifferencePercent()
		{
			var difference = Math.Round(target.DifferencePercent.Value, 2);
			difference.Should().Be.EqualTo(0.79);
		}

		[Test]
		public void ShouldCalculateForecastedStaff()
		{
			var forecastedFtes = Math.Round(target.ForecastedStaff, 2);
			forecastedFtes.Should().Be.EqualTo(10.20);
		}

		[Test]
		public void ShouldHaveAverageOfGrossStaff()
		{
			var gross = Math.Round(target.GrossStaff, 2);
			gross.Should().Be.EqualTo(4.67);
		}

		[Test]
		public void ShouldHandleSetOfGrossStaff()
		{
			target.GrossStaff = 6;
			target.GrossStaff.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleSetOfNetStaff()
		{
			target.NetStaff = 6;
			target.NetStaff.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleSetOfNetNetStaff()
		{
			target.NetNetStaff = 6;
			target.NetNetStaff.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleSetOfForecastedStaff()
		{
			target.ForecastedStaff = 6;
			target.ForecastedStaff.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleSetOfDifference()
		{
			target.Difference = 6;
			target.Difference.Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldHandleSetOfDifferencePercent()
		{
			target.DifferencePercent = new Percent(0.6);
			target.DifferencePercent.Value.Should().Be.EqualTo(0.6);
		}

		[Test]
		public void ShouldHaveSumStudentHours()
		{
			target.StudentsHours.Should().Be.EqualTo(1400); //Should this be an int? It's really 46.67.
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
		public void ShouldRecalculateAfterRelease()
		{
			target.Lock();
			target.BudgetDays.First().StudentsHours++;
			target.StudentsHours.Should().Be.EqualTo(1400);
			target.IsLocked.Should().Be.True();
			target.Release();

			target.StudentsHours.Should().Be.EqualTo(1401);
		}

		[Test]
		public void ShouldHaveAverageOfFte()
		{
			var fte = Math.Round(target.FulltimeEquivalentHours, 2);
			fte.Should().Be.EqualTo(10.63);
		}

		[Test]
		public void ShouldHaveSumOfOvertimeHours()
		{
			var overtime = Math.Round(target.OvertimeHours, 2);
			overtime.Should().Be.EqualTo(40);
		}

		[Test]
		public void ShouldHaveAverageOfDaysOffPerWeek()
		{
			var daysOff = Math.Round(target.DaysOffPerWeek, 2);
			daysOff.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveAverageOfAttritionRate()
		{
			var attrRate = Math.Round(target.AttritionRate.Value, 2);
			attrRate.Should().Be.EqualTo(0.19);
		}

		[Test]
		public void ShouldHaveASumOfDaysForecastedHours()
		{
			Assert.AreEqual(3255, target.ForecastedHours);
		}

		[Test]
		public void ShouldHaveAverageOfNetNetStaff()
		{
			Assert.AreEqual(18.23d, Math.Round(target.NetNetStaff, 2));
		}

		[Test]
		public void ShouldUpdateTotalForecastedHoursWhenChangingADay()
		{
			budgetDays[0].ForecastedHours = 501;
			Assert.AreEqual(3522, target.ForecastedHours);
		}

		[Test]
		public void ShouldUpdateAverageNetNetStaffWhenChangingADay()
		{
			budgetDays[0].BudgetedStaff = 99;
			Assert.AreEqual(20.90, Math.Round(target.NetNetStaff, 2));
		}

		[Test]
		public void ShouldHaveAverageOfBudgetLeave()
		{
			Assert.AreEqual(10, target.BudgetedLeave);
		}

		[Test]
		public void ShouldHaveAverageOfBudgetSurplus()
		{
			Assert.AreEqual(6, target.BudgetedSurplus);
		}

		[Test]
		public void ShouldHaveAverageOfFullAllowance()
		{
			Assert.AreEqual(10, target.FullAllowance);
		}

		[Test]
		public void ShouldHaveAverageOfShrinkedAllowance()
		{
			Assert.AreEqual(8, target.ShrinkedAllowance);
		}

		[Test]
		public void ShouldDistributeForecastedHoursToDaysFromMonth()
		{
			var notification = false; //hmm
			target.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == "ForecastedHours")
						notification = true;
				};

			target.ForecastedHours = 3000;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(100, budgetDays[i].ForecastedHours);
			Assert.AreEqual(3000, target.ForecastedHours);
			Assert.IsTrue(notification);
		}

		[Test]
		public void ShouldDistributeFte()
		{
			target.FulltimeEquivalentHours = 10;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(10, budgetDays[i].FulltimeEquivalentHours);
			Assert.AreEqual(10, target.FulltimeEquivalentHours);
		}

		[Test]
		public void ShouldDistributeAttritionRate()
		{
			target.AttritionRate = new Percent(0.25);
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(new Percent(0.25), budgetDays[i].AttritionRate);
			Assert.AreEqual(new Percent(0.25), target.AttritionRate);
		}

		[Test]
		public void ShouldDistributeAbsenceExtra()
		{
			target.AbsenceExtra = 2d;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(2d, budgetDays[i].AbsenceExtra);
			Assert.AreEqual(2d, target.AbsenceExtra);
		}

		[Test]
		public void ShouldDistributeAbsenceOverride()
		{
			target.AbsenceOverride = 2d;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(2d, budgetDays[i].AbsenceOverride);
			Assert.AreEqual(2d, target.AbsenceOverride);
		}

		[Test]
		public void ShouldDistributeAbsenceThreshold()
		{
			target.AbsenceThreshold = new Percent(0.25);
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(new Percent(0.25), budgetDays[i].AbsenceThreshold);
			Assert.AreEqual(new Percent(0.25), target.AbsenceThreshold);
		}

		[Test]
		public void ShouldHaveAverageFullAllowance()
		{
			var fullAllowance = Math.Round(target.FullAllowance, 2);
			fullAllowance.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldHaveAverageShrinkedAllowance()
		{
			var allowance = Math.Round(target.ShrinkedAllowance, 2);
			allowance.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSetRecruitmentOnFirstOpenDay()
		{
			foreach (var day in budgetDays)
				day.Recruitment = 0d;
			budgetDays[0].IsClosed = true;
			target.Recruitment = 360d;
			Assert.AreEqual(360d, budgetDays[1].Recruitment);
			for (var i = 0; i < 30; i++)
			{
				if (i == 1)
					continue;
				Assert.AreEqual(0d, budgetDays[i].Recruitment);
			}
			Assert.AreEqual(360d, target.Recruitment);
		}

		[Test]
		public void ShouldDistributeContractors()
		{
			target.Contractors = 45;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(1.5, budgetDays[i].Contractors);
			Assert.AreEqual(45, target.Contractors);
		}

		[Test]
		public void ShouldDistributeDaysOfPerWeek()
		{
			target.DaysOffPerWeek = 90;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(90, budgetDays[i].DaysOffPerWeek);
			Assert.AreEqual(90, target.DaysOffPerWeek);
		}

		[Test]
		public void ShouldDistributeOvertime()
		{
			target.OvertimeHours = 270;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(9, budgetDays[i].OvertimeHours);
			Assert.AreEqual(270, target.OvertimeHours);
		}

		[Test]
		public void ShouldDistributeStudents()
		{
			target.StudentsHours = 240;
			for (var i = 0; i < 30; i++)
				Assert.AreEqual(8, budgetDays[i].StudentsHours);
			Assert.AreEqual(240, target.StudentsHours);
		}

		[Test]
		public void ShouldSetStaffEmployedOnFirstDay()
		{
			//Reset staff employed
			foreach (var day in budgetDays)
				day.StaffEmployed = 0;

			target.StaffEmployed = 300;
			Assert.AreEqual(300, budgetDays[0].StaffEmployed);
			for (var i = 1; i < 30; i++)
				Assert.AreEqual(0, budgetDays[i].StaffEmployed);
			Assert.AreEqual(300, target.StaffEmployed);
		}

		[Test]
		public void ShouldShowMonthHeader()
		{
			var result = CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(budgetDays.First().BudgetDay.Day.Date.Month);
			//"June"
			target.Month.Should().Be.EqualTo(result);
		}

		[Test]
		public void ShouldShowYearInParentHeader()
		{
			const string result = "2010";
			target.Year.Should().Be.EqualTo(result);
		}

		[Test]
		public void StudenHours_ShouldOnlyDistributeOverOpenDays()
		{
			target.BudgetDays.Skip(20).ForEach(weekend => weekend.IsClosed = true);
			target.StudentsHours = 400;
			target.BudgetDays.Take(20).ForEach(weekday => weekday.StudentsHours.Should().Be.EqualTo(20));
			target.BudgetDays.Skip(20).ForEach(weekend => weekend.StudentsHours.Should().Be.EqualTo(0));
			target.StudentsHours.Should().Be.EqualTo(400);
		}

		[Test]
		public void OvertimeHours_ShouldOnlyDisitributeOverOpenDays()
		{
			target.BudgetDays.Skip(20).ForEach(weekend => weekend.IsClosed = true);
			target.OvertimeHours = 400;
			target.BudgetDays.Take(20).ForEach(weekday => weekday.OvertimeHours.Should().Be.EqualTo(20));
			target.BudgetDays.Skip(20).ForEach(weekend => weekend.OvertimeHours.Should().Be.EqualTo(0));
			target.OvertimeHours.Should().Be.EqualTo(400);
		}
	}
}