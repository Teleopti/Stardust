using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.OnlineReporting;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Reporting
{
	public static class ReportHandler
	{
		// This one is called only from SchedulingScreen
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void ShowReport(ReportDetail reportDetail, ScheduleViewBase scheduleViewBase, IScenario loadedScenario, CultureInfo culture)
		{
			switch (reportDetail.FunctionPath)
			{
				case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
					showScheduleTimePerActivity(reportDetail, scheduleViewBase, loadedScenario, culture);
					break;
			}
		}

		public static ReportDetail CreateReportDetail(IApplicationFunction appFunctionReport)
		{
			var reportDetail = new ReportDetail {FunctionPath = appFunctionReport.FunctionPath, FunctionCode = appFunctionReport.FunctionCode};
			switch (reportDetail.FunctionPath)
			{
				case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
					reportDetail.File = "report_scheduled_time_per_activity.rdlc";
					reportDetail.DisplayName = UserTexts.Resources.ScheduledTimePerActivity;
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:
					reportDetail.File = "report_auditing.rdlc";
					reportDetail.DisplayName = UserTexts.Resources.ScheduleAuditTrailReport;
					break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
					reportDetail.File = "report_schedule_time_vs_target.rdlc";
					reportDetail.DisplayName = UserTexts.Resources.ScheduledTimeVsTarget;
					break;
			}
			return reportDetail;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static void ShowReport(ReportDetail reportDetail, IComponentContext componentContext, IApplicationFunction applicationFunction)
		{
			var viewer = new SchedulerReportsViewer(new EventAggregator(), componentContext, applicationFunction);

			try
			{
				viewer.ShowSettings(reportDetail);
				
			}
			catch (DataSourceException dataSourceException)
			{
				using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenReports, UserTexts.Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				viewer.Close();
				return;
			}

			viewer.Show();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void CreateScheduleTimePerActivityData(IScheduleDictionary dictionary,
																				ReportSettingsScheduledTimePerActivityModel model,
																				Dictionary<string, IList<IReportData>> data)
		{
			IList<IPayload> payLoads = new List<IPayload>();
			foreach(IActivity activity in model.Activities)
			{
				payLoads.Add(activity);
			}

			data.Add("DataSet1", ScheduledTimePerActivityModel.GetReportDataFromScheduleDictionary(dictionary, model.Persons,
																											   model.Period.DayCollection(),
																											   payLoads));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void CreateScheduledTimeVersusTargetData(IUnitOfWork unitOfWork, ReportSettingsScheduleTimeVersusTargetTimeModel model, Dictionary<string, IList<IScheduledTimeVersusTargetTimeReportData>> data, TimeZoneInfo timeZoneInfo, CommonNameDescriptionSetting commonNameDescriptionSetting)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			if (data == null)
				throw new ArgumentNullException("data");

			if (commonNameDescriptionSetting == null)
				throw new ArgumentNullException("commonNameDescriptionSetting");

			data.Clear();

			IList<IScheduledTimeVersusTargetTimeReportData> detailDataList = new List<IScheduledTimeVersusTargetTimeReportData>();

			var startDate = DateOnly.MaxValue;
			var endDate = DateOnly.MinValue;
			var personDictionary = new Dictionary<IPerson, IList<IVirtualSchedulePeriod>>();

			foreach(var person in model.Persons)
			{
				var periodFinder = new VirtualSchedulePeriodFinder(person);
				var schedulePeriods = periodFinder.FindVirtualPeriods(model.Period);

				if (schedulePeriods.Count > 0)
				{
					startDate = startDate > schedulePeriods.Min(schedulePeriod => schedulePeriod.DateOnlyPeriod.StartDate)
									? schedulePeriods.Min(schedulePeriod => schedulePeriod.DateOnlyPeriod.StartDate)
									: startDate;
					endDate = endDate < schedulePeriods.Max(schedulePeriod => schedulePeriod.DateOnlyPeriod.EndDate)
								? schedulePeriods.Max(schedulePeriod => schedulePeriod.DateOnlyPeriod.EndDate)
								: endDate;
					personDictionary.Add(person, schedulePeriods);
				}
			}

			var loadPeriod = new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue);

			if(endDate > startDate)
				loadPeriod = new DateOnlyPeriod(startDate, endDate);
			var stateHolder = new SchedulerStateHolder(model.Scenario, new DateOnlyPeriodAsDateTimePeriod(loadPeriod,timeZoneInfo), model.Persons);
			stateHolder.CommonNameDescription.AliasFormat = commonNameDescriptionSetting.AliasFormat;
			loadSchedules(unitOfWork, model.Persons, stateHolder);
			
			foreach(KeyValuePair<IPerson, IList<IVirtualSchedulePeriod>> personSchedulePeriods in personDictionary)
			{
				foreach(var schedulePeriod in personSchedulePeriods.Value)
				{
					IScheduledTimeVersusTargetTimeReportData detailData = new ScheduledTimeVersusTargetTimeReportData();
					
					var period = new DateOnlyPeriod(schedulePeriod.DateOnlyPeriod.StartDate, schedulePeriod.DateOnlyPeriod.EndDate);
					var virtualSchedulePeriods = ViewBaseHelper.ExtractVirtualPeriods(personSchedulePeriods.Key, period);
					var targetTime = ViewBaseHelper.CalculateTargetTime(virtualSchedulePeriods, stateHolder.SchedulingResultState, false);

					detailData.PersonName = stateHolder.CommonAgentName(personSchedulePeriods.Key);
					detailData.PeriodFrom = schedulePeriod.DateOnlyPeriod.StartDate;
					detailData.PeriodTo = schedulePeriod.DateOnlyPeriod.EndDate;
					if (targetTime.HasValue) detailData.TargetTime = targetTime.Value.TotalMinutes;
					detailData.TargetDayOffs = ViewBaseHelper.CalculateTargetDaysOff(virtualSchedulePeriods);
					detailData.ScheduledTime = ViewBaseHelper.CurrentContractTime(stateHolder.Schedules[personSchedulePeriods.Key], period).TotalMinutes;
					detailData.ScheduledDayOffs = ViewBaseHelper.CurrentTotalDayOffs(stateHolder.Schedules[personSchedulePeriods.Key], period);

					detailDataList.Add(detailData);
				}
			}

			data.Add("DataSet1", detailDataList);
		}

		private static void loadSchedules(IUnitOfWork unitOfWork, IEnumerable<IPerson> persons, ISchedulerStateHolder stateHolder)
		{
			if (stateHolder == null) throw new ArgumentNullException("stateHolder");
			var period = new ScheduleDateTimePeriod(stateHolder.RequestedPeriod.Period(), stateHolder.SchedulingResultState.PersonsInOrganization);
			IPersonProvider personsInOrganizationProvider = new PersonsInOrganizationProvider(persons) { DoLoadByPerson = true};
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);
			unitOfWork.Reassociate(persons);
			stateHolder.LoadSchedules(new ScheduleRepository(unitOfWork), personsInOrganizationProvider, scheduleDictionaryLoadOptions, period);
		}
	
		public static IList<IReportDataParameter> CreateScheduleAuditingParameters(ReportSettingsScheduleAuditingModel model, IFormatProvider culture)
		{
			if(model == null)
				throw new ArgumentNullException("model");

			IList<IReportDataParameter> parameters = new List<IReportDataParameter>();
			parameters.Add(new ReportDataParameter("param_audit_owner", model.ModifiedByNameCommaSeparated()));
			parameters.Add(new ReportDataParameter("param_change_date_from", model.ChangePeriodDisplay.StartDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_change_date_to", model.ChangePeriodDisplay.EndDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_date_from", model.SchedulePeriodDisplay.StartDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_date_to", model.SchedulePeriodDisplay.EndDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_agents", model.AgentsNameCommaSeparated()));

			return parameters;
		}

		public static IList<IReportDataParameter> CreateScheduledTimeVersusTargetParameters(ReportSettingsScheduleTimeVersusTargetTimeModel model, IFormatProvider culture)
		{
			if(model == null)
				throw new ArgumentNullException("model");

			IList<IReportDataParameter> parameters = new List<IReportDataParameter>
														{
															new ReportDataParameter("param_scenario", model.Scenario.Description.Name)
														};

			var agents = string.Join(", ", model.Persons.Select(person => person.Name.ToString()).ToArray());
			parameters.Add(new ReportDataParameter("param_agents", agents));

			parameters.Add(new ReportDataParameter("param_date_from", model.Period.StartDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_date_to", model.Period.EndDate.ToShortDateString(culture)));
			
			return parameters;

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static IList<IReportDataParameter> CreateScheduleTimePerActivityParameters(ReportSettingsScheduledTimePerActivityModel model, IFormatProvider culture)
		{
			IList<IReportDataParameter> parameters = new List<IReportDataParameter>();
			parameters.Add(new ReportDataParameter("param_scenario", model.Scenario.Description.Name));

			var names = new List<string>();
			foreach (var person in model.Persons)
			{
				names.Add(person.Name.ToString());
			}
			string agents = string.Join(", ", names.ToArray());
			parameters.Add(new ReportDataParameter("param_agents", agents));

			var activityList = new List<string>();
			foreach (var activity in model.Activities)
			{
				activityList.Add(activity.Name);
			}
			string activityString = string.Join(", ", activityList.ToArray());
			parameters.Add(new ReportDataParameter("param_activities", activityString));
			parameters.Add(new ReportDataParameter("param_date_from", model.Period.StartDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_date_to", model.Period.EndDate.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_timezone", model.TimeZone.StandardName));

			return parameters;
		}

		private static void createScheduleTimePerActivityParameters(ScheduleViewBase scheduleViewBase, IScenario loadedScenario, IList<IReportDataParameter> parameters, CultureInfo culture)
		{
			parameters.Add(new ReportDataParameter("param_scenario", loadedScenario.Description.Name));

			var names = new List<string>();
			IEnumerable<IScheduleDay> selectedSchedules = scheduleViewBase.SelectedSchedules();
			foreach (var person in scheduleViewBase.AllSelectedPersons(selectedSchedules))
			{
				names.Add(person.Name.ToString());
			}
			string agents = string.Join(", ", names.ToArray());
			parameters.Add(new ReportDataParameter("param_agents", agents));
			parameters.Add(new ReportDataParameter("param_activities", UserTexts.Resources.All));

			var dates = selectedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			DateOnly dateFrom = dates.FirstOrDefault();
			DateOnly dateTo = dates.LastOrDefault();
			
			parameters.Add(new ReportDataParameter("param_date_from", dateFrom.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_date_to", dateTo.ToShortDateString(culture)));
			parameters.Add(new ReportDataParameter("param_timezone", TeleoptiPrincipal.Current.Regional.TimeZone.StandardName));
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void RefreshScheduleTimePerActivity(SchedulerReportsViewer viewer, ReportDetail reportDetail, ScheduleViewBase scheduleViewBase, IScenario loadedScenario, CultureInfo culture)
		{
			var parameters = new List<IReportDataParameter>();
			createScheduleTimePerActivityParameters(scheduleViewBase, loadedScenario, parameters, culture);

			var data = new Dictionary<string, IList<IReportData>>();
			data.Add("DataSet1", ScheduledTimePerActivityModel.GetReportDataFromScheduleParts(scheduleViewBase.SelectedSchedules()));

			viewer.LoadScheduledTimePerActivityReport(reportDetail, data, parameters, scheduleViewBase, loadedScenario);      
		}


		// måste nog brytas isär lite om man ska ladda från ScheduleDictionary från trädet.
		private static void showScheduleTimePerActivity(ReportDetail reportDetail, ScheduleViewBase scheduleViewBase, IScenario loadedScenario, CultureInfo culture)
		{
			var viewer = new SchedulerReportsViewer(new EventAggregator(), null, null);

			var parameters = new List<IReportDataParameter>();
			createScheduleTimePerActivityParameters(scheduleViewBase, loadedScenario, parameters, culture);
   
			var data = new Dictionary<string, IList<IReportData>>();
			data.Add("DataSet1", ScheduledTimePerActivityModel.GetReportDataFromScheduleParts(scheduleViewBase.SelectedSchedules()));

			viewer.LoadScheduledTimePerActivityReport(reportDetail, data, parameters, scheduleViewBase, loadedScenario);
			viewer.Show();
		}
	}
}
