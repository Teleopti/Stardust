using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
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


namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[TestFixture]
	[PerformanceTest]
	[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
	[Category("SaveSchedulesOptimizedEventsTest")]
	public class SaveSchedulesOptimizedEventsTest
	{
		public TestConfiguration Configuration;
		public Database Database;
		public IPersonRepository Persons;
		public IBusinessUnitRepository BusinessUnits;
		public WithUnitOfWork WithUnitOfWork;
		public Http Http;
		public MutableNow Now;
		public HangfireUtilities Hangfire;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IScheduleStorage Schedules;
		public IScheduleDictionaryPersister Persister;
		public TestLog TestLog;

		[Test]
		public void MeasurePerformance()
		{
			Guid businessUnitId;
			const string logOnDatasource = "TestData";
			using (DataSource.OnThisThreadUse(logOnDatasource))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.GetValueOrDefault();
			AsSystem.Logon(logOnDatasource, businessUnitId);

			Now.Is("2016-06-01".Utc());
			Http.PostJson("/Test/SetCurrentTime", new {ticks = Now.UtcDateTime().Ticks});
			var dates = Enumerable.Range(1, Configuration.NumberOfDays)
				.Select(i => new DateOnly(Now.UtcDateTime().AddDays(i)))
				.ToArray();

			IScheduleDictionary schedules = null;
			WithUnitOfWork.Do(() =>
			{
				var scenario = Scenarios.LoadDefaultScenario();
				var persons = Persons.LoadAll()
					.Where(p => p.Period(new DateOnly(Now.UtcDateTime())) != null) // UserThatCreatesTestData has no period
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

			Persister.Persist(schedules);

			var hangfireQueueLogCancellationToken = new CancellationTokenSource();
			Task.Run(() =>
			{
				NUnitSetup.LogHangfireQueues(TestLog, Hangfire);
			}, hangfireQueueLogCancellationToken.Token);
			Hangfire.WaitForQueue();
			hangfireQueueLogCancellationToken.Cancel();
		}
		
	}
	
}