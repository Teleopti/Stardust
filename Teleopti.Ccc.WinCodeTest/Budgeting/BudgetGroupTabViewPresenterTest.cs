using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetGroupTabViewPresenterTest
    {
        private MockRepository mocks;
        private IBudgetGroupTabView view;
        private IBudgetDayProvider budgetDayProvider;
        private BudgetGroupTabPresenter target;
        private IBudgetGroupDataService budgetGroupDataService;
        private ILoadForecastedHoursCommand loadForecastedHoursCommand;
        private ILoadStaffEmployedCommand loadStaffEmployedCommand;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            view = mocks.StrictMock<IBudgetGroupTabView>();
            budgetDayProvider = mocks.StrictMock<IBudgetDayProvider>();
            budgetGroupDataService = mocks.StrictMock<IBudgetGroupDataService>();
            loadForecastedHoursCommand = mocks.StrictMock<ILoadForecastedHoursCommand>();
            loadStaffEmployedCommand = mocks.StrictMock<ILoadStaffEmployedCommand>();
            target = new BudgetGroupTabPresenter(view, budgetDayProvider, budgetGroupDataService,
                                                     loadForecastedHoursCommand, loadStaffEmployedCommand);
        }

        [Test]
        public void ShouldLoadDayModels()
        {
            using(mocks.Record())
            {
                Expect.Call(budgetDayProvider.DayModels()).Return(new List<IBudgetGroupDayDetailModel>());
            }
            using (mocks.Playback())
            {
                target.LoadDayModels();
            }
        }

        [Test]
        public void ShouldDisableCalculationOnBeginUpdate()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetDayProvider.IsInBatch = true);
                Expect.Call(budgetDayProvider.DisableCalculation);
            }
            using (mocks.Playback())
            {
                target.BeginUpdate();
            }
        }

        [Test]
        public void ShouldEnableCalculationOnEndUpdate()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetDayProvider.IsInBatch = false);
                Expect.Call(budgetDayProvider.EnableCalculation);
                Expect.Call(budgetDayProvider.Recalculate);
            }
            using (mocks.Playback())
            {
                target.EndUpdate();
            }
        }

        [Test]
        public void ShouldSaveBudgetDaysUsingTheSaveService()
        {
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(()=>budgetGroupDataService.Save(budgetDayProvider));
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
                Expect.Call(budgetDayProvider.HasUnsavedChanges).PropertyBehavior();
            }
            using (mocks.Playback())
            {
                budgetDayProvider.HasUnsavedChanges = true;
                target.Save(unitOfWork);
                Assert.IsFalse(budgetDayProvider.HasUnsavedChanges);
            }
        }

        [Test]
        public void ShouldNotifyUserOfOptimisticLockException()
        {
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            using (mocks.Record())
            {
                Expect.Call(() => budgetGroupDataService.Save(budgetDayProvider)).Throw(new OptimisticLockException());
                Expect.Call(view.NotifyBudgetDaysUpdatedByOthers);
            }
            using (mocks.Playback())
            {
                target.Save(unitOfWork);
            }
        }

        [Test]
        public void ShouldNotShowClosingQuestionWhenNoChangesAvailable()
        {
            var cancelEventModel = mocks.StrictMock<ICancelEventModel>();
            budgetDayProvider.HasUnsavedChanges = false;
            target.Close(cancelEventModel);
        }

        [Test]
        public void ShouldSaveAfterYesOfClosingQuestion()
        {
            var cancelEventModel = mocks.StrictMock<ICancelEventModel>();
            using (mocks.Record())
            {
                Expect.Call(view.AskToCommitChanges()).Return(DialogResult.Yes);
                Expect.Call(() => view.OnSave(null)).IgnoreArguments();
                Expect.Call(budgetDayProvider.HasUnsavedChanges).PropertyBehavior();
            }
            using (mocks.Playback())
            {
                budgetDayProvider.HasUnsavedChanges = true;
                target.Close(cancelEventModel);
            }
        }

        [Test]
        public void ShouldNotSaveAfterNoOfClosingQuestion()
        {
            var cancelEventModel = mocks.StrictMock<ICancelEventModel>();
            using (mocks.Record())
            {
                Expect.Call(view.AskToCommitChanges()).Return(DialogResult.No);
                Expect.Call(budgetDayProvider.HasUnsavedChanges).PropertyBehavior();
            }
            using (mocks.Playback())
            {
                budgetDayProvider.HasUnsavedChanges = true;
                target.Close(cancelEventModel);
            }
        }

        [Test]
        public void ShouldCancelAfterCancelOfClosingQuestion()
        {
            var cancelEventModel = mocks.StrictMock<ICancelEventModel>();
            using (mocks.Record())
            {
                Expect.Call(view.AskToCommitChanges()).Return(DialogResult.Cancel);
                Expect.Call(cancelEventModel.CancelEvent = true);
                Expect.Call(budgetDayProvider.HasUnsavedChanges).PropertyBehavior();
            }
            using (mocks.Playback())
            {
                budgetDayProvider.HasUnsavedChanges = true;
                target.Close(cancelEventModel);
            }
        }

        [Test]
        public void ShouldExecuteCommandLoadForecastedHours()
        {
            using (mocks.Record())
            {
                Expect.Call(loadForecastedHoursCommand.Execute);
            }
            using (mocks.Playback())
            {
                target.LoadForecastedHours();
            }
        }

        [Test]
        public void ShouldExecuteCommandLoadStaffEmployed()
        {
            using (mocks.Record())
            {
                Expect.Call(loadStaffEmployedCommand.Execute);
            }
            using (mocks.Playback())
            {
                target.LoadStaff();
            }
        }
    }
}
