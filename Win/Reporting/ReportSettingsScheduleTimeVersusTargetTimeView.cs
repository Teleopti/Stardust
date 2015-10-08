
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Reporting
{
	public partial class ReportSettingsScheduleTimeVersusTargetTimeView : BaseUserControl, IReportSettingsScheduleTimeVersusTargetTimeView
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IComponentContext _componentContext;
		private readonly ReportSettingsScheduleTimeVersusTargetTimePresenter _presenter;
		private SchedulerStateHolder _schedulerStateHolder;
		private ReportSettingsScheduledTimeVersusTarget _setting;

		public ReportSettingsScheduleTimeVersusTargetTimeView(IEventAggregator eventAggregator, IComponentContext componentContext)
		{
			_eventAggregator = eventAggregator;
			_componentContext = componentContext;
			InitializeComponent();

			if (!StateHolderReader.IsInitialized || DesignMode) return;

            _presenter = new ReportSettingsScheduleTimeVersusTargetTimePresenter(this);
            _eventAggregator.GetEvent<LoadReportDone>().Subscribe(onLoadReportDone);
		}

	    private void onLoadReportDone(bool obj)
	    {
            Enabled = true;
	    }

	    protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!StateHolderReader.IsInitialized || DesignMode) return;

			SetTexts();

			LoadSetting();
	    	reportAgentSelector1.ComponentContext = _componentContext;
	    	reportAgentSelector1.ReportApplicationFunction =
	    		ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
	    		                               DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport);
			reportAgentSelector1.OpenDialog += ReportAgentSelector1BeforeDialog;
		}

		public void InitializeSettings()
		{
			_presenter.InitializeSettings();
		}

		public IList<IPerson> Persons
		{
			get
			{
				_schedulerStateHolder.FilterPersons(reportAgentSelector1.SelectedPersonGuids);
				return _schedulerStateHolder.FilteredPersonDictionary.Values.ToList();
			}
		}

		public IScenario Scenario
		{
			get { return reportScenarioSelector1.SelectedItem; }
		}

		public DateOnlyPeriod Period
		{
			get { return reportDateFromToSelector1.GetSelectedDates.First(); }
		}

		public ReportSettingsScheduleTimeVersusTargetTimeModel ScheduleTimeVersusTargetTimeModel
		{
			get { return _presenter.GetSettingsModel; }
		}

		void ReportAgentSelector1BeforeDialog(object sender, EventArgs e)
		{
			_schedulerStateHolder.RequestedPeriod =
				new DateOnlyPeriodAsDateTimePeriod(reportDateFromToSelector1.GetSelectedDates[0],TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
		}

		private SchedulerStateHolder CreateStateHolder()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(new FromFactory(() =>UnitOfWorkFactory.Current));

				var period = new DateOnlyPeriod(DateOnly.Today.AddDays(-366), DateOnly.Today.AddDays(366));

				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					new ContractScheduleRepository(unitOfWork).LoadAllAggregate();
					new ContractRepository(unitOfWork).LoadAll();					
				}
				new PartTimePercentageRepository(unitOfWork).LoadAll();
				var persons = rep.FindPeopleTeamSiteSchedulePeriodWorkflowControlSet(period);

				return new SchedulerStateHolder(Scenario, new DateOnlyPeriodAsDateTimePeriod(period, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), persons, new DisableDeletedFilter(new ThisUnitOfWork(unitOfWork)), new SchedulingResultStateHolder(), new TimeZoneGuardWrapper());
			}
		}

		public void InitAgentSelector()
		{
			_schedulerStateHolder = CreateStateHolder();
			reportAgentSelector1.SetStateHolder(_schedulerStateHolder);
		}

		private void ButtonAdvOkClick(object sender, EventArgs e)
		{
			Enabled = false;
			try
			{
				SaveSetting();
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view = new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports , Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				Enabled = true;
				return;
			}

			Enabled = false;
			_eventAggregator.GetEvent<LoadReport>().Publish(true);
		}

		private void SaveSetting()
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

				if (Scenario.Id.HasValue)
					_setting.Scenario = Scenario.Id.Value;

				_setting.StartDate = Period.StartDate.Date;
				_setting.EndDate = Period.EndDate.Date;
				_setting.GroupPage = reportAgentSelector1.SelectedGroupPageKey;

				new PersonalSettingDataRepository(uow).PersistSettingValue(_setting);
				uow.PersistAll();
			}	
		}

		private void LoadSetting()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_setting = new PersonalSettingDataRepository(uow).FindValueByKey("ReportSettingsScheduledTimeVersusTarget", new ReportSettingsScheduledTimeVersusTarget());
			}

			ApplySetting();
		}

		private void ApplySetting()
		{
			if (!_setting.Scenario.HasValue) return;

			reportAgentSelector1.SetSelectedPersons(new HashSet<Guid>(_setting.Persons));
			reportAgentSelector1.UpdateComboWithSelectedAgents();
			reportAgentSelector1.SelectedGroupPageKey = _setting.GroupPage;
			reportDateFromToSelector1.WorkPeriodStart = new DateOnly(_setting.StartDate);
			reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(_setting.EndDate);

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
