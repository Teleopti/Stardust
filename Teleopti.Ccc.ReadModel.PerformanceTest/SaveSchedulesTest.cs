using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
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
	[Category("SaveSchedulesTest")]
	public class SaveSchedulesTest
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
		public ImpersonateSystem Impersonate;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository Assignments;
		public TestLog TestLog;

		[Test]
		public void MeasurePerformance()
		{
			Guid businessUnitId;
			const string logOnDatasource = "TestData";
			using (DataSource.OnThisThreadUse(logOnDatasource))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.GetValueOrDefault();
			Impersonate.Impersonate(logOnDatasource, businessUnitId);

			Now.Is("2016-06-01".Utc());
			var utcNow = Now.UtcDateTime();
			Http.PostJson("/Test/SetCurrentTime", new { ticks = utcNow.Ticks });
			var dates = Enumerable.Range(1, Configuration.NumberOfDays)
				.Select(i => new DateOnly(utcNow.AddDays(i))).ToList();

				WithUnitOfWork.Do(() =>
			{
				var scenario = Scenarios.LoadDefaultScenario();
				var localDateOnly = Now.ServerDate_DontUse();
				var persons = Persons.LoadAll()
					.Where(p => p.Period(localDateOnly) != null) // UserThatCreatesTestData has no period
					.ToList();

				TestLog.Debug($"Creating data for {persons.Count} people for {dates.Count} dates.");

				var phone = Activities.LoadAll().Single(x => x.Name == "Phone");
				Assignments.AddRange(persons.SelectMany(person =>
				{
					return dates.Select(date =>
					{
						var assignment = new PersonAssignment(person, scenario, date);
						var startTime = DateTime.SpecifyKind(date.Date.AddHours(8), DateTimeKind.Utc);
						var endTime = DateTime.SpecifyKind(date.Date.AddHours(17), DateTimeKind.Utc);
						assignment.AddActivity(phone, startTime, endTime);
						return assignment;
					});
				}));
			});
			TestLog.Debug($"Done creating data waiting for the process to finish.");

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