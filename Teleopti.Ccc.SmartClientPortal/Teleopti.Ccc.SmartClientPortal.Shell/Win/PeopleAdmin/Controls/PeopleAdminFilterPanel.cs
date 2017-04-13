using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls
{
	public partial class PeopleAdminFilterPanel : BaseUserControl
	{
		private ILifetimeScope _container;
		private PeopleWorksheet _parentControl;
		private FilteredPeopleHolder _filteredPeopleHolder;
		private IApplicationFunction _myApplicationFunction =
			 ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
					  DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

		private IPersonSelectorPresenter _personSelectorPresenter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public PeopleAdminFilterPanel(FilteredPeopleHolder filteredPeopleHolder, PeopleWorksheet peopleWorksheet,
			 ILifetimeScope container)
		{
			_container = container;
			InitializeComponent();
			if (!DesignMode)
			{
				_parentControl = peopleWorksheet;
				SetTexts();
				_filteredPeopleHolder = filteredPeopleHolder;
				_personSelectorPresenter = _container.Resolve<IPersonSelectorPresenter>();
			}
		}

		private void peopleAdminFilterPanelLoad(object sender, EventArgs e)
		{
			_personSelectorPresenter.ApplicationFunction = _myApplicationFunction;
			_personSelectorPresenter.ShowUsers = true;
			_personSelectorPresenter.ShowPersons = true;
			var view = (Control)_personSelectorPresenter.View;
			tableLayoutPanel1.Controls.Add(view);
			view.Dock = DockStyle.Fill;
			view.Margin = new Padding(2, 2, 2, 0);
			var selectorView = _personSelectorPresenter.View;
			selectorView.HideMenu = true;
			selectorView.ShowCheckBoxes = true;
			selectorView.PreselectedPersonIds = new HashSet<Guid>(_filteredPeopleHolder.FilteredPersonIdCollection);
			selectorView.VisiblePersonIds = _filteredPeopleHolder.PersonIdCollection;

			_personSelectorPresenter.LoadTabs();
			_personSelectorPresenter.View.DoFilter += buttonAdvOkClick;
		}

		void reloadPeople()
		{
			_filteredPeopleHolder.SelectedDate = _personSelectorPresenter.SelectedDate;

			_filteredPeopleHolder.ReassociateSelectedPeopleWithNewUow(_personSelectorPresenter.CheckedPersonGuids);
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			doFilter();
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			cancelFilter();
		}

		private void doFilter()
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				reloadPeople();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
				_parentControl.FormKill();
				return;
			}

			_parentControl.RefreshView(_filteredPeopleHolder);

			Cursor.Current = Cursors.Default;
		}

		private void cancelFilter()
		{
			Hide();
		}

		private void tableLayoutPanel1_VisibleChanged(object sender, EventArgs e)
		{
			if (this.Visible)
				this.Focus();
		}
	}
}
