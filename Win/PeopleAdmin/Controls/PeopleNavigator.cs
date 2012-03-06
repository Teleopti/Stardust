using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using System.Windows.Forms;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.PeopleAdmin.Views;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WpfControls.Common.Interop;
using Teleopti.Ccc.WpfControls.Controls.Messaging;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Win.PeopleAdmin.Controls
{
    public partial class PeopleNavigator : BaseUserControl, IPeopleNavigator
    {
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

        private readonly PortalSettings _portalSettings;
        private readonly IComponentContext _componentContext;
        private readonly IComponentContext _container;
        private IPersonSelectorPresenter _selectorPresenter;
        private IPeopleNavigatorPresenter _myPresenter;
        private readonly IEventAggregator _localEventAggregator;
        private readonly IEventAggregator _globalEventAggregator;

        public PeopleNavigator(PortalSettings portalSettings, IComponentContext componentContext)
            : this()
        {
            _portalSettings = portalSettings;
            _componentContext = componentContext;

            var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
            _container = lifetimeScope.BeginLifetimeScope();
            _localEventAggregator = _container.Resolve<IEventAggregator>();
            _globalEventAggregator = _container.Resolve<IEventAggregatorLocator>().GlobalAggregator();

			SetTexts();
        }

        public PeopleNavigator()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                toolStripButtonOpen.Click += onOpenPeople;
                toolStripButtonAddPerson.Click += onAddNewPeople;

                _selectorPresenter = _container.Resolve<IPersonSelectorPresenter>();
                _selectorPresenter.ApplicationFunction = _myApplicationFunction;
                _selectorPresenter.ShowPersons = true;
                _selectorPresenter.ShowUsers = true;
                _selectorPresenter.ShowFind = true;

                var view = (Control) _selectorPresenter.View;
                splitContainerAdv1.Panel1.Controls.Add(view);
                view.Dock = DockStyle.Fill;
                _selectorPresenter.LoadTabs();

                _myPresenter = _container.Resolve<IPeopleNavigatorPresenter>();
                _myPresenter.Init(this);

                splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height - _portalSettings.PeopleActionPaneHeight;
                _container.Resolve<IEventAggregator>().GetEvent<SelectedNodesChanged>().Publish("");
            }
        }

        void onOpenPeople(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            openPeople(_selectorPresenter.SelectedPersonGuids);
            Cursor.Current = Cursors.Default;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void openPeople(IEnumerable<Guid> selectedGuids)
        {
            if (!selectedGuids.IsEmpty())
            {
                try
                {

                    IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();
                    //var accounts = new PersonAbsenceAccountRepository(uow).LoadAllAccounts();

                    var rep = new PersonRepository(uow);
                    var foundPeople = rep.FindPeople(selectedGuids).ToList();
                    var accounts = new PersonAbsenceAccountRepository(uow).FindByUsers(foundPeople);

                    ITraceableRefreshService service =
                        new TraceableRefreshService(new ScenarioRepository(uow).LoadDefaultScenario(), new RepositoryFactory());

                    var filteredPeopleHolder = new FilteredPeopleHolder(service, accounts)
                    {
                        SelectedDate = _selectorPresenter.SelectedDate, UnitOfWork = uow
                    };

                    var state = new WorksheetStateHolder(service);

                    loadPeopleGeneralViewAndInitialReferences(state, uow);

                    filteredPeopleHolder.LoadIt();

                    filteredPeopleHolder.SetRotationCollection(state.AllRotations);
                    filteredPeopleHolder.SetAvailabilityCollection(state.AllAvailabilities);

                    //filteredPeopleHolder.ReassociateSelectedPeopleWithNewUow(selectedGuids);
                    filteredPeopleHolder.ReassociateSelectedPeopleWithNewUowOpenPeople(foundPeople);

                    //Open ppladmin form.
                    openPeopleAdmin(state, filteredPeopleHolder);
                }
                catch (DataSourceException exception)
                {
                    ShowDataSourceException(exception, Resources.PersonAdmin);
                }
            }
        }

        void onAddNewPeople(object sender, EventArgs e)
        {
            OpenNewPerson();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void openPeopleAdmin(WorksheetStateHolder stateHolder, FilteredPeopleHolder filteredPeopleHolder)
        {
           new PeopleWorksheet(stateHolder, filteredPeopleHolder, _globalEventAggregator, _componentContext).Show();   
        }

        private void tsAddGroupPageClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<AddGroupPageClicked>().Publish("");
        }

        private void tsEditGroupPageClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<ModifyGroupPageClicked>().Publish("");
        }

        private void tsRenameGroupPageClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<RenameGroupPageClicked>().Publish("");
        }

        private void tsDeleteGroupPageClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<DeleteGroupPageClicked>().Publish("");
        }

        private static void loadPeopleGeneralViewAndInitialReferences(WorksheetStateHolder stateHolder, IUnitOfWork uow)
        {
            // Load all culture information (to show on the combo boxes).
            stateHolder.LoadCultureInfo();

            //Loads all rotations
            stateHolder.LoadAllRotations(uow);

            //Loads all availabilities
            stateHolder.LoadAllAvailabilities(uow);

            //Loads all workflow control sets
            stateHolder.LoadAllWorkflowControlSets(uow);

            stateHolder.LoadShiftCategories(uow);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void OpenNewPerson()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();
                var accounts = new PersonAbsenceAccountRepository(uow).LoadAllAccounts();
                IScenario defaultScenario = new ScenarioRepository(uow).LoadDefaultScenario();
                ITraceableRefreshService cacheServiceForPersonAccounts = new TraceableRefreshService(defaultScenario, new RepositoryFactory());
                var state = new WorksheetStateHolder(cacheServiceForPersonAccounts);
                var filteredPeopleHolder = new FilteredPeopleHolder(cacheServiceForPersonAccounts, accounts)
                {
                    SelectedDate = DateOnly.Today,
                    UnitOfWork = uow
                };

                //Load the people general view & initial references
                loadPeopleGeneralViewAndInitialReferences(state, uow);
                filteredPeopleHolder.LoadIt();
                
                state.AddAndSavePerson(0, filteredPeopleHolder);
                openPeopleAdmin(state, filteredPeopleHolder);
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceException(dataSourceException,Resources.PersonAdmin);
            }
            
            Cursor.Current = Cursors.Default;
        }

        public void OpenPeopleAdmin()
        {
            toolStripButtonOpen.PerformClick();
        }

        public void AddNew()
        {
            OpenNewPerson();
        }

        public bool SendMessageVisible
        {
            get { return toolStripButtonSendInstantMessage.Visible; }
            set { toolStripButtonSendInstantMessage.Visible = value; }
        }

        public void Open()
        {
            OpenPeopleAdmin();
        }

        public void ShowDataSourceException(DataSourceException dataSourceException, string dialogTitle)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    dialogTitle,
                                                                    Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        public bool AddGroupPageEnabled
        {
            get { return tsAddGroupPage.Enabled; }
            set { tsAddGroupPage.Enabled = value; }
        }

        public bool RenameGroupPageEnabled
        {
            get { return tsRenameGroupPage.Enabled; }
            set { tsRenameGroupPage.Enabled = value; }
        }

        public bool DeleteGroupPageEnabled
        {
            get { return tsDeleteGroupPage.Enabled; }
            set { tsDeleteGroupPage.Enabled = value; }
        }

        public bool ModifyGroupPageEnabled
        {
            get { return tsEditGroupPage.Enabled; }
            set { tsEditGroupPage.Enabled = value; }
        }

        public bool OpenEnabled
        {
            get { return toolStripButtonOpen.Enabled; }
            set
            {
                toolStripButtonOpen.Enabled = value;
            }
        }

        private void toolStripButtonSendInstantMessageClick(object sender, EventArgs e)
        {
            // we have to load real persons here
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    var rep = new RepositoryFactory().CreatePersonRepository(uow);
                    var persons = rep.FindPeople(_selectorPresenter.SelectedPersonGuids);
                    var messageForm = new PushMessageForm(persons);
                    messageForm.ShowFromWinForms(false);
                }
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.PersonAdmin,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
            }
            
        }

        private void splitContainerAdv1SplitterMoved(object sender, Syncfusion.Windows.Forms.Tools.Events.SplitterMoveEventArgs e)
        {
            _portalSettings.PeopleActionPaneHeight = splitContainerAdv1.Height - splitContainerAdv1.SplitterDistance;
        }

        private void toolStripButtonSearchClick(object sender, EventArgs e)
        {
           FindPeople();
        }

        public void FindPeople()
        {
            Cursor.Current = Cursors.WaitCursor;
            {
                IEnumerable<Guid> selectedGuids = new List<Guid>();

                using (var findForm = new PersonFinderView())
                {
                    findForm.ShowDialog(this);
                    if (findForm.DialogResult == DialogResult.OK)
                    {
                        selectedGuids = findForm.SelectedPersonGuids;
                    }
                }
                openPeople(selectedGuids);

            }
            Cursor.Current = Cursors.Default;
        }

        private void toolStripRefreshClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<RefreshGroupPageClicked>().Publish("");
        }
    }
}
