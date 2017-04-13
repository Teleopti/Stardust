using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public class BudgetGroupNavigatorPresenter
	{
		private readonly IBudgetGroupNavigatorView _view;
		private readonly BudgetGroupNavigatorModel _model;

		public BudgetGroupNavigatorPresenter(IBudgetGroupNavigatorView view, BudgetGroupNavigatorModel model)
		{
			_view = view;
			_model = model;
		}

		public void Initialize()
		{
			InitializeModel();
			_view.BudgetGroupRootModel = _model.BudgetRootModel;
			_view.BudgetingActionPaneHeight = _model.BudgetingActionPaneHeight;
		}

		private void InitializeModel()
		{
			_model.BudgetRootModel = _model.DataService.GetBudgetRootModels();
		}

		public IEntity GetSelectedEntity()
		{
			return _view.SelectedModel.ContainedEntity;
		}

		public void DeleteBudgetGroup()
		{
			_model.DataService.DeleteBudgetGroup((IBudgetGroup)_view.SelectedModel.ContainedEntity);
		}

		public IBudgetGroup LoadBudgetGroup()
		{
			return _model.DataService.LoadBudgetGroup((IBudgetGroup)_view.SelectedModel.ContainedEntity);
		}
	}

	public interface IBudgetNavigatorDataService
	{
		BudgetGroupRootModel GetBudgetRootModels();
		void DeleteBudgetGroup(IBudgetGroup budgetGroup);
		IBudgetGroup LoadBudgetGroup(IBudgetGroup budgetGroup);
	}
}