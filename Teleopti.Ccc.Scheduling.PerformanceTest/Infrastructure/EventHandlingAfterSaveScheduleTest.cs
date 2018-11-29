using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;


namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
{
	[Category("EventHandlingAfterSaveSchedule")]
	[IntegrationTest]
	public class EventHandlingAfterSaveScheduleTest 
	{
		public IPersonRepository Persons;
		public IBusinessUnitRepository BusinessUnits;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IScheduleStorage Schedules;
		public IScheduleDictionaryPersister Persister;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPlanningPeriodRepository PlanningPeriodRepository;

		public HangfireUtilities Hangfire;
		public TestLog TestLog;
		public IHangfireClientStarter HangfireClientStarter;

		[Test]
		public void MeasurePerformance()
		{
			const int absDayLength = 3;
			const int absEveryXDay = 40;
			
			PingWeb.Execute();
			HangfireClientStarter.Start();
			
			Guid businessUnitId;
			const string logOnDatasource = "Teleopti WFM";
			using (DataSource.OnThisThreadUse(logOnDatasource))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.GetValueOrDefault();
			AsSystem.Logon(logOnDatasource, businessUnitId);

			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				var periodPlanningPeriod = PlanningPeriodRepository.Get(AppConfigs.PlanningPeriodId).Range;
				var scenario = Scenarios.LoadDefaultScenario();
				var persons = Persons.LoadAll().Where(p => p.Period(periodPlanningPeriod.StartDate) != null).ToArray();

				schedules = Schedules.FindSchedulesForPersons(scenario, persons,
					new ScheduleDictionaryLoadOptions(false, false),
					periodPlanningPeriod.ToDateTimePeriod(TimeZoneInfo.Utc), persons, false);
				var activities = Activities.LoadAll().OrderBy(x => x.Description.Name).ToArray();
				var inWorkTimeActivities = activities.Where(x => x.InWorkTime).ToArray();
				var notInWorkTimeActivity = activities.First(x => !x.InWorkTime);
				var dayoffTempate = DayOffTemplateRepository.FindAllDayOffsSortByDescription().First();
				var absence = AbsenceRepository.LoadAllSortByName().First();

				TestLog.Debug($"Creating data for {persons.Length} people for {periodPlanningPeriod.DayCount()} dates.");
				
				foreach (var person in persons)
				{
					var day = 0;
					foreach (var date in periodPlanningPeriod.DayCollection())
					{
						var scheduledDay = schedules[person].ScheduledDay(date);

						if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
						{
							scheduledDay.CreateAndAddDayOff(dayoffTempate);
						}
						else
						{
							var assignment = new PersonAssignment(person, scenario, date)
								.WithLayer(inWorkTimeActivities[0], new TimePeriod(8, 17))
								.WithLayer(inWorkTimeActivities[1], new TimePeriod(10, 0, 10, 15))
								.WithLayer(inWorkTimeActivities[1], new TimePeriod(14, 0, 14, 15))
								.WithLayer(notInWorkTimeActivity, new TimePeriod(12, 13));
							scheduledDay.Add(assignment);
						}

						if (day % absEveryXDay == 0)
						{
							var absDate = periodPlanningPeriod.StartDate.AddDays(day);
							var startDate = TimeZoneHelper.ConvertToUtc(absDate.Date, TimeZoneInfo.Utc);
							scheduledDay.CreateAndAddAbsence(new AbsenceLayer(absence, new DateTimePeriod(startDate, startDate.AddDays(absDayLength))));
						}

						schedules.Modify(scheduledDay, new DoNothingScheduleDayChangeCallBack());
						day++;
					}
				}
			});

			Hangfire.CleanQueue();
			TestLog.Debug($"Number of succeeded jobs before persist schedule {Hangfire.SucceededFromStatistics()}");
			var hangfireQueueLogCancellationToken = new CancellationTokenSource();
			Task.Run(() =>
			{
				HangfireLogger.LogHangfireQueues(TestLog, Hangfire);
			}, hangfireQueueLogCancellationToken.Token);

			Persister.Persist(schedules);

			TestLog.Debug($"Number of succeeded jobs before Hangfire.WaitForQueue {Hangfire.SucceededFromStatistics()}");
			Hangfire.WaitForQueue();
			hangfireQueueLogCancellationToken.Cancel();
			TestLog.Debug($"Number of succeeded jobs after Hangfire.WaitForQueue {Hangfire.SucceededFromStatistics()}");
		}
	}
}