using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class BudgetGroupMonthModel
    {
        private readonly BudgetGroupMainModel _mainModel;
        private readonly IBudgetDayProvider _budgetDayProvider;
        private readonly IBudgetPermissionService _budgetPermissionService;
        private IList<BudgetGroupMonthDetailModel> _dataSource = new List<BudgetGroupMonthDetailModel>();

        public BudgetGroupMonthModel(BudgetGroupMainModel mainModel, IBudgetDayProvider budgetDayProvider, IBudgetPermissionService budgetPermissionService)
        {
            _mainModel = mainModel;
            _budgetDayProvider = budgetDayProvider;
            _budgetPermissionService = budgetPermissionService;
        }

        public IList<BudgetGroupMonthDetailModel> DataSource
        {
            get { return _dataSource; }
        }

        public BudgetGroupMainModel MainModel
        {
            get { return _mainModel; }
        }

        public void LoadModel()
        {
            _dataSource = LoadModels();
        }

        private IList<BudgetGroupMonthDetailModel> LoadModels()
        {
        	var days = _budgetDayProvider.VisibleDayModels();
            var monthFilter = new BudgetGroupMonthFilter(days, CultureInfo.CurrentCulture);
            var months = monthFilter.Filter();

            foreach (var month in months)
            {
                _dataSource.Add(new BudgetGroupMonthDetailModel(month, _budgetDayProvider, _budgetPermissionService));
            }

            return _dataSource;
        }
    }
}