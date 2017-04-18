using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetDaysUpdaterTest : IDisposable
    {
        private MockRepository mocks;
        private IBudgetDayProvider budgetDayProvider;
        private BudgetDaysUpdater target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            budgetDayProvider = mocks.StrictMock<IBudgetDayProvider>();
        }

        [Test]
        public void ShouldThrowExceptionWhenBudgetDayProviderIsNull()
        {
	        Assert.Throws<ArgumentNullException>(() => target = new BudgetDaysUpdater(null));
        }

        [Test]
        public void ShouldStartBatchForProvider()
        {
            using (mocks.Record())
            {
                using (mocks.Ordered())
                {
                    Expect.Call(budgetDayProvider.IsInBatch).Return(false);
                    Expect.Call(budgetDayProvider.IsInBatch = true);
                    Expect.Call(budgetDayProvider.DisableCalculation);
                    Expect.Call(budgetDayProvider.HasUnsavedChanges = true);
                    Expect.Call(budgetDayProvider.Recalculate);
                    Expect.Call(budgetDayProvider.EnableCalculation);
                    Expect.Call(budgetDayProvider.IsInBatch = false);
                }
            }
            using (mocks.Playback())
            {
                target = new BudgetDaysUpdater(budgetDayProvider);
                target.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                target.Dispose();
            }
        }
    }
}
