using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
{
	[TestFixture]
	[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
	[PerformanceInfrastructureTest]
	public class EventHandlingAfterSaveScheduleTest
	{
		public IPersonRepository Persons;
		public IBusinessUnitRepository BusinessUnits;
		public WithUnitOfWork WithUnitOfWork;
		public Http Http;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IScheduleStorage Schedules;
		public IScheduleDictionaryPersister Persister;

		public HangfireUtilities Hangfire;
		public TestLog TestLog;

		[Test]
		[Category("EventHandlingAfterSaveSchedule")]
		public void MeasurePerformance()
		{
			PingWeb.Execute();
			
			Guid businessUnitId;
			const string logOnDatasource = "Teleopti WFM";
			using (DataSource.OnThisThreadUse(logOnDatasource))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.GetValueOrDefault();
			AsSystem.Logon(logOnDatasource, businessUnitId);

			var today = new DateTime(2018, 3, 1);
			var dates = Enumerable.Range(0, 30)
				.Select(i => new DateOnly(today.AddDays(i)))
				.ToArray();

			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				var scenario = Scenarios.LoadDefaultScenario();
				var persons = Persons.LoadAll()
					.Where(p => p.Period(new DateOnly(today)) != null)
					.ToList();

				schedules = Schedules.FindSchedulesForPersons(scenario,
					persons,
					new ScheduleDictionaryLoadOptions(false, false),
					new DateTimePeriod(dates.Min().Utc(), dates.Max().AddDays(1).Utc()), persons, false);
				schedules.TakeSnapshot();
				var phone = Activities.LoadAll().Single(x => x.Name == "Phone");

				TestLog.Debug($"Creating data for {persons.Count} people for {dates.Length} dates.");

				persons.ForEach(person =>
				{
					dates.ForEach(date =>
					{
						var assignment = new PersonAssignment(person, scenario, date);
						var startTime = DateTime.SpecifyKind(date.Date.AddHours(8), DateTimeKind.Utc);
						var endTime = DateTime.SpecifyKind(date.Date.AddHours(17), DateTimeKind.Utc);
						assignment.AddActivity(phone, startTime, endTime);
						var scheduledDay = schedules[person].ScheduledDay(date);
						scheduledDay.Add(assignment);
						schedules.Modify(scheduledDay, new DoNothingScheduleDayChangeCallBack());
					});
				});
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