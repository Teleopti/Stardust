using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.Win.Main;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class SchedulerNavigator : BaseUserControl, IScheduleNavigator
	{
		private readonly IComponentContext _container;
		private IOpenPeriodMode _scheduler;

		private readonly PortalSettings _portalSettings;
		private readonly IPersonRepository _personRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGracefulDataSourceExceptionHandler _gracefulDataSourceExceptionHandler;
		private IScheduleNavigatorPresenter _myPresenter;
		private readonly IEventAggregator _localEventAggregator;
		private Form _mainWindow;
		private readonly IApplicationInsights _applicationInsights;

		public SchedulerNavigator()
		{
			InitializeComponent();
		}

		public SchedulerNavigator(IComponentContext componentContext, PortalSettings portalSettings,
			IPersonRepository personRepository, IUnitOfWorkFactory unitOfWorkFactory,
			IGracefulDataSourceExceptionHandler gracefulDataSourceExceptionHandler, IApplicationInsights applicationInsights)
			: this()
		{
			var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
			_container = lifetimeScope.BeginLifetimeScope();
			_localEventAggregator = _container.Resolve<IEventAggregator>();
			_portalSettings = portalSettings;
			_personRepository = personRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_gracefulDataSourceExceptionHandler = gracefulDataSourceExceptionHandler;
			_applicationInsights = applicationInsights;
			SetTexts();
			toolStripButtonOpen.Click += onOpenScheduler;
		}

		protected IPersonSelectorPresenter SelectorPresenter { get; private set; }

		protected virtual IApplicationFunction MyApplicationFunction
		{
			get
			{
				return ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
				DefinedRaptorApplicationFunctionPaths.OpenSchedulePage);
			}
		}

		protected virtual IOpenPeriodMode OpenPeriodMode
		{
			get { return _scheduler ?? (_scheduler = new OpenPeriodSchedulerMode()); }
		}

		protected override void OnLoad(EventArgs e)
		{
			if (DesignMode) return;

			if (OpenPeriodMode == _scheduler)
			{
				splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height - _portalSettings.SchedulerActionPaneHeight;
			}
			else
			{
				splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height - _portalSettings.IntradayActionPaneHeight;
			}

			tryLoadNavigationPanel();
		}

		private void tryLoadNavigationPanel()
		{
			if (!_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				SelectorPresenter = _container.Resolve<IPersonSelectorPresenter>();
				SelectorPresenter.ApplicationFunction = MyApplicationFunction;
				SelectorPresenter.ShowPersons = false;
				SelectorPresenter.ShowFind = false;

				var view = (Control)SelectorPresenter.View;
				splitContainerAdv1.Panel1.Controls.Add(view);
				view.Dock = DockStyle.Fill;
				SelectorPresenter.LoadTabs();

				_myPresenter = _container.Resolve<IScheduleNavigatorPresenter>();
				_myPresenter.Init(this);
				_localEventAggregator.GetEvent<SelectedNodesChanged>().Publish("");

				//temporary, open organize casciding skills will be called from somewhere else
				enableOrganizeCascadingSkills();

				toolStripSeparator1.Visible = false;
				enableCopySchedule();
				enableImportSchedule();

				SetTexts();
				view.Focus();
			}))
			{
				SelectorPresenter = null;
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
			return _myPresenter == null || SelectorPresenter == null;
		}

		void onOpenScheduler(object sender, EventArgs e)
		{
			openWizard();
		}

		public void Open()
		{
			openWizard();
		}

		private void openWizard()
		{
			Cursor.Current = Cursors.WaitCursor;


			var entityList = getSelectedEntities();

			if (entityList == null)
			{
				Cursor.Current = Cursors.Default;
				return;
			}

			using (var openSchedule = new OpenScenarioForPeriod(OpenPeriodMode, entityList))
			{
				if (openSchedule.ShowDialog() != DialogResult.Cancel)
				{
					LogPointOutput.LogInfo("Scheduler.LoadAndOptimizeData:openWizard", "Started");
					StartModule(openSchedule.SelectedPeriod, openSchedule.Scenario, openSchedule.Shrinkage, openSchedule.Calculation, openSchedule.Validation, openSchedule.TeamLeaderMode, openSchedule.Requests, entityList, _mainWindow);
				}
			}

			Cursor.Current = Cursors.Default;
		}

		private Collection<IEntity> getSelectedEntities()
		{
			var entityList = new Collection<IEntity>(); 

			if (!_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=>{
				using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					loadNeededStuffIntoUnitOfWork(uow);

					if (SelectorPresenter.IsOnOrganizationTab)
					{
						var teamRep = TeamRepository.DONT_USE_CTOR(uow);
						var teams = teamRep.FindTeams(SelectorPresenter.SelectedTeamGuids);
						entityList.AddRange(teams.Cast<IEntity>());
					}
					else
					{
						IEnumerable<IPerson> persons = _personRepository.FindPeople(SelectorPresenter.SelectedPersonGuids);
						entityList.AddRange(persons.Cast<IEntity>());
					}
				}
			}))
			{
				return null;
			}

			return entityList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		protected virtual void StartModule(DateOnlyPeriod selectedPeriod, IScenario scenario, bool shrinkage,
			bool calculation, bool validation, bool teamLeaderMode, bool loadRequests, Collection<IEntity> entityCollection, Form ownerWindow)
		{
			_gracefulDataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=>{
				var sc = new SchedulingScreen(_container,
														   selectedPeriod,
														   scenario,
														   shrinkage,
														   calculation,
														   validation,
														   teamLeaderMode,
															loadRequests,
															entityCollection,
														   ownerWindow);
				sc.Show();
			});
			_applicationInsights.TrackEvent("Opened schedule for agents in Schedule Module.");
		}

		private static void loadNeededStuffIntoUnitOfWork(IUnitOfWork uow)
		{
			var businessUnitRepository = BusinessUnitRepository.DONT_USE_CTOR(uow);
			businessUnitRepository.Get(
				((ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnitId.GetValueOrDefault());
			using (uow.DisableFilter(QueryFilter.Deleted))
			{
				SiteRepository.DONT_USE_CTOR(uow).LoadAll();
				TeamRepository.DONT_USE_CTOR(uow).LoadAll();
			}
			
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

		private void splitContainerAdv1SplitterMoved(object sender, SplitterMoveEventArgs e)
		{
			if (_portalSettings == null)
				return;

			if (OpenPeriodMode == _scheduler)
				_portalSettings.SchedulerActionPaneHeight = splitContainerAdv1.Height - splitContainerAdv1.SplitterDistance;
			else
			{
				_portalSettings.IntradayActionPaneHeight = splitContainerAdv1.Height - splitContainerAdv1.SplitterDistance;
			}
		}

		protected void SetOpenToolStripText(string text)
		{
			toolStripButtonOpen.Text = text;
		}

		private void toolStripButtonAddMeetingClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<AddMeetingFromPanelClicked>().Publish("");
		}

		private void toolStripButtonShowMeetingsClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<OpenMeetingsOverviewClicked>().Publish("");
		}

		public void SetMainOwner(Form mainWindow)
		{
			_mainWindow = mainWindow;
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
			set { toolStripButtonOpen.Enabled = value; }
		}

		public bool OpenMeetingOverviewEnabled
		{
			get { return toolStripButtonShowMeetings.Enabled; }
			set { toolStripButtonShowMeetings.Enabled = value; }
		}

		public bool AddMeetingOverviewEnabled
		{
			get { return toolStripButtonAddMeeting.Enabled; }
			set { toolStripButtonAddMeeting.Enabled = value; }
		}

		public bool OpenIntradayTodayEnabled
		{
			get { return TodayButton.Enabled; }
			set { TodayButton.Enabled = value; }
		}

		public ToolStripButton TodayButton
		{
			get { return toolStripButtonToday; }
		}

		private void toolStripRefreshClick(object sender, EventArgs e)
		{
			_localEventAggregator.GetEvent<RefreshGroupPageClicked>().Publish("");
		}

		private void toolStripButtonOpen_Click(object sender, EventArgs e)
		{

		}

		private void enableOrganizeCascadingSkills()
		{
			var toggleManager = _container.Resolve<IToggleManager>();
			var toggled = toggleManager.IsEnabled(Toggles.Wfm_SkillPriorityRoutingGUI_39885);
			var permitted = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills);

			toolStripButtonOrganizeCascadingSkills.Visible = toggled && permitted;
		}

		private void toolStripButtonOrganizeCascadingSkillsClick(object sender, EventArgs e)
		{
			//TODO: won't work if feature toggle svc no longer runs on same web app as WFM
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			var url = $"{wfmPath}WFM/#/skillprio";
			if (url.IsAnUrl())
				Process.Start(url);
		}

		private void enableCopySchedule()
		{
			var permitted = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.CopySchedule);
			toolStripButtonCopySchedule.Visible = permitted;
			if (toolStripButtonCopySchedule.Visible)
				toolStripSeparator1.Visible = true;
		}

		private void toolStripButtonCopyScheduleClick(object sender, EventArgs e)
		{
			openResourcePlannerInWfm("copyschedule");
		}

		private void enableImportSchedule()
		{
			var permitted = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportSchedule);
			toolStripButtonImportSchedule.Visible = permitted;
			if (toolStripButtonImportSchedule.Visible)
				toolStripSeparator1.Visible = true;
		}

		private void toolStripButtonImportScheduleClick(object sender, EventArgs e)
		{
			openResourcePlannerInWfm("importschedule");
		}

		private void openResourcePlannerInWfm(string what)
		{
			var wfmPath = _container.Resolve<IConfigReader>().AppConfig("FeatureToggle");
			var url = $"{wfmPath}WFM/#/resourceplanner/{what}";
			if (url.IsAnUrl())
				Process.Start(url);
		}
	}
}
