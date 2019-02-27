using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

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
			var schedules = loadSchedules(unitOfWork, model.Persons, model.Scenario, new DateOnlyPeriodAsDateTimePeriod(loadPeriod, timeZoneInfo));
			
			personDictionary.ForEach(personSchedulePeriods =>
			{
				var person = personSchedulePeriods.Key;
				var schedulePeriods = personSchedulePeriods.Value;

				schedulePeriods.ForEach(schedulePeriod =>
				{
					//TODO: refactor report to use the same data as in the agentinfo in scheduler. DaysOff, TargetTime, ScheduledDayOff, ScheduledTime 
					// should be the virtual schedule period's responsibility where is is tested code!!! This is NOT!

					schedules[person].ForceRecalculationOfTargetTimeContractTimeAndDaysOff();

					IScheduledTimeVersusTargetTimeReportData detailData = new ScheduledTimeVersusTargetTimeReportData();

					var targetTime = schedules[person].CalculatedTargetTimeHolder(schedulePeriod.DateOnlyPeriod);

					detailData.PersonName = commonNameDescriptionSetting.BuildFor(person);
					detailData.PeriodFrom = schedulePeriod.DateOnlyPeriod.StartDate.Date;
					detailData.PeriodTo = schedulePeriod.DateOnlyPeriod.EndDate.Date;
					if (targetTime.HasValue) detailData.TargetTime = targetTime.Value.TotalMinutes;
					detailData.TargetDayOffs = schedules[person].CalculatedTargetScheduleDaysOff(schedulePeriod.DateOnlyPeriod).GetValueOrDefault(0);

					detailData.ScheduledTime = schedules[person].CalculatedContractTimeHolderOnPeriod(schedulePeriod.DateOnlyPeriod).TotalMinutes;
					detailData.ScheduledDayOffs = schedules[person].CalculatedScheduleDaysOffOnPeriod(schedulePeriod.DateOnlyPeriod);
					detailDataList.Add(detailData);
				});
			});

			data.Add("DataSet1", detailDataList);
		}

		private static IScheduleDictionary loadSchedules(IUnitOfWork unitOfWork, IEnumerable<IPerson> persons,
			IScenario scenario, DateOnlyPeriodAsDateTimePeriod dateOnlyPeriodAsDateTimePeriod)
		{
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false, false);
			unitOfWork.Reassociate(persons);

			var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
			var personAssignmentRepository = PersonAssignmentRepository.DONT_USE_CTOR(currentUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
			var noteRepository = new NoteRepository(currentUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
			var agentDayScheduleTagRepository = AgentDayScheduleTagRepository.DONT_USE_CTOR(currentUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
			var scheduleStorage = new ScheduleStorage(currentUnitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository,
				PersonAvailabilityRepository.DONT_USE_CTOR(currentUnitOfWork),
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
				CurrentAuthorization.Make());
			return scheduleStorage.FindSchedulesForPersons(scenario, persons, scheduleDictionaryLoadOptions, dateOnlyPeriodAsDateTimePeriod.Period(), persons, true);
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
	}
}
