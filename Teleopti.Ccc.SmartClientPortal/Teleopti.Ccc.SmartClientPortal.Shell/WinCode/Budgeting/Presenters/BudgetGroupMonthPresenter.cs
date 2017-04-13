using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public class BudgetGroupMonthPresenter
    {
        private readonly IBudgetGroupMonthView _view;
        private readonly BudgetGroupMonthModel _model;

        public BudgetGroupMonthPresenter(IBudgetGroupMonthView view, BudgetGroupMonthModel model)
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

        public void UpdateShrinkageRow(ICustomShrinkage customShrinkage)
        {
            _view.AddShrinkageRow(customShrinkage);
        }

        public void UpdateEfficiencyShrinkageRow(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
        {
            _view.AddEfficiencyShrinkageRow(customEfficiencyShrinkage);
        }

        public void UpdateBudgetGroup(IBudgetGroup budgetGroup)
        {
            _model.MainModel.BudgetGroup = budgetGroup;
        }
    }
}