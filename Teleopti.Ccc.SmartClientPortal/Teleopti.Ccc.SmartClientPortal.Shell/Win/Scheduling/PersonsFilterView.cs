using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public interface IScheduleAgentFilter
	{
		void SetCurrentFilter(IDictionary<Guid, IPerson> filteredPersons);
	}

	public partial class PersonsFilterView : BaseDialogForm, IReportPersonsSelectionView, IScheduleAgentFilter
	{
		private readonly IComponentContext _componentContext;
		private readonly IPersonSelectorPresenter _personSelectorPresenter;
		private PersonsFilterView(){}
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();
		private readonly ICollection<IPerson> _selectedPersons;
		private FilterMultiplePersons _filterMultiplePersons;
		private bool _getTheGuidsFromAdvanceFilter = false;
		private readonly IPersonSelectorView _selectorView;

		public PersonsFilterView(DateOnlyPeriod selectedPeriod, IDictionary<Guid, IPerson> selectedPersons, IComponentContext componentContext, IApplicationFunction applicationFunction, string selectedGroupPage, IEnumerable<Guid> visiblePersonGuids, bool isAdvanced)
		{
			_componentContext = componentContext;
			InitializeComponent();

			var lifeTimeScope = componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope();
			_personSelectorPresenter = lifeTimeScope.Resolve<IPersonSelectorPresenter>();

			_selectorView = _personSelectorPresenter.View;
			_personSelectorPresenter.ApplicationFunction = applicationFunction;
			var view = (Control)_personSelectorPresenter.View;
			panel1.Controls.Add(view);
			view.Dock = DockStyle.Fill;

			_selectorView.SelectedPeriod = selectedPeriod;
			_personSelectorPresenter.ShowPersons = true;
			_personSelectorPresenter.ShowUsers = false;
			_selectorView.PreselectedPersonIds = new HashSet<Guid>(selectedPersons.Keys);
			_selectorView.VisiblePersonIds = visiblePersonGuids;
			_selectorView.ShowCheckBoxes = true;

			_selectorView.ShowDateSelection = false;
			_selectorView.HideMenu = true;
			if (_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(_personSelectorPresenter.LoadTabs))
			{
				_personSelectorPresenter.SetSelectedTab(selectedGroupPage);
			}

			SetTexts();
			buttonAdvance.Visible = isAdvanced;
			_selectedPersons = selectedPersons.Values;
			_selectorView.DoFilter += selectorViewDoFilter;

		}

		void selectorViewDoFilter(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		public void SetCurrentFilter(IDictionary<Guid, IPerson> filteredPersons)
		{
			var filteredGuids = new HashSet<Guid>(filteredPersons.Keys);
			_personSelectorPresenter.SetSelectedPersonGuids(filteredGuids);
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
				_filterMultiplePersons.SetSearchablePersons(_selectedPersons, _componentContext.Resolve<ITenantLogonDataManagerClient>(), _componentContext.Resolve<AdvancedAgentsFilter>());
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
