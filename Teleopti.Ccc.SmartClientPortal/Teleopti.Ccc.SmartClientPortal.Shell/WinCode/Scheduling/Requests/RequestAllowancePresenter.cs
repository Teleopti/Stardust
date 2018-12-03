using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
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

		public void Initialize(IBudgetGroup budgetGroup, DateOnly defaultDate)
		{
			_model.Initialize(budgetGroup, defaultDate);
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

		public void OnRadioButtonFullAllowanceCheckChanged(bool value)
		{
			if (Equals(_model.FullAllowanceSelected, value)) return;
			_model.FullAllowanceSelected = value;
			reloadModel(true);
		}

		public void OnRadioButtonShrinkedAllowanceCheckChanged(bool value)
		{
			if (Equals(_model.ShrinkedAllowanceSelected, value)) return;
			_model.ShrinkedAllowanceSelected = value;
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
			if (newBudgetGroup == null)
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