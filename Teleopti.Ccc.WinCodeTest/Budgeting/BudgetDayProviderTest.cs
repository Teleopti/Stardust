using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetDayProviderTest
    {
        private MockRepository mocks;
        private IBudgetGroupDataService budgetGroupDataService;
        private IVisibleBudgetDays visibleBudgetDays;
        private IBudgetDayProvider target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            budgetGroupDataService = mocks.StrictMock<IBudgetGroupDataService>();
            visibleBudgetDays = mocks.StrictMock<IVisibleBudgetDays>();
            target = new BudgetDayProvider(budgetGroupDataService, visibleBudgetDays);
        }

        [Test]
        public void VerifyBatchUpdater()
        {
            var budgetDayModel = mocks.StrictMock<IBudgetGroupDayDetailModel>();
            var listOfBudgetDayModels = new List<IBudgetGroupDayDetailModel>{budgetDayModel};

            using (mocks.Record())
            {
                Expect.Call(budgetGroupDataService.FindAndCreate()).Return(listOfBudgetDayModels);
                Expect.Call(() => budgetDayModel.Invalidate += OnBudgetDayModelOnInvalidate).IgnoreArguments();
                Expect.Call(()=>budgetGroupDataService.Recalculate(listOfBudgetDayModels));
                Expect.Call(budgetDayModel.EnablePropertyChangedInvocation);
                Expect.Call(budgetDayModel.DisablePropertyChangedInvocation);
            }
            using (mocks.Playback())
            {
                using(target.BatchUpdater())
                {}
                Assert.IsTrue(target.HasUnsavedChanges);
            }
        }

        [Test]
        public void ShouldRecalculateAndSetUnsavedChangesWhenInvalidatingOneDay()
        {
            var budgetDayModel = mocks.StrictMock<IBudgetGroupDayDetailModel>();
            var listOfBudgetDayModels = new List<IBudgetGroupDayDetailModel> { budgetDayModel };

            using (mocks.Record())
            {
                Expect.Call(budgetGroupDataService.FindAndCreate()).Return(listOfBudgetDayModels);
                Expect.Call(() => budgetDayModel.Invalidate += OnBudgetDayModelOnInvalidate).IgnoreArguments();
                Expect.Call(() => budgetGroupDataService.Recalculate(listOfBudgetDayModels));
                Expect.Call(visibleBudgetDays.Filter(listOfBudgetDayModels)).Return(listOfBudgetDayModels);
            }
            using (mocks.Playback())
            {
                var models = target.VisibleDayModels();
                budgetDayModel.Raise(b => b.Invalidate += null, null, null);
                Assert.IsTrue(target.HasUnsavedChanges);
                Assert.AreEqual(1,models.Count);
            }
        }

        private static void OnBudgetDayModelOnInvalidate(object sender, CustomEventArgs<IBudgetGroupDayDetailModel> e)
        {
        }
    }
}
