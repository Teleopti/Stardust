using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Win.Common;
using System.Windows.Forms;
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
    	private readonly IPersonRepository _personRepository;
    	private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IGracefulDataSourceExceptionHandler _gracefulDataSourceExceptionHandler;
    	private readonly IComponentContext _container;
        private IPersonSelectorPresenter _selectorPresenter;
        private IPeopleNavigatorPresenter _myPresenter;
        private readonly IEventAggregator _localEventAggregator;
        private readonly IEventAggregator _globalEventAggregator;
        private ICurrentScenario _currentScenario;

        public PeopleNavigator(PortalSettings portalSettings, IComponentContext componentContext, IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory, IGracefulDataSourceExceptionHandler gracefulDataSourceExceptionHandler)
            : this()
        {
            _portalSettings = portalSettings;
            _componentContext = componentContext;
        	_personRepository = personRepository;
        	_unitOfWorkFactory = unitOfWorkFactory;
        	_gracefulDataSourceExceptionHandler = gracefulDataSourceExceptionHandler;

        	var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
            _container = lifetimeScope.BeginLifetimeScope();
            _localEventAggregator = _container.Resolve<IEventAggregator>();
            _globalEventAggregator = _container.Resolve<IEventAggregatorLocator>().GlobalAggregator();
            _currentScenario = _container.Resolve<ICurrentScenario>();

			SetTexts();
        }

        public PeopleNavigator()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
        	base.OnLoad(e);
        	if (DesignMode) return;

        	toolStripButtonOpen.Click += onOpenPeople;
        	toolStripButtonAddPerson.Click += onAddNewPeople;

			tryLoadNavigationPanel();
        }

		private void tryLoadNavigationPanel()
    	{
    		if (!_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
    		                                                                             	{
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
    		                                                                             	}))
    		{
    			_selectorPresenter = null;
    			_myPresenter = null;
				splitContainerAdv1.Panel1.Controls.Clear();
    		}
    	}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (navigationPanelIsNotLoaded())
			{
				tryLoadNavigationPanel();
			}

			base.OnVisibleChanged(e);
		}

    	private bool navigationPanelIsNotLoaded()
    	{
    		return _myPresenter == null || _selectorPresenter == null;
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
            if (selectedGuids == null || selectedGuids.IsEmpty()) return;

        	_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
        	                                                                             	{
        	                                                                             		IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();

        	                                                                             		var foundPeople = _personRepository.FindPeople(selectedGuids).ToList();
        	                                                                             		var accounts = new PersonAbsenceAccountRepository(uow).FindByUsers(foundPeople);

        	                                                                             	    ITraceableRefreshService service = new TraceableRefreshService(_currentScenario,new ScheduleRepository(uow));

        	                                                                             		var filteredPeopleHolder = new FilteredPeopleHolder(service, accounts) {
        	                                                                             					SelectedDate = _selectorPresenter.SelectedDate,
        	                                                                             					GetUnitOfWork = uow
        	                                                                             				};

        	                                                                             		var state = new WorksheetStateHolder();

        	                                                                             		loadPeopleGeneralViewAndInitialReferences(state, uow);

        	                                                                             		filteredPeopleHolder.LoadIt();

        	                                                                             		filteredPeopleHolder.SetRotationCollection(state.AllRotations);
        	                                                                             		filteredPeopleHolder.SetAvailabilityCollection(state.AllAvailabilities);

        	                                                                             		filteredPeopleHolder.ReassociateSelectedPeopleWithNewUowOpenPeople(foundPeople);

        	                                                                             		openPeopleAdmin(state, filteredPeopleHolder);
        	                                                                             	});
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
            _gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=>
            {
                IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
                var accounts = new PersonAbsenceAccountRepository(uow).LoadAllAccounts();
                ITraceableRefreshService cacheServiceForPersonAccounts = new TraceableRefreshService(_currentScenario, new ScheduleRepository(uow));
                var state = new WorksheetStateHolder();
                var filteredPeopleHolder = new FilteredPeopleHolder(cacheServiceForPersonAccounts, accounts)
                {
                    SelectedDate = DateOnly.Today,
                    GetUnitOfWork = uow
                };

                //Load the people general view & initial references
                loadPeopleGeneralViewAndInitialReferences(state, uow);
                filteredPeopleHolder.LoadIt();
                
                state.AddAndSavePerson(0, filteredPeopleHolder);
                openPeopleAdmin(state, filteredPeopleHolder);
            });
            
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

    	public bool SendMessageEnable
    	{
			get { return toolStripButtonSendInstantMessage.Enabled; }
			set { toolStripButtonSendInstantMessage.Enabled = value; }
    	}

        public bool AddNewEnabled
        {
            get { return toolStripButtonAddPerson.Enabled; }
            set { toolStripButtonAddPerson.Enabled = value; }
        }

        public void Open()
        {
            OpenPeopleAdmin();
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
            _gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=>
            {
                using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var persons = _personRepository.FindPeople(_selectorPresenter.SelectedPersonGuids);
                    var messageForm = new PushMessageForm(persons,_unitOfWorkFactory,_container.Resolve<IRepositoryFactory>());
                    messageForm.ShowFromWinForms(false,TimeZoneInfo.Local );
                }
            });
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

				using (var findForm = new PersonFinderView(_selectorPresenter.SelectedDate))
                {
                    findForm.DoubleClickSelectedPeople += FindFormDoubleClickSelectedPeople;
                    findForm.ShowDialog(this);
                    if (findForm.DialogResult == DialogResult.OK)
                    {
                        selectedGuids = findForm.SelectedPersonGuids;
                    }
                    findForm.DoubleClickSelectedPeople -= FindFormDoubleClickSelectedPeople;
                }
                openPeople(selectedGuids);

            }
            Cursor.Current = Cursors.Default;
        }

        private void FindFormDoubleClickSelectedPeople(object sender, EventArgs e)
        {
            var form = sender as PersonFinderView;
            if (form == null) return;
            openPeople(form.SelectedPersonGuids);
            form.Close();
        }

        private void toolStripRefreshClick(object sender, EventArgs e)
        {
            _localEventAggregator.GetEvent<RefreshGroupPageClicked>().Publish("");
        }
    }
}
