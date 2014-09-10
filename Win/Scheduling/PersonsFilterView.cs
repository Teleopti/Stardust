using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
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
		private ICollection<IPerson> _selectedPersons;
		private FilterMultiplePersons _filterMultiplePersons;
		private bool _getTheGuidsFromAdvanceFilter = false;

		public PersonsFilterView(DateOnlyPeriod selectedPeriod, IDictionary<Guid, IPerson> selectedPersons, IComponentContext componentContext, IApplicationFunction applicationFunction, string selectedGroupPage, IEnumerable<Guid> visiblePersonGuids, bool isAdvancedEnabled = false)
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
			selectorView.PreselectedPersonIds = selectedPersons.Keys;
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
			buttonAdvance.Visible = isAdvancedEnabled;
			_selectedPersons = selectedPersons.Values ;

		}

		public HashSet<Guid> SelectedAgentGuids()
		{
			if (_getTheGuidsFromAdvanceFilter)
			{
				_getTheGuidsFromAdvanceFilter = false;
				return _filterMultiplePersons.SelectedPersonGuids();
			}
				
			return _personSelectorPresenter.CheckedPersonGuids;
		}

		public string SelectedGroupPageKey
		{
			get { return _personSelectorPresenter.SelectedGroupPageKey(); }
		}

		private void buttonAdvance_Click(object sender, EventArgs e)
		{
			_getTheGuidsFromAdvanceFilter = false;
			using (_filterMultiplePersons = new FilterMultiplePersons())
			{
				_filterMultiplePersons.SetSearchablePersons(_selectedPersons );
				_filterMultiplePersons.ShowDialog(this);

				if (_filterMultiplePersons.DialogResult == DialogResult.OK)
				{
					_getTheGuidsFromAdvanceFilter = true;
				}
			}
			DialogResult = DialogResult.OK;
		}
	}
}
