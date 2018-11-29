using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	public static class ReportHandler
	{

		public static ReportDetail CreateReportDetail(IApplicationFunction appFunctionReport)
		{
			var reportDetail = new ReportDetail
			{
				FunctionPath = appFunctionReport.FunctionPath,
				FunctionCode = appFunctionReport.FunctionCode,
				File = "report_schedule_time_vs_target.rdlc",
				DisplayName = UserTexts.Resources.ScheduledTimeVsTarget
			};
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static void CreateScheduledTimeVersusTargetData(IUnitOfWork unitOfWork, ReportSettingsScheduleTimeVersusTargetTimeModel model, Dictionary<string, IList<IScheduledTimeVersusTargetTimeReportData>> data, TimeZoneInfo timeZoneInfo, CommonNameDescriptionSetting commonNameDescriptionSetting)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (data == null)
				throw new ArgumentNullException(nameof(data));

			if (commonNameDescriptionSetting == null)
				throw new ArgumentNullException(nameof(commonNameDescriptionSetting));

			data.Clear();

			IList<IScheduledTimeVersusTargetTimeReportData> detailDataList = new List<IScheduledTimeVersusTargetTimeReportData>();

			var startDate = DateOnly.MaxValue;
			var endDate = DateOnly.MinValue;
			var personDictionary = new Dictionary<IPerson, IList<IVirtualSchedulePeriod>>();

			model.Persons.ForEach(person =>
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

			});

			var loadPeriod = new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue);

			if(endDate > startDate)
				loadPeriod = new DateOnlyPeriod(startDate, endDate);
			var stateHolder = new SchedulerStateHolder(model.Scenario, new DateOnlyPeriodAsDateTimePeriod(loadPeriod, timeZoneInfo), model.Persons, new DisableDeletedFilter(new ThisUnitOfWork(unitOfWork)), new SchedulingResultStateHolder(), new TimeZoneGuard());
			loadSchedules(unitOfWork, model.Persons, stateHolder);
			
			personDictionary.ForEach(personSchedulePeriods =>
			{
				var person = personSchedulePeriods.Key;
				var schedulePeriods = personSchedulePeriods.Value;

				schedulePeriods.ForEach(schedulePeriod =>
				{
					//TODO: refactor report to use the same data as in the agentinfo in scheduler. DaysOff, TargetTime, ScheduledDayOff, ScheduledTime 
					// should be the virtual schedule period's responsibility where is is tested code!!! This is NOT!

					stateHolder.Schedules[person].ForceRecalculationOfTargetTimeContractTimeAndDaysOff();

					IScheduledTimeVersusTargetTimeReportData detailData = new ScheduledTimeVersusTargetTimeReportData();

					var targetTime = stateHolder.Schedules[person].CalculatedTargetTimeHolder(schedulePeriod.DateOnlyPeriod);

					detailData.PersonName = commonNameDescriptionSetting.BuildFor(person);
					detailData.PeriodFrom = schedulePeriod.DateOnlyPeriod.StartDate.Date;
					detailData.PeriodTo = schedulePeriod.DateOnlyPeriod.EndDate.Date;
					if (targetTime.HasValue) detailData.TargetTime = targetTime.Value.TotalMinutes;
					detailData.TargetDayOffs = stateHolder.Schedules[person].CalculatedTargetScheduleDaysOff(schedulePeriod.DateOnlyPeriod).GetValueOrDefault(0);

					detailData.ScheduledTime = stateHolder.Schedules[person].CalculatedContractTimeHolderOnPeriod(schedulePeriod.DateOnlyPeriod).TotalMinutes;
					detailData.ScheduledDayOffs = stateHolder.Schedules[person].CalculatedScheduleDaysOffOnPeriod(schedulePeriod.DateOnlyPeriod);
					detailDataList.Add(detailData);
				});
			});

			data.Add("DataSet1", detailDataList);
		}

		private static void loadSchedules(IUnitOfWork unitOfWork, IEnumerable<IPerson> persons, ISchedulerStateHolder stateHolder)
		{
			if (stateHolder == null) throw new ArgumentNullException(nameof(stateHolder));
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);
			unitOfWork.Reassociate(persons);
			var repositoryFactory = new RepositoryFactory();
			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			stateHolder.LoadSchedules(new ScheduleStorage(currentUnitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork), CurrentAuthorization.Make()), persons, scheduleDictionaryLoadOptions, stateHolder.RequestedPeriod.Period());
		}
	
		public static IList<IReportDataParameter> CreateScheduledTimeVersusTargetParameters(ReportSettingsScheduleTimeVersusTargetTimeModel model, IFormatProvider culture)
		{
			if(model == null)
				throw new ArgumentNullException(nameof(model));

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
			parameters.Add(new ReportDataParameter("param_timezone", TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone.StandardName));
		}

	}
}
