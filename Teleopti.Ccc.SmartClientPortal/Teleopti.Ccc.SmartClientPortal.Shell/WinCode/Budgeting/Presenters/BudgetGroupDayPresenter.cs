using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public class BudgetGroupDayPresenter
	{
		private readonly IBudgetGroupDayView _view;
		private readonly BudgetGroupDayModel _model;
		
		public BudgetGroupDayPresenter(IBudgetGroupDayView view, BudgetGroupDayModel model)
		{
			_view = view;
			_model = model;
		}

		public void Initialize()
		{
			InitializeShrinkages();
			InitializeEfficiencyShrinkages();
			InitializeDataBinding();
		}

		private void InitializeDataBinding()
		{
			_model.LoadModel();
		    _model.Recalculate();
			_view.DataSource = _model.DataSource;
		}

	    public void InitializeShrinkages()
		{
			foreach (var customShrinkage in _model.MainModel.BudgetGroup.CustomShrinkages)
			{
				_view.AddShrinkageRow(customShrinkage);
			}
		}

        public void InitializeEfficiencyShrinkages()
		{
			foreach (var customEfficiencyShrinkage in _model.MainModel.BudgetGroup.CustomEfficiencyShrinkages)
			{
				_view.AddEfficiencyShrinkageRow(customEfficiencyShrinkage);
			}
		}

		public void AddShrinkageRow(ICustomShrinkage customShrinkage)
		{
			_view.AddShrinkageRow(customShrinkage);
		}

		public void AddEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
		{
			_view.AddEfficiencyShrinkageRow(customEfficiencyShrinkage);
		}

        public void RecalculateAll()
        {
            _model.Recalculate();
        }

        public void UpdateBudgetGroup(IBudgetGroup budgetGroup)
        {
            _model.MainModel.BudgetGroup = budgetGroup;
            _model.DataSource.ForEach(m => m.BudgetDay.BudgetGroup = budgetGroup);
        }
	}

	public interface IBudgetGroupDataService
	{
		IList<IBudgetGroupDayDetailModel> FindAndCreate();
        void Save(IBudgetDayProvider budgetDayProvider);
	    void Recalculate(IList<IBudgetGroupDayDetailModel> models);
	}
}
