using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class BudgetGroupWeekModel
    {
    	private readonly BudgetGroupMainModel _mainModel;
        private readonly IBudgetDayProvider _budgetDayProvider;
        private readonly IBudgetPermissionService _budgetPermissionService;
        private IList<BudgetGroupWeekDetailModel> _dataSource = new List<BudgetGroupWeekDetailModel>();

		public BudgetGroupWeekModel(BudgetGroupMainModel mainModel, IBudgetDayProvider budgetDayProvider, IBudgetPermissionService budgetPermissionService)
        {
			_mainModel = mainModel;
            _budgetDayProvider = budgetDayProvider;
		    _budgetPermissionService = budgetPermissionService;
        }

    	public BudgetGroupMainModel MainModel
    	{
    		get { return _mainModel; }
    	}

    	public IList<BudgetGroupWeekDetailModel> DataSource
        {
            get { return _dataSource; }
        }

        public void LoadModel()
        {
            _dataSource = LoadModels();
        }

        private IList<BudgetGroupWeekDetailModel> LoadModels()
        {
            var days = _budgetDayProvider.VisibleDayModels();
            var weeksFilter = new BudgetGroupWeekFilter(days, CultureInfo.CurrentCulture);
            var weeks = weeksFilter.Filter();

            foreach (var week in weeks)
            {
                _dataSource.Add(new BudgetGroupWeekDetailModel(week,_budgetDayProvider, _budgetPermissionService));
            }

            return _dataSource;
        }
    }
}