using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupDayDetailModelTest
	{
		private BudgetGroupDayDetailModel target;
		private IBudgetDay _budgetDay;
		private MockRepository mock;

		[SetUp]
		public void Setup()
		{
			mock = new MockRepository();
			var budgetGroup = new BudgetGroup();
			budgetGroup.Name = "BG";
			budgetGroup.TrySetDaysPerYear(365);
			budgetGroup.TimeZone = (TimeZoneInfo.GetSystemTimeZones()[5]);
			_budgetDay = new BudgetDay(budgetGroup, ScenarioFactory.CreateScenarioAggregate(), new DateOnly(2010, 8, 17));
			_budgetDay.AttritionRate = new Percent(0.1);
			_budgetDay.Contractors = 12d;
			_budgetDay.DaysOffPerWeek = 2d;
			_budgetDay.ForecastedHours = 23d;
			_budgetDay.FulltimeEquivalentHours = 14d;
			_budgetDay.OvertimeHours = 7;
			_budgetDay.Recruitment = 3d;
			_budgetDay.StaffEmployed = 9d;
			_budgetDay.StudentHours = 4;
			_budgetDay.IsClosed = true;
			_budgetDay.AbsenceThreshold = new Percent(0.8);
			_budgetDay.AbsenceExtra = 2d;
			_budgetDay.AbsenceOverride = 10d;
			_budgetDay.FullAllowance = 10d;
			_budgetDay.ShrinkedAllowance = 8d;
			target = new BudgetGroupDayDetailModel(_budgetDay);
		}

		[Test]
		public void ShouldSetValues()
		{
			target.AttritionRate.Should().Be.EqualTo(_budgetDay.AttritionRate);
			target.BudgetDay.Should().Be.EqualTo(_budgetDay);
			target.Contractors.Should().Be.EqualTo(_budgetDay.Contractors);
			target.Date.ToString().Should().Be.EqualTo(new DateDayModel(_budgetDay.Day).ToString());
			target.DaysOffPerWeek.Should().Be.EqualTo(_budgetDay.DaysOffPerWeek);
			target.ForecastedHours.Should().Be.EqualTo(_budgetDay.ForecastedHours);
			target.FulltimeEquivalentHours.Should().Be.EqualTo(_budgetDay.FulltimeEquivalentHours);
			target.OvertimeHours.Should().Be.EqualTo(_budgetDay.OvertimeHours);
			target.Recruitment.Should().Be.EqualTo(_budgetDay.Recruitment);
			target.StaffEmployed.Should().Be.EqualTo(_budgetDay.StaffEmployed);
			target.StudentsHours.Should().Be.EqualTo(_budgetDay.StudentHours);
			target.IsClosed.Should().Be.EqualTo(_budgetDay.IsClosed);
			target.AbsenceThreshold.Should().Be.EqualTo(_budgetDay.AbsenceThreshold);
			target.AbsenceExtra.Should().Be.EqualTo(_budgetDay.AbsenceExtra);
			target.AbsenceOverride.Should().Be.EqualTo(_budgetDay.AbsenceOverride);
			target.FullAllowance.Should().Be.EqualTo(_budgetDay.FullAllowance);
			target.ShrinkedAllowance.Should().Be.EqualTo(_budgetDay.ShrinkedAllowance);
		}

		[Test]
		public void ShouldRecalculate()
		{
			var calculator = mock.StrictMock<IBudgetCalculator>();
			var result = new BudgetCalculationResult(1, 2, 3, 4, 5, 7, new Percent(8), 9, 10, 11, 12d);
			using (mock.Record())
			{
				Expect.Call(calculator.Calculate(_budgetDay)).Return(result);
			}
			using (mock.Playback())
			{
				target.Recalculate(calculator);
				target.BudgetedStaff.Should().Be.EqualTo(result.BudgetedStaff);
				target.Difference.Should().Be.EqualTo(result.Difference);
				target.DifferencePercent.Should().Be.EqualTo(result.DifferencePercent);
				target.NetStaffFcAdj.Should().Be.EqualTo(result.NetStaffFcAdj);
				target.NetStaff.Should().Be.EqualTo(result.NetStaff);
				target.GrossStaff.Should().Be.EqualTo(result.GrossStaff);
				target.ForecastedStaff.Should().Be.EqualTo(result.ForecastedStaff);
				target.BudgetedLeave.Should().Be.EqualTo(result.BudgetedLeave);
				target.BudgetedSurplus.Should().Be.EqualTo(result.BudgetedSurplus);
				target.ShrinkedAllowance.Should().Be.EqualTo(result.ShrinkedAllowance);
				target.FullAllowance.Should().Be.EqualTo(result.FullAllowance);
			}
		}

		[Test]
		public void ShouldGetCustomShrinkageValue()
		{
			_budgetDay = mock.StrictMock<IBudgetDay>();
			var customShrinkageWrapper = mock.StrictMock<ICustomShrinkageWrapper>();
			var customShrinkage = mock.StrictMock<ICustomShrinkage>();
			var guid = Guid.NewGuid();
			using (mock.Record())
			{
				Expect.Call(_budgetDay.CustomShrinkages).Return(customShrinkageWrapper);
				Expect.Call(customShrinkageWrapper.GetShrinkage(guid)).Return(new Percent(1d));
				Expect.Call(customShrinkage.Id).Return(guid);
			}
			using (mock.Playback())
			{
				target.BudgetDay = _budgetDay;
				Assert.AreEqual(new Percent(1d), target.GetShrinkage(customShrinkage));
			}
		}

		[Test]
		public void ShouldSetCustomShrinkageValue()
		{
			_budgetDay = mock.StrictMock<IBudgetDay>();
			var customShrinkageWrapper = mock.StrictMock<ICustomShrinkageWrapper>();
			var customShrinkage = mock.StrictMock<ICustomShrinkage>();
			var guid = Guid.NewGuid();

			using (mock.Record())
			{
				Expect.Call(_budgetDay.CustomShrinkages).Return(customShrinkageWrapper);
				Expect.Call(() => customShrinkageWrapper.SetShrinkage(guid, new Percent(1d)));
				Expect.Call(customShrinkage.Id).Return(guid);
			}
			using (mock.Playback())
			{
				bool recalculationTriggered = false;
				target.Invalidate += (sender, e) => { recalculationTriggered = true; };
				target.BudgetDay = _budgetDay;
				target.SetShrinkage(customShrinkage, new Percent(1d));
				Assert.IsTrue(recalculationTriggered);
			}
		}

		[Test]
		public void ShouldGetCustomEfficiencyShrinkageValue()
		{
			_budgetDay = mock.StrictMock<IBudgetDay>();
			var customEfficiencyShrinkageWrapper = mock.StrictMock<ICustomEfficiencyShrinkageWrapper>();
			var customEfficiencyShrinkage = mock.StrictMock<ICustomEfficiencyShrinkage>();
			var guid = Guid.NewGuid();
			using (mock.Record())
			{
				Expect.Call(_budgetDay.CustomEfficiencyShrinkages).Return(customEfficiencyShrinkageWrapper);
				Expect.Call(customEfficiencyShrinkageWrapper.GetEfficiencyShrinkage(guid)).Return(new Percent(1d));
				Expect.Call(customEfficiencyShrinkage.Id).Return(guid);
			}
			using (mock.Playback())
			{
				target.BudgetDay = _budgetDay;
				Assert.AreEqual(new Percent(1d), target.GetEfficiencyShrinkage(customEfficiencyShrinkage));
			}
		}

		[Test]
		public void ShouldSetCustomEfficiencyShrinkageValue()
		{
			_budgetDay = mock.StrictMock<IBudgetDay>();
			var customEfficiencyShrinkageWrapper = mock.StrictMock<ICustomEfficiencyShrinkageWrapper>();
			var customEfficiencyShrinkage = mock.StrictMock<ICustomEfficiencyShrinkage>();
			var guid = Guid.NewGuid();

			using (mock.Record())
			{
				Expect.Call(_budgetDay.CustomEfficiencyShrinkages).Return(customEfficiencyShrinkageWrapper);
				Expect.Call(() => customEfficiencyShrinkageWrapper.SetEfficiencyShrinkage(guid, new Percent(1d)));
				Expect.Call(customEfficiencyShrinkage.Id).Return(guid);
			}
			using (mock.Playback())
			{
				bool recalculationTriggered = false;
				target.Invalidate += (sender, e) => { recalculationTriggered = true; };
				target.BudgetDay = _budgetDay;
				target.SetEfficiencyShrinkage(customEfficiencyShrinkage, new Percent(1d));
				Assert.IsTrue(recalculationTriggered);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void VerifyDateDayModel()
		{
			var date = new DateOnly(2010, 12, 23);
			var d = new DateDayModel(date);
			d.Date.Should().Be.EqualTo(date);
			d.ToString().Should().Be.EqualTo(date.Day.ToString(CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldSetDateFromModel()
		{
			var dateModel = new DateDayModel(new DateOnly(2011, 1, 1));
			target.Date = dateModel;

			target.Date.Date.Should().Be.EqualTo(dateModel.Date);
		}

		[Test, Culture("sv-SE"), SetUICulture("en-GB")]
		public void ShouldHandleWeekPropertyCorrectly()
		{
			var dateModel = new DateDayModel(new DateOnly(2011, 1, 1));
			target.Date = dateModel;

			target.Week.Should().Be.EqualTo("w. 52 2010-12-27");

			target.Week = "2011-01-08";
			target.Week.Should().Be.EqualTo("w. 1 2011-01-03");
		}

		[Test]
		public void ShouldTriggerInvalidate()
		{
			bool eventFired = false;
			target.Invalidate += (sender, e) =>
									 {
										 eventFired = true;
									 };

			target.TriggerRecalculation();

			eventFired.Should().Be.True();
		}
	}
}