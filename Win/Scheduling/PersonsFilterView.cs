using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class PersonsFilterView : BaseDialogForm, IReportPersonsSelectionView
	{
		private readonly IPersonSelectorPresenter _personSelectorPresenter;
		private PersonsFilterView(){}
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();

		public PersonsFilterView(DateOnlyPeriod selectedPeriod, IEnumerable<Guid> selectedPersonGuids, IComponentContext componentContext,
			IApplicationFunction applicationFunction, string selectedGroupPage, IEnumerable<Guid> visiblePersonGuids)
		{
			InitializeComponent();

			_personSelectorPresenter =
				componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope().Resolve<IPersonSelectorPresenter>();
			_personSelectorPresenter.ApplicationFunction = applicationFunction;
			var view = (Control)_personSelectorPresenter.View;
			panel1.Controls.Add(view);
			view.Dock = DockStyle.Fill;

			var selectorView = _personSelectorPresenter.View;
			selectorView.SelectedPeriod = selectedPeriod;
			_personSelectorPresenter.ShowPersons = true;
			_personSelectorPresenter.ShowUsers = false;
			selectorView.PreselectedPersonIds = selectedPersonGuids;
			selectorView.VisiblePersonIds = visiblePersonGuids;
			selectorView.ShowCheckBoxes = true;
			selectorView.KeepInteractiveOnDuringLoad = true;

			selectorView.ShowDateSelection = false;
			selectorView.HideMenu = true;
			if (_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(_personSelectorPresenter.LoadTabs))
			{
				_personSelectorPresenter.SetSelectedTab(selectedGroupPage);
			}
		   
			SetTexts();
			
		}

		public HashSet<Guid> SelectedAgentGuids()
		{
			return _personSelectorPresenter.CheckedPersonGuids;
		}

		public string SelectedGroupPageKey
		{
			get { return _personSelectorPresenter.SelectedGroupPageKey(); }
		}
	}
}
