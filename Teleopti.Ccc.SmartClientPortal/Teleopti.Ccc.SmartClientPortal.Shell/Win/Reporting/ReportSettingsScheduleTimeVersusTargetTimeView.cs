﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Reporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	public partial class ReportSettingsScheduleTimeVersusTargetTimeView : BaseUserControl,
		IReportSettingsScheduleTimeVersusTargetTimeView
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly IComponentContext _componentContext;
		private readonly ReportSettingsScheduleTimeVersusTargetTimePresenter _presenter;
		private SchedulingScreenState _schedulerStateHolder;
		private ReportSettingsScheduledTimeVersusTarget _setting;

		public ReportSettingsScheduleTimeVersusTargetTimeView(IEventAggregator eventAggregator,
			IComponentContext componentContext)
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

			loadSetting();
			reportAgentSelector1.ComponentContext = _componentContext;
			reportAgentSelector1.ReportApplicationFunction =
				ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
					DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport);
			reportAgentSelector1.OpenDialog += reportAgentSelector1BeforeDialog;
		}

		public void InitializeSettings()
		{
			_presenter.InitializeSettings();
		}

		public IList<IPerson> Persons
		{
			get
			{
				_schedulerStateHolder.SchedulerStateHolder.FilterPersons(reportAgentSelector1.SelectedPersonGuids);
				return _schedulerStateHolder.SchedulerStateHolder.FilteredCombinedAgentsDictionary.Values.ToList();
			}
		}

		public IScenario Scenario => reportScenarioSelector1.SelectedItem;

		public DateOnlyPeriod Period => reportDateFromToSelector1.GetSelectedDates.First();

		public ReportSettingsScheduleTimeVersusTargetTimeModel ScheduleTimeVersusTargetTimeModel =>
			_presenter.GetSettingsModel;

		private void reportAgentSelector1BeforeDialog(object sender, EventArgs e)
		{
			_schedulerStateHolder.SchedulerStateHolder.RequestedPeriod =
				new DateOnlyPeriodAsDateTimePeriod(reportDateFromToSelector1.GetSelectedDates[0],
					TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
		}

		private SchedulerStateHolder createStateHolder()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(new FromFactory(() => UnitOfWorkFactory.Current), null, null);
				var period = new DateOnlyPeriod(DateOnly.Today.AddDays(-366), DateOnly.Today.AddDays(366));

				using (unitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					ContractScheduleRepository.DONT_USE_CTOR(unitOfWork).LoadAllAggregate();
					ContractRepository.DONT_USE_CTOR(unitOfWork).LoadAll();
				}

				PartTimePercentageRepository.DONT_USE_CTOR(unitOfWork).LoadAll();
				var persons = rep.FindPeopleTeamSiteSchedulePeriodWorkflowControlSet(period);

				var dateTimePeriod =
					new DateOnlyPeriodAsDateTimePeriod(period, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
				var filter = new DisableDeletedFilter(new ThisUnitOfWork(unitOfWork));
				var schedulingResultStateHolder = new SchedulingResultStateHolder {LoadedAgents = persons};
				return new SchedulerStateHolder(Scenario, dateTimePeriod, persons, filter, schedulingResultStateHolder);
			}
		}

		public void InitAgentSelector()
		{
			_schedulerStateHolder = new SchedulingScreenState(null, createStateHolder());
			reportAgentSelector1.SetStateHolder(_schedulerStateHolder);
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
				using (var view =
					new SimpleExceptionHandlerView(dataSourceException, Resources.OpenReports, Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				Enabled = true;
				return;
			}

			Enabled = false;
			_eventAggregator.GetEvent<LoadReport>().Publish(true);
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
				_setting = new PersonalSettingDataRepository(uow).FindValueByKey("ReportSettingsScheduledTimeVersusTarget",
					new ReportSettingsScheduledTimeVersusTarget());
			}

			applySetting();
		}

		private void applySetting()
		{
			if (!_setting.Scenario.HasValue) return;

			reportAgentSelector1.SetSelectedPersons(new HashSet<Guid>(_setting.Persons));
			reportAgentSelector1.UpdateComboWithSelectedAgents();
			reportAgentSelector1.SelectedGroupPageKey = _setting.GroupPage;
			reportDateFromToSelector1.WorkPeriodStart = new DateOnly(_setting.StartDate);
			reportDateFromToSelector1.WorkPeriodEnd = new DateOnly(_setting.EndDate);

			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var scenarioRepository = ScenarioRepository.DONT_USE_CTOR(unitOfWork);
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
