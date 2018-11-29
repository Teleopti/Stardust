using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Reporting;


//Problem to open ScheduleReportViewer in design mode -> remove ReportDateFromToSelector control from this control.

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
    public partial class ReportSettingsScheduledTimePerActivityView : BaseUserControl, IReportSettingsScheduledTimePerActivityView
    {
        private readonly IEventAggregator _eventAggregator;
    	private readonly IComponentContext _componentContext;
    	private readonly ReportSettingsScheduledTimePerActivityPresenter _presenter;
        private SchedulerStateHolder _schedulerStateHolder;

        IList<IActivity> _availableActivities = new List<IActivity>();
        readonly IList<IActivity> _selectedActivities = new List<IActivity>();
        private ReportSettingsScheduledTimePerActivity _setting;
		public ReportSettingsScheduledTimePerActivityView(IEventAggregator eventAggregator, IComponentContext componentContext)
        {
            _eventAggregator = eventAggregator;
			_componentContext = componentContext;
			InitializeComponent();

            if (!StateHolderReader.IsInitialized || DesignMode) return;

            _presenter = new ReportSettingsScheduledTimePerActivityPresenter(this);
            _eventAggregator.GetEvent<LoadReportDone>().Subscribe(onLoadReportDone);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!StateHolderReader.IsInitialized || DesignMode) return;

            SetTexts();
            
            reportDateFromToSelector1.WorkPeriodStart = new DateOnly(_setting.StartDate);
            reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(_setting.EndDate);

			reportAgentSelector1.ComponentContext = _componentContext;
			reportAgentSelector1.ReportApplicationFunction =
				ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
											   DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport);
            reportAgentSelector1.OpenDialog += reportAgentSelector1BeforeDialog;
        }

        private void onLoadReportDone(bool obj)
        {
            Enabled = true;
        }

        void reportAgentSelector1BeforeDialog(object sender, EventArgs e)
        {
            _schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(
                reportDateFromToSelector1.GetSelectedDates[0],TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
        }

        private SchedulerStateHolder createStateHolder()
        {
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var factory = new RepositoryFactory();

                var rep = factory.CreatePersonRepository(unitOfWork);

                var period = new DateOnlyPeriod(DateOnly.Today.AddDays(-366),DateOnly.Today.AddDays(366));

                
                _availableActivities = factory.CreateActivityRepository(unitOfWork).LoadAllSortByName();
                
                using (unitOfWork.DisableFilter(QueryFilter.Deleted))
                {
                    factory.CreateSkillRepository(unitOfWork).LoadAll();
                }

                factory.CreateContractScheduleRepository(unitOfWork).LoadAllAggregate();

                ICollection<IPerson> persons = rep.FindAllAgents(period, false);
				return new SchedulerStateHolder(Scenario, new DateOnlyPeriodAsDateTimePeriod(period, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), persons, new DisableDeletedFilter(new ThisUnitOfWork(unitOfWork)), new SchedulingResultStateHolder(), new TimeZoneGuard());
            }
        }

        public void InitAgentSelector()
        {
            _schedulerStateHolder = createStateHolder();
            reportAgentSelector1.SetStateHolder(_schedulerStateHolder);
        }

        public void InitActivitiesSelector()
        {
            loadSetting();
            foreach (var availableActivity in _availableActivities)
            {
                if (availableActivity.Id.HasValue && _setting.Activities.Contains(availableActivity.Id.Value))
                    _selectedActivities.Add(availableActivity);
            }
            twoListSelector1.Initiate(_availableActivities, _selectedActivities, "Description",
                                      Resources.Activities, "");
            //using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            //{
            //    RepositoryFactory rep = new RepositoryFactory();
            //    _availableActivities = rep.CreateActivityRepository(unitOfWork).LoadAllSortByName();

            //    twoListSelector1.Initiate(_availableActivities, _selectedActivities, "Description",
            //                              UserTexts.Resources.Activities, "");
            //}
        }

        public void InitializeSettings()
        {
            _presenter.InitializeSettings();
        }

        public IScenario Scenario
        {
            get { return reportScenarioSelector1.SelectedItem; }
        }

        public DateOnlyPeriod Period
        {
            get { return reportDateFromToSelector1.GetSelectedDates.First(); }
        }

        public IList<IPerson> Persons
        {
            get
            {
				_schedulerStateHolder.FilterPersons(reportAgentSelector1.SelectedPersonGuids);
				return _schedulerStateHolder.FilteredCombinedAgentsDictionary.Values.ToList();
            }
        }

        public TimeZoneInfo TimeZone
        {
            get
            {
                if (reportTimeZoneSelector1.Visible)
                    return reportTimeZoneSelector1.TimeZone();

                return TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
            }

        }

        public IList<IActivity> Activities
        {
            get { return twoListSelector1.GetSelected<IActivity>(); }
        }

        private void buttonAdvOkClick(object sender, EventArgs e)
        {
            Enabled = false;
            try
            {
                saveSetting();
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }

                Enabled = true;
                return;
            }
            _eventAggregator.GetEvent<LoadReport>().Publish(true);
        }

        public ReportSettingsScheduledTimePerActivityModel ScheduleTimePerActivitySettingsModel
        {
            get { return _presenter.GetSettingsModel(); }
        }

        public void HideTimeZoneControl()
        {
            reportTimeZoneSelector1.Visible = false;
        }

        private void saveSetting()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IList<Guid> personList = new List<Guid>();

                foreach (var person in Persons)
                {
                    if (!person.Id.HasValue) throw new ArgumentException("persons");

                    personList.Add(person.Id.Value);

                }
                _setting.Persons = personList;

                IList<Guid> activities = new List<Guid>();
                foreach (var selectedActivity in Activities)
                {
                    if(selectedActivity.Id.HasValue)
                        activities.Add(selectedActivity.Id.Value);
                }

                _setting.Activities = activities;

                if (Scenario.Id.HasValue)
                    _setting.Scenario = Scenario.Id.Value;

                _setting.StartDate = Period.StartDate.Date;
                _setting.EndDate = Period.EndDate.Date;
                _setting.GroupPage = reportAgentSelector1.SelectedGroupPageKey;

                new PersonalSettingDataRepository(uow).PersistSettingValue(_setting);
                uow.PersistAll();
            }
        }

        private void loadSetting()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _setting = new PersonalSettingDataRepository(uow).FindValueByKey("ReportSettingsScheduledTimePerActivity", new ReportSettingsScheduledTimePerActivity());
            }

            applySetting();
        }

        private void applySetting()
        {
            if (!_setting.Scenario.HasValue) return;

			reportAgentSelector1.SetSelectedPersons(new HashSet<Guid>(_setting.Persons));
            reportAgentSelector1.UpdateComboWithSelectedAgents();
            reportAgentSelector1.SelectedGroupPageKey = _setting.GroupPage;
            
            using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var scenarioRepository = new ScenarioRepository(unitOfWork);
                var scenarios = scenarioRepository.LoadAll();

                foreach (var scenario in scenarios)
                {
                    if (scenario.Id != _setting.Scenario.Value) continue;
                    reportScenarioSelector1.SelectedItem = scenario;
                    break;
                }
            }

        }
    }
}
