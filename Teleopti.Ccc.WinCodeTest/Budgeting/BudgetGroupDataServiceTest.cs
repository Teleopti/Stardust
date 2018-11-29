using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupDataServiceTest
	{
		private BudgetGroupDataService _target;
		private MockRepository _mocks;
		private IBudgetDayRepository _mockRep;
		private IScenario _scenario;
		private BudgetGroup _budgetGroup;
		private DateOnlyPeriod _period;
		private IBudgetDayGapFiller _mockGapFiller;
		private BudgetGroupMainModel _mainModel;
		private IBudgetPermissionService budgetPermissionService;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_mockRep = _mocks.StrictMock<IBudgetDayRepository>();
			_mockGapFiller = _mocks.StrictMock<IBudgetDayGapFiller>();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_budgetGroup = new BudgetGroup { Name = "BG" };
			_budgetGroup.TrySetDaysPerYear(365);
			_period = new DateOnlyPeriod(2010, 2, 1, 2010, 2, 5);
			_mainModel = new BudgetGroupMainModel(null) { BudgetGroup = _budgetGroup, Period = _period, Scenario = _scenario };
			budgetPermissionService = MockRepository.GenerateMock<IBudgetPermissionService>();
			_target = new BudgetGroupDataService(_mainModel, _mockRep, _mockGapFiller, budgetPermissionService);
		}

		[Test]
		public void ShouldGetBudgetDaysAndFillTheGaps()
		{
			var day = new DateOnly(2010, 2, 1);
			var budgetDay1 = new BudgetDay(_budgetGroup, _scenario, day);
			var budgetDays = new List<IBudgetDay> { budgetDay1 };

			using (_mocks.Record())
			{
				Expect.Call(_mockRep.FindLastDayWithStaffEmployed(_scenario, _budgetGroup, _period.StartDate)).Return(_period.StartDate);
				Expect.Call(_mockRep.Find(_scenario, _budgetGroup, _period)).Return(budgetDays);
				Expect.Call(_mockGapFiller.AddMissingDays(budgetDays, _period)).Return(budgetDays);
			}

			using (_mocks.Playback())
			{
				var foundAndCreatedModels = _target.FindAndCreate();
				Assert.AreEqual(1, foundAndCreatedModels.Count);
			}
		}

		[Test]
		public void ShouldExtendThePeriodWithPreviousDateWithStaffEmployed()
		{
			var day = new DateOnly(2010, 2, 1);
			var budgetDay1 = new BudgetDay(_budgetGroup, _scenario, day);
			var budgetDays = new List<IBudgetDay> { budgetDay1 };
			var expandedPeriod = new DateOnlyPeriod(_period.StartDate.AddDays(-3), _period.EndDate);

			using (_mocks.Record())
			{
				Expect.Call(_mockRep.FindLastDayWithStaffEmployed(_scenario, _budgetGroup, _period.StartDate)).Return(_period.StartDate.AddDays(-3));
				Expect.Call(_mockRep.Find(_scenario, _budgetGroup, expandedPeriod)).Return(budgetDays);
				Expect.Call(_mockGapFiller.AddMissingDays(budgetDays, expandedPeriod)).Return(budgetDays);
			}

			using (_mocks.Playback())
			{
				var foundAndCreatedModels = _target.FindAndCreate();
				Assert.AreEqual(1, foundAndCreatedModels.Count);
			}
		}

		[Test]
		public void ShouldRecalculateModels()
		{
			var day = new DateOnly(2010, 2, 1);

			var models = new List<IBudgetGroupDayDetailModel>();

			for (var i = 0; i < 7; i++)
			{
				var budgetDay1 = new BudgetDay(_budgetGroup, _scenario, day.AddDays(i))
				{
					FulltimeEquivalentHours = 8,
					StaffEmployed = 23,
					ForecastedHours = 100,
					AbsenceThreshold = new Percent(1)
				};
				models.Add(new BudgetGroupDayDetailModel(budgetDay1));
			}
			models[0].GrossStaff.Should().Be.EqualTo(0);
			models[0].NetStaff.Should().Be.EqualTo(0);
			models[0].NetStaffFcAdj.Should().Be.EqualTo(0);
			models[0].BudgetedStaff.Should().Be.EqualTo(0);
			models[0].ForecastedStaff.Should().Be.EqualTo(0);
			models[0].Difference.Should().Be.EqualTo(0);
			models[0].DifferencePercent.Value.Should().Be.EqualTo(0);
			models[0].BudgetedLeave.Should().Be.EqualTo(0);
			models[0].BudgetedSurplus.Should().Be.EqualTo(0);
			models[0].ShrinkedAllowance.Should().Be.EqualTo(0);

			budgetPermissionService.Stub(x => x.IsAllowancePermitted).Return(true);
			_target.Recalculate(models);

			models[0].GrossStaff.Should().Be.EqualTo(23);
			models[0].NetStaff.Should().Be.EqualTo(23);
			models[0].NetStaffFcAdj.Should().Be.EqualTo(23);
			models[0].BudgetedStaff.Should().Be.EqualTo(23);
			models[0].ForecastedStaff.Should().Be.EqualTo(12.5);
			models[0].Difference.Should().Be.EqualTo(10.5);
			models[0].BudgetedLeave.Should().Be.EqualTo(0);
			models[0].BudgetedSurplus.Should().Be.EqualTo(10.5);
			models[0].ShrinkedAllowance.Should().Be.EqualTo(10.5);
			var percent = Math.Round(models[0].DifferencePercent.Value, 2);
			percent.Should().Be.EqualTo(0.84);
		}
	}
}