using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
	public class BudgetGroupDayModel
	{
		private readonly BudgetGroupMainModel _mainModel;
		private readonly IBudgetDayProvider _budgetDayProvider;
		private IList<IBudgetGroupDayDetailModel> _dataSource = new List<IBudgetGroupDayDetailModel>();

		public BudgetGroupDayModel(BudgetGroupMainModel mainModel, IBudgetDayProvider budgetDayProvider)
		{
			_mainModel = mainModel;
			_budgetDayProvider = budgetDayProvider;
		}

		public IList<IBudgetGroupDayDetailModel> DataSource
		{
			get { return _dataSource; }
		}

		public void LoadModel()
		{
			_dataSource = _budgetDayProvider.VisibleDayModels();
		}

		public BudgetGroupMainModel MainModel
		{
            get { return _mainModel; }
		}

		public void Recalculate()
		{
			_budgetDayProvider.Recalculate();
		}
	}
}