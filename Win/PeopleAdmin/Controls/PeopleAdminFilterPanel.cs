using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using System.Windows.Forms;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls
{
    public partial class PeopleAdminFilterPanel : BaseUserControl
    {
        private readonly ILifetimeScope _container;
        private PeopleWorksheet _parentControl;
        private FilteredPeopleHolder _filteredPeopleHolder;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                   DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

        private readonly WorksheetStateHolder _worksheetStateHolder;
        private IPersonSelectorPresenter _personSelectorPresenter;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public PeopleAdminFilterPanel(FilteredPeopleHolder filteredPeopleHolder, WorksheetStateHolder worksheetStateHolder, PeopleWorksheet peopleWorksheet,
            ILifetimeScope container)
        {
            _container = container;
            InitializeComponent();
            if (!DesignMode)
            {
                _parentControl = peopleWorksheet;
                _worksheetStateHolder = worksheetStateHolder;
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

            var selectorView = _personSelectorPresenter.View;
            selectorView.HideMenu = true;

            _personSelectorPresenter.LoadTabs();
        }

        void ReloadPeople()
        {
            _filteredPeopleHolder.GetUnitOfWork.Dispose();

            var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();

            var accounts = new PersonAbsenceAccountRepository(uow).LoadAllAccounts();

            var currentScenario = _container.Resolve<ICurrentScenario>();
            ITraceableRefreshService service = new TraceableRefreshService(currentScenario ,new ScheduleRepository(uow));

            _filteredPeopleHolder.SetState(service, uow, accounts);
            _filteredPeopleHolder.SelectedDate = _personSelectorPresenter.SelectedDate;
                 
            _filteredPeopleHolder.LoadIt();
            _filteredPeopleHolder.SetRotationCollection(_worksheetStateHolder.AllRotations);
            _filteredPeopleHolder.SetAvailabilityCollection(_worksheetStateHolder.AllAvailabilities);

            _filteredPeopleHolder.ReassociateSelectedPeopleWithNewUow(_personSelectorPresenter.SelectedPersonGuids);
        }

        private void ButtonAdvOkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                ReloadPeople();
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

        private void ButtonAdvCancelClick(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
