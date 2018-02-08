using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public class BudgetGroupNavigatorPresenter
	{
		private readonly IBudgetGroupNavigatorView _view;
		private readonly BudgetGroupNavigatorModel _model;
		private readonly IApplicationInsights _applicationInsights;

		public BudgetGroupNavigatorPresenter(IBudgetGroupNavigatorView view, BudgetGroupNavigatorModel model, IApplicationInsights applicationInsights)
		{
			_view = view;
			_model = model;
			_applicationInsights = applicationInsights;
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
			_applicationInsights.TrackEvent("Opened budget group in Budget Module.");
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