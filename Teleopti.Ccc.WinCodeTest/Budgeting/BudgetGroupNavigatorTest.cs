using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetGroupNavigatorTest
    {
        private BudgetGroupNavigatorPresenter target;
        private IBudgetGroupNavigatorView view;
        private MockRepository mock;
        private IBudgetNavigatorDataService service;
        private BudgetGroupNavigatorModel model;
        private IPortalSettings settings;

        [SetUp]
        public void Setup()
        {
            mock = new MockRepository();
            view = mock.StrictMock<IBudgetGroupNavigatorView>();
            service = mock.StrictMock<IBudgetNavigatorDataService>();
            settings = new FakeSettings();
            model = new BudgetGroupNavigatorModel(settings, service); //Sets ActionHeigt to 13
            target = new BudgetGroupNavigatorPresenter(view, model);
        }

        [Test]
        public void CanInitialize()
        {
            var models = FakeRepository.All();
            
            using(mock.Record())
            {
                Expect.Call(service.GetBudgetRootModels()).Return(models);
                view.BudgetGroupRootModel = models;
                view.BudgetingActionPaneHeight = model.BudgetingActionPaneHeight;
            }
            using (mock.Playback())
            {
                target.Initialize();
                Assert.AreEqual(13, model.BudgetingActionPaneHeight);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void TestConstantImageIndexes()
        {
            var models = FakeRepository.All();
            Assert.AreEqual(2, models.ImageIndex);
            Assert.AreEqual(UserTexts.Resources.Budgeting, models.DisplayName);
            foreach (var budgetGroupModel in models.BudgetGroups)
            {
                Assert.AreEqual(1, budgetGroupModel.ImageIndex);

                foreach (var skillModel in budgetGroupModel.SkillModels)
                {
                    Assert.AreEqual(0, skillModel.ImageIndex);
                }
            }
        }

        [Test]
        public void SelectedModelIsCalledInView()
        {
            var budgetGroup = new BudgetGroup();
            var budgetGroupModel = new BudgetGroupModel(budgetGroup);
            using (mock.Record())
            {
                Expect.Call(view.SelectedModel).Return(budgetGroupModel).Repeat.Once();
            }
            target.GetSelectedEntity();
        }

        [Test]
        public void CanDeleteBudgetGroup()
        {
            var budgetGroup = new BudgetGroup();
            var budgetGroupModel = new BudgetGroupModel(budgetGroup);
            using (mock.Record())
            {
                Expect.Call(view.SelectedModel).Return(budgetGroupModel).Repeat.Once();
                Expect.Call(() => service.DeleteBudgetGroup(budgetGroup));
            }
            target.DeleteBudgetGroup();
        }

		[Test]
		public void ShouldLoadBudgetGroup()
		{
			var budgetGroup = new BudgetGroup();
			var budgetGroupModel = new BudgetGroupModel(budgetGroup);
			var newBudgetGroup = new BudgetGroup();
			using (mock.Record())
			{
				Expect.Call(view.SelectedModel).Return(budgetGroupModel).Repeat.Once();
				Expect.Call(service.LoadBudgetGroup(budgetGroup)).Return(newBudgetGroup);
			}
			Assert.AreEqual(newBudgetGroup, target.LoadBudgetGroup());
		}
    }

    public static class FakeRepository
    {
        public static BudgetGroupRootModel All()
        {
            var b1 = new BudgetGroupModel();
            b1.DisplayName = "Sales";
            b1.SkillModels = new List<SkillModel>
                                {
                                    new SkillModel {DisplayName = "Direct sales" },
                                    new SkillModel {DisplayName = "Channel sales" }
                                };

            var b2 = new BudgetGroupModel();
            b2.DisplayName = "Support";
            b2.SkillModels = new List<SkillModel>
                                {
                                    new SkillModel { DisplayName = "PC" },
                                    new SkillModel { DisplayName = "Mac", }, 
                                    new SkillModel { DisplayName = "Xbox", }
                                };

            return new BudgetGroupRootModel { BudgetGroups = new List<BudgetGroupModel> { b1, b2 } };
        }
    }

    //Cant mock this shit
    public class FakeSettings : IPortalSettings
    {
        public int NumberOfVisibleGroupBars { get; set; }
        public int SchedulerActionPaneHeight { get; set; }
        public string LastModule {get; set;}
        public int ModuleSelectorPanelHeight { get; set; }
        public int IntradayActionPaneHeight { get; set; }
        public int ForecasterActionPaneHeight { get; set; }
        public int BudgetingActionPaneHeight
        {
            get { return 13; }
            set { throw new NotImplementedException(); }
        }
        public int PayrollActionPaneHeight { get; set; }
        public int PeopleActionPaneHeight { get; set; }
    }
}