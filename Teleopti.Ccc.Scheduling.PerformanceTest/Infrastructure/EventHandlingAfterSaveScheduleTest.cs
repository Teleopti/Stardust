using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
{
	[Category("EventHandlingAfterSaveSchedule")]
	[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
	[InfrastructureTest]
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

		public HangfireUtilities Hangfire;
		public TestLog TestLog;

		[Test]
		[RealHangfire]
		public void MeasurePerformance()
		{
			PingWeb.Execute();
			
			Guid businessUnitId;
			const string logOnDatasource = "Teleopti WFM";
			using (DataSource.OnThisThreadUse(logOnDatasource))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.GetValueOrDefault();
			AsSystem.Logon(logOnDatasource, businessUnitId);

			var startDate = new DateOnly(2018, 3, 1);
			var period = new DateOnlyPeriod(startDate, startDate.AddDays(60));

			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				var scenario = Scenarios.LoadDefaultScenario();
				var persons = Persons.LoadAll().Where(p => p.Period(startDate) != null).ToArray();

				schedules = Schedules.FindSchedulesForPersons(scenario, persons,
					new ScheduleDictionaryLoadOptions(false, false),
					period.ToDateTimePeriod(TimeZoneInfo.Utc), persons, false);
				var activities = Activities.LoadAll().OrderBy(x => x.Description.Name).ToArray();
				var inWorkTimeActivities = activities.Where(x => x.InWorkTime).ToArray();
				var notInWorkTimeActivity = activities.First(x => !x.InWorkTime);
				var dayoffTempate = DayOffTemplateRepository.FindActivedDayOffsSortByDescription().First();
				var absence = AbsenceRepository.LoadAllSortByName().First();

				TestLog.Debug($"Creating data for {persons.Length} people for {period.DayCount()} dates.");

				foreach (var person in persons)
				{
					foreach (var date in period.DayCollection())
					{
						var scheduledDay = schedules[person].ScheduledDay(date);

						if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
						{
							scheduledDay.CreateAndAddDayOff(dayoffTempate);
						}
						else if (date.DayOfWeek == DayOfWeek.Wednesday)
						{
							scheduledDay.CreateAndAddAbsence(new AbsenceLayer(absence, date.ToDateTimePeriod(TimeZoneInfo.Utc)));
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

						schedules.Modify(scheduledDay, new DoNothingScheduleDayChangeCallBack());
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