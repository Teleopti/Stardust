using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class SchedulerNavigator : BaseUserControl, IScheduleNavigator
    {
        private readonly IComponentContext _container;
        private IOpenPeriodMode _scheduler;

        private readonly PortalSettings _portalSettings;
       // protected IPersonSelectorPresenter SelectorPresenter;
        private IScheduleNavigatorPresenter _myPresenter;
        private readonly IEventAggregator _localEventAggregator;


        public SchedulerNavigator()
        {
            InitializeComponent();
        }


        public SchedulerNavigator(IComponentContext componentContext, PortalSettings portalSettings)
            : this()
        {
            var lifetimeScope = componentContext.Resolve<ILifetimeScope>();
            _container = lifetimeScope.BeginLifetimeScope();
            _localEventAggregator = _container.Resolve<IEventAggregator>();
            _portalSettings = portalSettings;
            SetTexts();
            toolStripButtonOpen.Click += onOpenScheduler;
        }

        protected IPersonSelectorPresenter SelectorPresenter { get; private set; }

        protected virtual IApplicationFunction MyApplicationFunction
        {
            get
            {
                return ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                DefinedRaptorApplicationFunctionPaths.OpenSchedulePage);
            }
        }

        protected virtual IOpenPeriodMode OpenPeriodMode
        {
            get { return _scheduler ?? (_scheduler = new OpenPeriodSchedulerMode()); }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!DesignMode)
            {
                if (OpenPeriodMode == _scheduler)
                    splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height - _portalSettings.SchedulerActionPaneHeight;
                else
                {
                    splitContainerAdv1.SplitterDistance = splitContainerAdv1.Height - _portalSettings.IntradayActionPaneHeight;
                }

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

                SetTexts();
            }
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
                    StartModule(openSchedule.SelectedPeriod, openSchedule.Scenario, openSchedule.Shrinkage, openSchedule.Calculation, openSchedule.Validation, openSchedule.TeamLeaderMode, entityList);
                }
            }

            Cursor.Current = Cursors.Default;
        }

        private Collection<IEntity> getSelectedEntities()
        {
            var entityList = new Collection<IEntity>(); 

            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    loadBusinessUnitIntoUnitOfWork(uow);

                    if (SelectorPresenter.IsOnOrganizationTab)
                    {
                        var teamRep = new TeamRepository(uow);
                        var teams = teamRep.FindTeams(SelectorPresenter.SelectedTeamGuids);
                        entityList.AddRange(teams.Cast<IEntity>());
                    }
                    else
                    {
                        var rep = new PersonRepository(uow);
                        IEnumerable<IPerson> persons = rep.FindPeople(SelectorPresenter.SelectedPersonGuids);
                        entityList.AddRange(persons.Cast<IEntity>());
                    }
                }
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceException(dataSourceException, Resources.OpenTeleoptiCCC);
                return null;
            }

            return entityList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected virtual void StartModule(DateOnlyPeriod selectedPeriod, IScenario scenario, bool shrinkage,
            bool calculation, bool validation, bool teamLeaderMode, Collection<IEntity> entityCollection)
        {
            try
            {
                var sc = new SchedulingScreen(_container,
                                                           selectedPeriod,
                                                           scenario,
                                                           shrinkage,
                                                           calculation,
                                                           validation,
                                                           teamLeaderMode,
                                                           entityCollection);
                sc.Show();
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceException(dataSourceException, Resources.OpenTeleoptiCCC );
            }
        }

		private static void loadBusinessUnitIntoUnitOfWork(IUnitOfWork uow)
		{
			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Get(
				((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault());
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

       
    }
}
