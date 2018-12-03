using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupDayPresenterTest
	{
		private BudgetGroupDayPresenter target;
		private MockRepository mock;
		private IBudgetGroupDayView view;
		private BudgetGroupDayModel model;
		private IBudgetGroup budgetGroup;
		private IBudgetDayProvider budgetDayProvider;

		[SetUp]
		public void Setup()
		{
			mock = new MockRepository();
			view = mock.DynamicMock<IBudgetGroupDayView>();
			budgetDayProvider = mock.StrictMock<IBudgetDayProvider>();
			budgetGroup = new BudgetGroup { Name = "BG" };
			var period = new DateOnlyPeriod(2010, 8, 12, 2010, 9, 13);
		    var budgetGroupMainModel = new BudgetGroupMainModel(null)
		                                   {
		                                       BudgetGroup = budgetGroup,
		                                       Period = period,
		                                       Scenario = ScenarioFactory.CreateScenarioAggregate()
		                                   };

			model = new BudgetGroupDayModel(budgetGroupMainModel, budgetDayProvider);
			target = new BudgetGroupDayPresenter(view, model);
		}

		[Test]
		public void CanInitializeAndCallsAreMade()
		{
			var allBudgetDays = new List<IBudgetGroupDayDetailModel>();
			using (mock.Record())
			{
			    Expect.Call(budgetDayProvider.Recalculate);
				Expect.Call(budgetDayProvider.VisibleDayModels()).Return(allBudgetDays);
				view.DataSource = allBudgetDays;
			}

			using (mock.Playback())
			{
				target.Initialize();
			}
		}

		[Test]
		public void ShouldUpdateShrinkageRows()
		{
			var customShrinkage = new CustomShrinkage("SomeSickness");
			using (mock.Record())
			{
				Expect.Call(() => view.AddShrinkageRow(customShrinkage));
			}
			using (mock.Playback())
			{
				target.AddShrinkageRow(customShrinkage);
			}
		}

		[Test]
		public void ShouldAddCustomShrinkageRowsToView()
		{
			var customShrinkage = new CustomShrinkage("SomSickness");
			budgetGroup.AddCustomShrinkage(customShrinkage);
			var budgetDays = new List<IBudgetGroupDayDetailModel>();
			using (mock.Record())
			{
				Expect.Call(budgetDayProvider.VisibleDayModels()).Return(budgetDays);
				Expect.Call(() => view.AddShrinkageRow(customShrinkage));
				Expect.Call(budgetDayProvider.Recalculate);
			} 
			using (mock.Playback())
			{
				target.Initialize();
			}
		}

		[Test]
		public void ShouldUpdateEfficiencyShrinkageRows()
		{
			var customEfficiencyShrinkage = new CustomEfficiencyShrinkage("SomeSickness");
			using (mock.Record())
			{
				Expect.Call(() => view.AddEfficiencyShrinkageRow(customEfficiencyShrinkage));
			}
			using (mock.Playback())
			{
				target.AddEfficiencyShrinkageRow(customEfficiencyShrinkage);
			}
		}

		[Test]
		public void ShouldAddCustomEfficiencyShrinkageRowsToView()
		{
			var customEfficiencyShrinkage = new CustomEfficiencyShrinkage("SomSickness");
			budgetGroup.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);
			var budgetDays = new List<IBudgetGroupDayDetailModel>();
			using (mock.Record())
			{
				Expect.Call(budgetDayProvider.VisibleDayModels()).Return(budgetDays);
				Expect.Call(() => view.AddEfficiencyShrinkageRow(customEfficiencyShrinkage));
				Expect.Call(budgetDayProvider.Recalculate);
			}
			using (mock.Playback())
			{
				target.Initialize();
			}
		}

        [Test]
        public void ShouldReloadShrinkages()
        {
            using (mock.Record())
            {
                Expect.Call(() => view.AddShrinkageRow(null)).IgnoreArguments().Repeat.Any();
            }
            using (mock.Playback())
            {
                target.InitializeShrinkages();
            }
        }
        
        [Test]
        public void ShouldReloadEfficiencyShrinkages()
        {
            using (mock.Record())
            {
                Expect.Call(() => view.AddEfficiencyShrinkageRow(null)).IgnoreArguments().Repeat.Any();
            }
            using (mock.Playback())
            {
                target.InitializeEfficiencyShrinkages();
            }
        }
	}
}
