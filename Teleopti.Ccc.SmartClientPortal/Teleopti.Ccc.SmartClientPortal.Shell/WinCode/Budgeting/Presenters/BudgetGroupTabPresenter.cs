using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public class BudgetGroupTabPresenter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BudgetGroupTabPresenter));
        private readonly IBudgetGroupTabView _view;
        private readonly IBudgetDayProvider _budgetDayProvider;
        private readonly IBudgetGroupDataService _budgetGroupDataService;
        private readonly ILoadForecastedHoursCommand _loadForecastedHoursCommand;
        private readonly ILoadStaffEmployedCommand _loadStaffEmployedCommand;

        public BudgetGroupTabPresenter(IBudgetGroupTabView view, IBudgetDayProvider budgetDayProvider,
                                           IBudgetGroupDataService budgetGroupDataService,
                                           ILoadForecastedHoursCommand loadForecastedHoursCommand,
                                           ILoadStaffEmployedCommand loadStaffEmployedCommand)
        {
            _view = view;
            _budgetDayProvider = budgetDayProvider;
            _budgetGroupDataService = budgetGroupDataService;
            _loadForecastedHoursCommand = loadForecastedHoursCommand;
            _loadStaffEmployedCommand = loadStaffEmployedCommand;
        }

        public void LoadDayModels()
        {
            _budgetDayProvider.DayModels();
        }

        public void BeginUpdate()
        {
            _budgetDayProvider.IsInBatch = true;
            _budgetDayProvider.DisableCalculation();
        }

        public void EndUpdate()
        {
            _budgetDayProvider.IsInBatch = false;
            _budgetDayProvider.EnableCalculation();
            _budgetDayProvider.Recalculate();
        }

        public void Save(IUnitOfWork unitOfWork)
        {
            try
            {
                _budgetGroupDataService.Save(_budgetDayProvider);
                unitOfWork.PersistAll();
                _budgetDayProvider.HasUnsavedChanges = false;
            }
            catch (OptimisticLockException optimisticLockException)
            {
                Logger.Warn("OptimisticLockException when saving budgetdays.", optimisticLockException);
                _view.NotifyBudgetDaysUpdatedByOthers();
            }
        }

        private void CheckToClose(ICancelEventModel cancelEventModel)
        {
            switch (_view.AskToCommitChanges())
            {
                case DialogResult.Yes:
                    _view.OnSave(null);
                    break;
                case DialogResult.No:
                    break;
                case DialogResult.Cancel:
                    cancelEventModel.CancelEvent = true;
                    break;
            }
        }

        public void Close(ICancelEventModel cancelEventModel)
        {
            if (_budgetDayProvider.HasUnsavedChanges) CheckToClose(cancelEventModel);
        }

        public void LoadStaff()
        {
            _loadStaffEmployedCommand.Execute();
        }

        public void LoadForecastedHours()
        {
            _loadForecastedHoursCommand.Execute();
        }
    }
}