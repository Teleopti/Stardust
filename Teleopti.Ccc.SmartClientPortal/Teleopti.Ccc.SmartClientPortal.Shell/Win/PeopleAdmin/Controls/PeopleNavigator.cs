using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Common.Interop;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Messaging;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.Win.Main;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls
{
	public partial class PeopleNavigator : BaseUserControl, IPeopleNavigator
	{
		private readonly IApplicationFunction _myApplicationFunction =
			 ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
													  DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

		private readonly PortalSettings _portalSettings;
		private readonly IComponentContext _componentContext;
		private readonly IPersonRepository _personRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGracefulDataSourceExceptionHandler _gracefulDataSourceExceptionHandler;
		private readonly ITenantLogonDataManagerClient _tenantDataManager;
		private readonly IComponentContext _container;
		private IPersonSelectorPresenter _selectorPresenter;
		private IPeopleNavigatorPresenter _myPresenter;
		private readonly IEventAggregator _localEventAggregator;
		private readonly IEventAggregator _globalEventAggregator;
		private ICurrentScenario _currentScenario;
		private Form _mainWindow;
		private readonly IApplicationInsights _applicationInsights;

		public PeopleNavigator(PortalSettings portalSettings, IComponentContext componentContext,
			IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory,
			IGracefulDataSourceExceptionHandler gracefulDataSourceExceptionHandler,
			ITenantLogonDataManagerClient tenantDataManager, IApplicationInsights applicationInsights)
			: this()
		{
			_portalSettings = portalSettings;
			_componentContext = componentContext;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_gracefulDataSourceExceptionHandler = gracefulDataSourceExceptionHandler;
			_tenantDataManager = tenantDataManager;
			_applicationInsights = applicationInsights;

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
			var addPersonPermission = PrincipalAuthorization.Current_DONTUSE().IsPermitted(
				DefinedRaptorApplicationFunctionPaths.AddPerson);
			toolStripButtonAddPerson.Enabled = addPersonPermission;
			if(addPersonPermission)
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

																														var view = (Control)_selectorPresenter.View;
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
			_applicationInsights.TrackEvent("Opened agents in People Module.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope")]
		private void openPeople(IEnumerable<Guid> selectedGuids)
		{
			if (selectedGuids == null || selectedGuids.IsEmpty()) return;
			var saviour = _container.Resolve<ITenantDataManagerClient>();
			var toggle78424 = _container.Resolve<IToggleManager>()
				.IsEnabled(Toggles.SchedulePeriod_HideChineseMonth_78424);
			_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();

				var foundPeople = _personRepository.FindPeople(selectedGuids).ToList();
				var accounts = new PersonAbsenceAccountRepository(uow).FindByUsers(foundPeople);
				var logonData = _tenantDataManager.GetLogonInfoModelsForGuids(selectedGuids);
				var currentUnitOfWork = new ThisUnitOfWork(uow);
				var personAssignmentRepository = _container.Resolve<IPersonAssignmentRepository>();
				var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
				var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
				var noteRepository = new NoteRepository(currentUnitOfWork);
				var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
				var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
				var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
				var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
				ITraceableRefreshService service = new TraceableRefreshService(_currentScenario,
					new ScheduleStorage(currentUnitOfWork, personAssignmentRepository,
						personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
						agentDayScheduleTagRepository, noteRepository,
						publicNoteRepository, preferenceDayRepository,
						studentAvailabilityDayRepository,
						new PersonAvailabilityRepository(currentUnitOfWork),
						new PersonRotationRepository(currentUnitOfWork),
						overtimeAvailabilityRepository,
						new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
						new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
							() => personAbsenceRepository,
							() => preferenceDayRepository, () => noteRepository,
							() => publicNoteRepository,
							() => studentAvailabilityDayRepository,
							() => agentDayScheduleTagRepository,
							() => overtimeAvailabilityRepository),
						CurrentAuthorization.Make()));

				var filteredPeopleHolder = new FilteredPeopleHolder(service, accounts, saviour, _personRepository)
				{
					SelectedDate = _selectorPresenter.SelectedDate,
					GetUnitOfWork = uow
				};

				var state = new WorksheetStateHolder();

				loadPeopleGeneralViewAndInitialReferences(state, uow);

				filteredPeopleHolder.LoadIt();

				filteredPeopleHolder.SetRotationCollection(state.AllRotations);
				filteredPeopleHolder.SetAvailabilityCollection(state.AllAvailabilities);

				filteredPeopleHolder.ReassociateSelectedPeopleWithNewUowOpenPeople(foundPeople, logonData);
				foreach (var person in filteredPeopleHolder.PersonCollection)
				{
					foreach (var schedulePeriod in person.PersonSchedulePeriodCollection)
					{
						((SchedulePeriod) schedulePeriod).Toggle78424 = toggle78424;
					}
				}

				openPeopleAdmin(state, filteredPeopleHolder, _mainWindow);
			});
		}

		void onAddNewPeople(object sender, EventArgs e)
		{
			OpenNewPerson();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void openPeopleAdmin(WorksheetStateHolder stateHolder, FilteredPeopleHolder filteredPeopleHolder, Form mainWindow)
		{
			new PeopleWorksheet(stateHolder, filteredPeopleHolder, _globalEventAggregator, _componentContext, mainWindow).Show();
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
			_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
				IDictionary<IPerson, IPersonAccountCollection> accounts = new Dictionary<IPerson, IPersonAccountCollection>();
				if (_container.Resolve<IToggleManager>().IsEnabled(Toggles.ResourcePlanner_LoadLessPersonAccountsWhenOpeningScheduler_78487))
				{
					accounts = new PersonAbsenceAccountRepository(uow).FindByUsers(new List<IPerson>());
				}
				else
				{
					accounts = new PersonAbsenceAccountRepository(uow).LoadAllAccounts();
				}
				var currentUnitOfWork = new ThisUnitOfWork(uow);
				var personAssignmentRepository = _container.Resolve<IPersonAssignmentRepository>();
				var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
				var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
				var noteRepository = new NoteRepository(currentUnitOfWork);
				var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
				var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
				var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
				var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
				ITraceableRefreshService cacheServiceForPersonAccounts = new TraceableRefreshService(_currentScenario,
					new ScheduleStorage(currentUnitOfWork, personAssignmentRepository,
						personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
						agentDayScheduleTagRepository, noteRepository,
						publicNoteRepository, preferenceDayRepository,
						studentAvailabilityDayRepository,
						new PersonAvailabilityRepository(currentUnitOfWork),
						new PersonRotationRepository(currentUnitOfWork),
						overtimeAvailabilityRepository,
						new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
						new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
							() => personAbsenceRepository,
							() => preferenceDayRepository, () => noteRepository,
							() => publicNoteRepository,
							() => studentAvailabilityDayRepository,
							() => agentDayScheduleTagRepository,
							() => overtimeAvailabilityRepository),
						CurrentAuthorization.Make()));
				var state = new WorksheetStateHolder();
				var saviour = _container.Resolve<ITenantDataManagerClient>();
				var filteredPeopleHolder = new FilteredPeopleHolder(cacheServiceForPersonAccounts, accounts, saviour, _personRepository)
				{
					SelectedDate = DateOnly.Today,
					GetUnitOfWork = uow
				};

				//Load the people general view & initial references
				loadPeopleGeneralViewAndInitialReferences(state, uow);
				filteredPeopleHolder.LoadIt();

				state.AddAndSavePerson(0, filteredPeopleHolder);
				openPeopleAdmin(state, filteredPeopleHolder, _mainWindow);
			});

			Cursor.Current = Cursors.Default;
		}

		public void OpenPeopleAdmin()
		{
			toolStripButtonOpen.PerformClick();
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

		public void SetMainOwner(Form mainWindow)
		{
			_mainWindow = mainWindow;
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
			_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					var persons = _personRepository.FindPeople(_selectorPresenter.SelectedPersonGuids);
					var messageForm = new PushMessageForm(persons, _unitOfWorkFactory, _container.Resolve<IRepositoryFactory>());
					messageForm.ShowFromWinForms(false, TimeZoneInfo.Local);
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
					findForm.DoubleClickSelectedPeople += findFormDoubleClickSelectedPeople;
					findForm.ShowDialog(this);
					if (findForm.DialogResult == DialogResult.OK)
					{
						selectedGuids = findForm.SelectedPersonGuids;
					}
					findForm.DoubleClickSelectedPeople -= findFormDoubleClickSelectedPeople;
				}
				openPeople(selectedGuids);

			}
			Cursor.Current = Cursors.Default;
		}

		private void findFormDoubleClickSelectedPeople(object sender, EventArgs e)
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
