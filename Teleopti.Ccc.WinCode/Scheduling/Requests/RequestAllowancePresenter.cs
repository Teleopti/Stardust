using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public class RequestAllowancePresenter
    {
        private readonly IRequestAllowanceView _view;
        private readonly IRequestAllowanceModel _model;

        public RequestAllowancePresenter(IRequestAllowanceView view, 
                                        IRequestAllowanceModel model)
        {
            _view = view;
            _model = model;
        }

        public IEnumerable<IAbsence> Absences
        {
            get { return _model.AbsencesInBudgetGroup; }
        }

        public void Initialize(IPersonRequest personRequest, DateOnly defaultDate)
        {
            _model.Initialize(personRequest, defaultDate);
        }

        public void InitializeGridBinding()
        {
            reloadModel(true);
        }

        public IEnumerable<IBudgetGroup> BudgetGroups()
        {
            return _model.BudgetGroups;
        }

        public IBudgetGroup SelectedBudgetGroup()
        {
            return _model.SelectedBudgetGroup;
        }

        public void OnRadioButtonTotalAllowanceCheckChanged(bool value)
        {
            if (Equals(_model.TotalAllowanceSelected, value)) return;
            _model.TotalAllowanceSelected = value;
            reloadModel(true);
        }

        public void OnRadioButtonAllowanceCheckChanged(bool value)
        {
            if (Equals(_model.AllowanceSelected, value)) return;
            _model.AllowanceSelected = value;
            reloadModel(true);
        }

        private void reloadModel(bool reloadAllowance)
        {
            _model.ReloadModel(_model.VisibleWeek, reloadAllowance);
            _view.DataSource = _model.VisibleModel;
        }

        public void OnComboBoxAdvBudgetGroupSelectedIndexChanged(object selectedItem)
        {
            var newBudgetGroup = selectedItem as IBudgetGroup;
            if(newBudgetGroup == null)
                throw new ArgumentNullException("selectedItem");
            if (_model.SelectedBudgetGroup.Equals(newBudgetGroup)) return;
            _model.SelectedBudgetGroup = newBudgetGroup;
            reloadModel(true);
            _view.ReloadAbsenceSection();
        }

        public string WeekName { get { return _model.WeekName; } }

        public void OnRequestAllowanceGridControlNextButtonClicked()
        {
            _model.MoveToNextWeek();
            reloadModel(true);
        }

        public void OnRequestAllowanceGridControlPreviousButtonClicked()
        {
            _model.MoveToPreviousWeek();
            reloadModel(true);
        }

        public void OnRefreshButtonClicked()
        {
            reloadModel(true);
        }
    }
}