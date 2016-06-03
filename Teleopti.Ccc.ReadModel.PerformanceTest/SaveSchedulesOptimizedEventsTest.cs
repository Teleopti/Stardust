using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	[TestFixture]
	[PerformanceTest]
	[Toggle(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
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
		public IHangfireUtilities Hangfire;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IScheduleStorage Schedules;
		public IScheduleDictionaryPersister Persister;

		[Test]
		public void MeasurePerformance()
		{
			Guid businessUnitId;
			using (DataSource.OnThisThreadUse("TestData"))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			AsSystem.Logon("TestData", businessUnitId);

			Now.Is("2016-06-01".Utc());
			Http.Get("/Test/SetCurrentTime?ticks=" + Now.UtcDateTime().Ticks);
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
				schedules = Schedules.FindSchedulesForPersons(
					new DateTimePeriod(dates.Min().Utc(), dates.Max().AddDays(1).Utc()),
					scenario,
					new PersonProvider(persons),
					new ScheduleDictionaryLoadOptions(false, false),
					persons);
				schedules.TakeSnapshot();
				var phone = Activities.LoadAll().Single(x => x.Name == "Phone");

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
						schedules.Modify(scheduledDay);
					});
				});
			});

			Persister.Persist(schedules);
			
			Hangfire.WaitForQueue();
		}
	}
	
}